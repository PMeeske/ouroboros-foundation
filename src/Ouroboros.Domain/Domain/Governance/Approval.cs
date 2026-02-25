namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents a single approval from an approver.
/// </summary>
public sealed record Approval
{
    /// <summary>
    /// Gets the approver identifier (user ID or role).
    /// </summary>
    public required string ApproverId { get; init; }

    /// <summary>
    /// Gets the approval decision.
    /// </summary>
    public required ApprovalDecision Decision { get; init; }

    /// <summary>
    /// Gets optional comments from the approver.
    /// </summary>
    public string? Comments { get; init; }

    /// <summary>
    /// Gets the timestamp when the approval was given.
    /// </summary>
    public DateTime ApprovedAt { get; init; } = DateTime.UtcNow;
}