namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines actions to take when approval times out.
/// </summary>
public enum ApprovalTimeoutAction
{
    /// <summary>
    /// Block the operation.
    /// </summary>
    Block = 0,

    /// <summary>
    /// Escalate to higher authority.
    /// </summary>
    Escalate = 1,

    /// <summary>
    /// Auto-approve with reduced privileges.
    /// </summary>
    AutoApproveReduced = 2,

    /// <summary>
    /// Auto-reject.
    /// </summary>
    AutoReject = 3
}