namespace Ouroboros.Domain.Governance;

/// <summary>
/// Result of policy enforcement.
/// </summary>
public sealed record PolicyEnforcementResult
{
    /// <summary>
    /// Gets the policy evaluations.
    /// </summary>
    public required IReadOnlyList<PolicyEvaluationResult> Evaluations { get; init; }

    /// <summary>
    /// Gets the actions that should be taken.
    /// </summary>
    public IReadOnlyList<PolicyAction> ActionsRequired { get; init; } = Array.Empty<PolicyAction>();

    /// <summary>
    /// Gets the approval requests that were created.
    /// </summary>
    public IReadOnlyList<ApprovalRequest> ApprovalsRequired { get; init; } = Array.Empty<ApprovalRequest>();

    /// <summary>
    /// Gets a value indicating whether the operation is blocked.
    /// </summary>
    public bool IsBlocked { get; init; }

    /// <summary>
    /// Gets the timestamp when enforcement was performed.
    /// </summary>
    public DateTime EnforcedAt { get; init; } = DateTime.UtcNow;
}