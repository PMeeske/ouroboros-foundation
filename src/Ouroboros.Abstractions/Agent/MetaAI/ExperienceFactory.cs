// <copyright file="ExperienceFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Factory methods for creating Experience records.
/// </summary>
public static class ExperienceFactory
{
    /// <summary>
    /// Creates an Experience from a plan execution result.
    /// </summary>
    /// <param name="goal">The goal that was pursued.</param>
    /// <param name="execution">The execution result.</param>
    /// <param name="verification">The verification result.</param>
    /// <param name="tags">Optional tags for categorization.</param>
    /// <param name="metadata">Optional additional metadata.</param>
    /// <returns>A new Experience record.</returns>
    public static Experience FromExecution(
        string goal,
        PlanExecutionResult execution,
        PlanVerificationResult verification,
        IReadOnlyList<string>? tags = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(execution);
        ArgumentNullException.ThrowIfNull(verification);

        return new Experience(
            Id: Guid.NewGuid().ToString(),
            Timestamp: DateTime.UtcNow,
            Context: goal,
            Action: execution.Plan.Steps.FirstOrDefault()?.Action ?? "unknown",
            Outcome: execution.FinalOutput ?? (execution.Success ? "success" : "failure"),
            Success: execution.Success,
            Tags: tags ?? ExtractTags(goal),
            Goal: goal,
            Execution: execution,
            Verification: verification,
            Plan: execution.Plan,
            Metadata: metadata);
    }

    /// <summary>
    /// Creates a simple Experience without full execution details.
    /// </summary>
    /// <param name="goal">The goal that was pursued.</param>
    /// <param name="action">The action taken.</param>
    /// <param name="outcome">The outcome of the action.</param>
    /// <param name="success">Whether the action was successful.</param>
    /// <param name="tags">Optional tags for categorization.</param>
    /// <returns>A new Experience record.</returns>
    public static Experience Simple(
        string goal,
        string action,
        string outcome,
        bool success,
        IReadOnlyList<string>? tags = null)
    {
        var plan = new Plan(goal, new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var stepResult = new StepResult(
            new PlanStep(action, new Dictionary<string, object>(), outcome, success ? 1.0 : 0.0),
            success, outcome, null, TimeSpan.Zero, new Dictionary<string, object>());
        var execution = new PlanExecutionResult(
            plan, new List<StepResult> { stepResult }, success, outcome,
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, success, success ? 1.0 : 0.0,
            new List<string>(), new List<string>(), DateTime.UtcNow);

        return new Experience(
            Id: Guid.NewGuid().ToString(),
            Timestamp: DateTime.UtcNow,
            Context: goal,
            Action: action,
            Outcome: outcome,
            Success: success,
            Tags: tags ?? ExtractTags(goal),
            Goal: goal,
            Execution: execution,
            Verification: verification,
            Plan: plan,
            Metadata: null);
    }

    private static IReadOnlyList<string> ExtractTags(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        return text.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', ':', ';', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Distinct()
            .Take(10)
            .ToList();
    }
}