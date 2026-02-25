namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Status of an intention.
/// </summary>
public enum IntentionStatus
{
    /// <summary>Waiting for user decision.</summary>
    Pending,

    /// <summary>User approved, ready for execution.</summary>
    Approved,

    /// <summary>User rejected this intention.</summary>
    Rejected,

    /// <summary>Currently being executed.</summary>
    Executing,

    /// <summary>Successfully executed.</summary>
    Completed,

    /// <summary>Execution failed.</summary>
    Failed,

    /// <summary>Intention expired before being acted upon.</summary>
    Expired,

    /// <summary>Cancelled by the system or user.</summary>
    Cancelled,
}