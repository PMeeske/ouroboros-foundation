namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines approval decisions.
/// </summary>
public enum ApprovalDecision
{
    /// <summary>
    /// Approve the request.
    /// </summary>
    Approve = 0,

    /// <summary>
    /// Reject the request.
    /// </summary>
    Reject = 1,

    /// <summary>
    /// Request more information.
    /// </summary>
    RequestInfo = 2
}