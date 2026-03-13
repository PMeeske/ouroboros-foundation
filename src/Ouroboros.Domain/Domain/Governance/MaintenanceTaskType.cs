namespace Ouroboros.Domain.Governance;

/// <summary>
/// Maintenance task types.
/// </summary>
public enum MaintenanceTaskType
{
    /// <summary>
    /// Compaction task.
    /// </summary>
    Compaction = 0,

    /// <summary>
    /// Archiving task.
    /// </summary>
    Archiving = 1,

    /// <summary>
    /// Anomaly detection task.
    /// </summary>
    AnomalyDetection = 2,

    /// <summary>
    /// Custom task.
    /// </summary>
    Custom = 99
}