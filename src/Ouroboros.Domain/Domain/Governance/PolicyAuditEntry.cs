namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents an audit trail entry for policy enforcement.
/// </summary>
public sealed record PolicyAuditEntry
{
    /// <summary>
    /// Gets the entry identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the policy that was evaluated or enforced.
    /// </summary>
    public required Policy Policy { get; init; }

    /// <summary>
    /// Gets the action that was taken.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the actor who triggered the audit entry (user, system, etc.).
    /// </summary>
    public required string Actor { get; init; }

    /// <summary>
    /// Gets the evaluation result if applicable.
    /// </summary>
    public PolicyEvaluationResult? EvaluationResult { get; init; }

    /// <summary>
    /// Gets the approval request if applicable.
    /// </summary>
    public ApprovalRequest? ApprovalRequest { get; init; }

    /// <summary>
    /// Gets the timestamp of the audit entry.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets additional metadata for the audit entry.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}