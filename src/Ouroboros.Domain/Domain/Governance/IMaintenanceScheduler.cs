namespace Ouroboros.Domain.Governance;

/// <summary>
/// Interface for maintenance scheduler.
/// </summary>
public interface IMaintenanceScheduler
{
    /// <summary>
    /// Schedules a maintenance task.
    /// </summary>
    Result<MaintenanceTask> ScheduleTask(MaintenanceTask task);

    /// <summary>
    /// Starts the scheduler.
    /// </summary>
    Result<bool> Start();

    /// <summary>
    /// Stops the scheduler.
    /// </summary>
    Task<Result<bool>> StopAsync();

    /// <summary>
    /// Executes a task immediately.
    /// </summary>
    Task<Result<MaintenanceExecution>> ExecuteTaskAsync(MaintenanceTask task, CancellationToken ct = default);

    /// <summary>
    /// Gets execution history.
    /// </summary>
    IReadOnlyList<MaintenanceExecution> GetHistory(int limit = 50);

    /// <summary>
    /// Gets anomaly alerts.
    /// </summary>
    IReadOnlyList<AnomalyAlert> GetAlerts(bool unresolvedOnly = true);

    /// <summary>
    /// Creates an anomaly alert.
    /// </summary>
    Result<AnomalyAlert> CreateAlert(AnomalyAlert alert);

    /// <summary>
    /// Resolves an anomaly alert.
    /// </summary>
    Result<AnomalyAlert> ResolveAlert(Guid alertId, string resolution);
}