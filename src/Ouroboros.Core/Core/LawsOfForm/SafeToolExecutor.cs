// <copyright file="SafeToolExecutor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.LawsOfForm;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Monads;

/// <summary>
/// Executes tools with safety gates based on multiple criteria.
/// Uses AuditableDecision for full audit trail and three-valued certainty logic.
/// </summary>
public sealed class SafeToolExecutor
{
    private readonly IToolLookup toolLookup;
    private readonly List<SafetyCriterion> criteria;
    private Func<ToolCall, ExecutionContext, Task<bool>>? uncertaintyHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeToolExecutor"/> class.
    /// </summary>
    /// <param name="toolLookup">The tool lookup service for resolving tools.</param>
    public SafeToolExecutor(IToolLookup toolLookup)
    {
        this.toolLookup = toolLookup ?? throw new ArgumentNullException(nameof(toolLookup));
        this.criteria = new List<SafetyCriterion>();
    }

    /// <summary>
    /// Adds a safety criterion that must be satisfied for tool execution.
    /// </summary>
    /// <param name="name">The name of the criterion.</param>
    /// <param name="criterion">Function that evaluates the criterion returning a Form.</param>
    /// <returns>This executor for fluent chaining.</returns>
    public SafeToolExecutor AddCriterion(
        string name,
        Func<ToolCall, ExecutionContext, Form> criterion)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(criterion);

