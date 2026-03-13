// <copyright file="ImmutableEthicsFramework.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Ethics.MeTTa;

namespace Ouroboros.Core.Ethics;

/// <summary>
/// Immutable implementation of the Ethics Framework.
/// All methods are sealed and cannot be overridden.
/// Core principles cannot be modified at runtime.
/// This class provides the foundational ethical evaluation for the Ouroboros system.
/// </summary>
public sealed partial class ImmutableEthicsFramework : IEthicsFramework
{
    private readonly IReadOnlyList<EthicalPrinciple> _corePrinciples;
    private readonly IEthicsAuditLog _auditLog;
    private readonly IEthicalReasoner _reasoner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableEthicsFramework"/> class.
    /// Constructor is internal to enforce creation through factory.
    /// </summary>
    /// <param name="auditLog">The audit log for recording evaluations.</param>
    /// <param name="reasoner">The ethical reasoning component.</param>
    internal ImmutableEthicsFramework(IEthicsAuditLog auditLog, IEthicalReasoner reasoner)
    {
        ArgumentNullException.ThrowIfNull(auditLog);
        ArgumentNullException.ThrowIfNull(reasoner);

        // Verify ethical atom integrity before anything else.
        // If the MeTTa foundation has been tampered with, refuse to initialize.
        EthicalAtomIntegrity.VerifyAll();

        _auditLog = auditLog;
        _reasoner = reasoner;
        _corePrinciples = EthicalPrinciple.GetCorePrinciples();
    }

    /// <inheritdoc/>
    public IReadOnlyList<EthicalPrinciple> GetCorePrinciples()
    {
        // Return an array copy to prevent modification
        return _corePrinciples.ToArray();
    }

    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluateActionAsync(
        ProposedAction action,
        ActionContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Analyze the action against ethical principles
            (IReadOnlyList<EthicalViolation>? violations, IReadOnlyList<EthicalConcern>? concerns) = _reasoner.AnalyzeAction(action, context, _corePrinciples);

            EthicalClearance clearance;

            if (violations.Count > 0)
            {
                // Action violates ethical principles - deny
                clearance = EthicalClearance.Denied(
                    $"Action '{action.ActionType}' violates ethical principles",
                    violations,
                    _corePrinciples);
            }
            else if (_reasoner.RequiresHumanApproval(action, context))
            {
                // High-risk action requires human approval
                clearance = EthicalClearance.RequiresApproval(
                    $"Action '{action.ActionType}' requires human approval due to high risk",
                    concerns,
                    _corePrinciples);
            }
            else if (concerns.Count > 0)
            {
                // Action has concerns but is permitted
                clearance = new EthicalClearance
                {
                    IsPermitted = true,
                    Level = EthicalClearanceLevel.PermittedWithConcerns,
                    RelevantPrinciples = _corePrinciples,
                    Violations = Array.Empty<EthicalViolation>(),
                    Concerns = concerns,
                    Reasoning = $"Action '{action.ActionType}' is permitted with {concerns.Count} concern(s)"
                };
            }
            else
            {
                // Action is permitted
                clearance = EthicalClearance.Permitted(
                    $"Action '{action.ActionType}' complies with ethical principles",
                    _corePrinciples);
            }

            // Log the evaluation
            await LogEvaluationAsync("Action", action.Description, context, clearance, ct).ConfigureAwait(false);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Ethics evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Ethics evaluation failed: {ex.Message}");
        }
    }


    private async Task LogEvaluationAsync(
        string evaluationType,
        string description,
        ActionContext context,
        EthicalClearance clearance,
        CancellationToken ct)
    {
        EthicsAuditEntry entry = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = context.AgentId,
            UserId = context.UserId,
            EvaluationType = evaluationType,
            Description = description,
            Clearance = clearance,
            Context = new Dictionary<string, object>
            {
                ["Environment"] = context.Environment,
                ["ClearanceLevel"] = clearance.Level.ToString()
            }
        };

        await _auditLog.LogEvaluationAsync(entry, ct).ConfigureAwait(false);
    }
}
