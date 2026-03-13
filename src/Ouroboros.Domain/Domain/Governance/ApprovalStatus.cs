namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines approval request statuses.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Waiting for approval.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Rejected.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Expired/timed out.
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 4
}