        this.criteria.Add(new SafetyCriterion(name, criterion));
        return this;
    }

    /// <summary>
    /// Configures the handler for uncertain decisions (Imaginary state).
    /// </summary>
    /// <param name="handler">Async function that returns true to approve, false to reject.</param>
    /// <returns>This executor for fluent chaining.</returns>
    public SafeToolExecutor OnUncertain(Func<ToolCall, Task<bool>> handler)
    {
        this.uncertaintyHandler = (call, _) => handler(call);
        return this;
    }

    /// <summary>
    /// Configures the handler for uncertain decisions with context.
    /// </summary>
    /// <param name="handler">Async function that returns true to approve, false to reject.</param>
    /// <returns>This executor for fluent chaining.</returns>
    public SafeToolExecutor OnUncertain(Func<ToolCall, ExecutionContext, Task<bool>> handler)
    {
        this.uncertaintyHandler = handler;
        return this;
    }

    /// <summary>
    /// Executes a tool call with full safety auditing.
    /// Returns an AuditableDecision with complete evidence trail.
    /// </summary>
    /// <param name="toolCall">The tool call to execute.</param>
    /// <param name="context">The execution context.</param>
    /// <returns>An auditable decision containing the tool result or rejection/uncertainty.</returns>
    public async Task<AuditableDecision<ToolResult>> ExecuteWithAudit(
        ToolCall toolCall,
        ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(toolCall);
        ArgumentNullException.ThrowIfNull(context);

        // Evaluate all criteria
        List<Evidence> evidence = new List<Evidence>();
        List<Form> criteriaForms = new List<Form>();

        foreach (SafetyCriterion criterion in this.criteria)
        {
            try
            {
                Form form = criterion.Evaluate(toolCall, context);
                criteriaForms.Add(form);

                string description = form.Match(
                    onMark: () => $"{criterion.Name} passed",
                    onVoid: () => $"{criterion.Name} failed",
                    onImaginary: () => $"{criterion.Name} uncertain");

                evidence.Add(new Evidence(criterion.Name, form, description));
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Treat exceptions as uncertain
                criteriaForms.Add(Form.Imaginary);
                evidence.Add(new Evidence(
                    criterion.Name,
                    Form.Imaginary,
                    $"{criterion.Name} threw exception: {ex.Message}"));
            }
        }

        // Combine all criteria using AND logic (all must pass)
        Form overallCertainty = FormExtensions.All(criteriaForms.ToArray());

        // Handle each certainty state
        return await overallCertainty.Match(
            onMark: async () => await this.ExecuteTool(toolCall, context, evidence),
            onVoid: () => Task.FromResult(RejectExecution(toolCall, evidence)),
            onImaginary: async () => await this.HandleUncertain(toolCall, context, evidence));
    }

    private async Task<AuditableDecision<ToolResult>> ExecuteTool(
        ToolCall toolCall,
        ExecutionContext context,
        List<Evidence> evidence)
    {
        DateTime startTime = DateTime.UtcNow;

        try
        {
            // Look up the tool
            Option<IToolExecutor> toolOption = this.toolLookup.GetTool(toolCall.ToolName);

            if (!toolOption.HasValue)
            {
                return AuditableDecision<ToolResult>.Reject(
                    $"Tool '{toolCall.ToolName}' not found",
                    "Tool lookup failed",
                    evidence.ToArray());
            }

            // Execute the tool
            IToolExecutor tool = toolOption.Value!;
            Result<string, string> result = await tool.InvokeAsync(toolCall.Arguments);

            TimeSpan duration = DateTime.UtcNow - startTime;

            // Convert Result to ToolResult
            ToolResult execResult = result.Match(
                onSuccess: output => ToolResult.Success(output, toolCall, duration),
                onFailure: error => ToolResult.Failure(error, toolCall, duration));

            // Record execution for rate limiting
            context.RateLimiter.Record(toolCall);

            return AuditableDecision<ToolResult>.Approve(
                execResult,
                "All safety criteria passed, tool executed successfully",
                evidence.ToArray());
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return AuditableDecision<ToolResult>.Reject(
                $"Tool execution failed: {ex.Message}",
                "Exception during tool execution",
                evidence.ToArray());
        }
    }

    private static AuditableDecision<ToolResult> RejectExecution(
        ToolCall toolCall,
        List<Evidence> evidence)
    {
        List<string> failedCriteria = evidence
            .Where(e => e.Evaluation.IsVoid())
            .Select(e => e.CriterionName)
            .ToList();

        string reasoning = $"Safety criteria failed: {string.Join(", ", failedCriteria)}";

        return AuditableDecision<ToolResult>.Reject(
            "Tool execution blocked by safety criteria",
            reasoning,
            evidence.ToArray());
    }

    private async Task<AuditableDecision<ToolResult>> HandleUncertain(
        ToolCall toolCall,
        ExecutionContext context,
        List<Evidence> evidence)
    {
        List<string> uncertainCriteria = evidence
            .Where(e => e.Evaluation.IsImaginary())
            .Select(e => e.CriterionName)
            .ToList();

        string reasoning = $"Uncertain criteria: {string.Join(", ", uncertainCriteria)}";

        if (this.uncertaintyHandler == null)
        {
            // No handler configured, escalate as uncertain
            return AuditableDecision<ToolResult>.Uncertain(
                "Tool execution requires human approval (no handler configured)",
                reasoning,
                evidence.ToArray());
        }

        try
        {
            // Invoke the uncertainty handler
            bool approved = await this.uncertaintyHandler(toolCall, context);

            if (approved)
            {
                // Add approval evidence
                evidence.Add(new Evidence(
                    "human_approval",
                    Form.Mark,
                    "Human reviewer approved execution"));

                // Execute the tool
                return await this.ExecuteTool(toolCall, context, evidence);
            }
            else
            {
                // Add rejection evidence
                evidence.Add(new Evidence(
                    "human_approval",
                    Form.Void,
                    "Human reviewer rejected execution"));

                return AuditableDecision<ToolResult>.Reject(
                    "Tool execution rejected by human reviewer",
                    reasoning + " - Human review declined",
                    evidence.ToArray());
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return AuditableDecision<ToolResult>.Uncertain(
                $"Uncertainty handler failed: {ex.Message}",
                reasoning,
                evidence.ToArray());
        }
    }

    /// <summary>
    /// Internal record for storing safety criteria.
    /// </summary>
    private sealed record SafetyCriterion
    {
        public string Name { get; init; }
        public Func<ToolCall, ExecutionContext, Form> Evaluator { get; init; }

        public SafetyCriterion(string name, Func<ToolCall, ExecutionContext, Form> evaluator)
        {
            this.Name = name;
            this.Evaluator = evaluator;
        }

        public Form Evaluate(ToolCall toolCall, ExecutionContext context)
        {
            return this.Evaluator(toolCall, context);
        }
    }
}
