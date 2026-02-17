namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines actions that can be taken by policies.
/// </summary>
public enum PolicyAction
{
    /// <summary>
    /// Log the policy violation.
    /// </summary>
    Log = 0,

    /// <summary>
    /// Send an alert notification.
    /// </summary>
    Alert = 1,

    /// <summary>
    /// Block the operation.
    /// </summary>
    Block = 2,

    /// <summary>
    /// Require human approval.
    /// </summary>
    RequireApproval = 3,

    /// <summary>
    /// Throttle the operation.
    /// </summary>
    Throttle = 4,

    /// <summary>
    /// Archive old data.
    /// </summary>
    Archive = 5,

    /// <summary>
    /// Compact storage.
    /// </summary>
    Compact = 6,

    /// <summary>
    /// Execute a custom action.
    /// </summary>
    Custom = 99
}