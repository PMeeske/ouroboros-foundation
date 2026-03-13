namespace Ouroboros.Domain.Governance;

/// <summary>
/// Maintenance execution status.
/// </summary>
public enum MaintenanceStatus
{
    /// <summary>
    /// Task is running.
    /// </summary>
    Running = 0,

    /// <summary>
    /// Task completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Task failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Task was cancelled.
    /// </summary>
    Cancelled = 3
}