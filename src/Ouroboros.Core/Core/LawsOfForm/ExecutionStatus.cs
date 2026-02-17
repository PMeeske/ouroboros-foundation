namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents the execution status of a tool.
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// Execution succeeded.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Execution failed.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Execution was blocked by safety checks.
    /// </summary>
    Blocked = 2,

    /// <summary>
    /// Execution requires human approval.
    /// </summary>
    PendingApproval = 3
}