namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents an approval request.
/// </summary>
public sealed record ApprovalRequest
{
    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the approval gate that triggered this request.
    /// </summary>
    public required ApprovalGate Gate { get; init; }

    /// <summary>
    /// Gets the operation description requiring approval.
    /// </summary>
    public required string OperationDescription { get; init; }

    /// <summary>
    /// Gets the context information for the approval decision.
    /// </summary>
    public IReadOnlyDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the status of the approval request.
    /// </summary>
    public ApprovalStatus Status { get; init; } = ApprovalStatus.Pending;

    /// <summary>
    /// Gets the approvals received.
    /// </summary>
    public IReadOnlyList<Approval> Approvals { get; init; } = Array.Empty<Approval>();

    /// <summary>
    /// Gets the timestamp when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the deadline for approval.
    /// </summary>
    public DateTime Deadline { get; init; }

    /// <summary>
    /// Gets a value indicating whether the request has met approval requirements.
    /// </summary>
    public bool IsApproved => Status == ApprovalStatus.Approved &&
                              Approvals.Count(a => a.Decision == ApprovalDecision.Approve) >= Gate.MinimumApprovals;

    /// <summary>
    /// Gets a value indicating whether the request is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > Deadline;
}