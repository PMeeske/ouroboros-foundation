// <copyright file="MaintenanceScheduler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace Ouroboros.Domain.Governance;

/// <summary>
/// Scheduled maintenance system for DAG compaction, archiving, and anomaly detection.
/// Phase 5: Governance, Safety, and Ops.
/// </summary>
public sealed class MaintenanceScheduler : IMaintenanceScheduler
{
    private readonly ConcurrentBag<MaintenanceTask> _tasks = new();
    private readonly ConcurrentBag<MaintenanceExecution> _history = new();
    private readonly ConcurrentBag<AnomalyAlert> _alerts = new();
    private CancellationTokenSource? _schedulerCts;
    private Task? _schedulerTask;

    /// <summary>
    /// Schedules a maintenance task.
    /// </summary>
    public Result<MaintenanceTask> ScheduleTask(MaintenanceTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        _tasks.Add(task);
        return Result<MaintenanceTask>.Success(task);
    }

    /// <summary>
    /// Starts the maintenance scheduler.
    /// </summary>
    public Result<bool> Start()
    {
        if (_schedulerTask != null && !_schedulerTask.IsCompleted)
        {
            return Result<bool>.Failure("Scheduler is already running");
        }

        _schedulerCts?.Dispose(); // Dispose any previous instance
        _schedulerCts = new CancellationTokenSource();
        _schedulerTask = RunSchedulerAsync(_schedulerCts.Token);
        
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Stops the maintenance scheduler.
    /// </summary>
    public async Task<Result<bool>> StopAsync()
    {
        if (_schedulerCts == null || _schedulerTask == null)
        {
            return Result<bool>.Failure("Scheduler is not running");
        }

        _schedulerCts.Cancel();
        
        try
        {
            await _schedulerTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        _schedulerCts.Dispose();
        _schedulerCts = null;
        _schedulerTask = null;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Executes a maintenance task immediately.
    /// </summary>
    public async Task<Result<MaintenanceExecution>> ExecuteTaskAsync(
        MaintenanceTask task,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        var execution = new MaintenanceExecution
        {
            Task = task,
            StartedAt = DateTime.UtcNow,
            Status = MaintenanceStatus.Running
        };

        try
        {
            var result = await task.Execute(ct);
            
            execution = execution with
            {
                CompletedAt = DateTime.UtcNow,
                Status = result.IsSuccess ? MaintenanceStatus.Completed : MaintenanceStatus.Failed,
                ResultMessage = result.IsSuccess ? "Success" : result.Error,
                Metadata = result.IsSuccess ? new Dictionary<string, object> { ["result"] = result.Value } : new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            execution = execution with
            {
                CompletedAt = DateTime.UtcNow,
                Status = MaintenanceStatus.Failed,
                ResultMessage = ex.Message
            };
        }

        _history.Add(execution);
        return Result<MaintenanceExecution>.Success(execution);
    }

    /// <summary>
    /// Gets maintenance task execution history.
    /// </summary>
    public IReadOnlyList<MaintenanceExecution> GetHistory(int limit = 50)
    {
        return _history
            .OrderByDescending(e => e.StartedAt)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Gets anomaly alerts.
    /// </summary>
    public IReadOnlyList<AnomalyAlert> GetAlerts(bool unresolvedOnly = true)
    {
        var alerts = _alerts.AsEnumerable();
        if (unresolvedOnly)
        {
            alerts = alerts.Where(a => !a.IsResolved);
        }
        return alerts.OrderByDescending(a => a.DetectedAt).ToList();
    }

    /// <summary>
    /// Creates an anomaly alert.
    /// </summary>
    public Result<AnomalyAlert> CreateAlert(AnomalyAlert alert)
    {
        ArgumentNullException.ThrowIfNull(alert);
        _alerts.Add(alert);
        return Result<AnomalyAlert>.Success(alert);
    }

    /// <summary>
    /// Resolves an anomaly alert.
    /// </summary>
    public Result<AnomalyAlert> ResolveAlert(Guid alertId, string resolution)
    {
        var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
        if (alert == null)
        {
            return Result<AnomalyAlert>.Failure($"Alert {alertId} not found");
        }

        // Since ConcurrentBag doesn't support updates, we add a resolved copy
        var resolved = alert with
        {
            IsResolved = true,
            ResolvedAt = DateTime.UtcNow,
            Resolution = resolution
        };

        _alerts.Add(resolved);
        return Result<AnomalyAlert>.Success(resolved);
    }

    private async Task RunSchedulerAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                foreach (var task in _tasks.Where(t => t.IsEnabled))
                {
                    if (ShouldExecute(task, now))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await ExecuteTaskAsync(task, ct);
                            }
                            catch (Exception)
                            {
                                // Logged in ExecuteTaskAsync
                            }
                        }, ct);
                    }
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private bool ShouldExecute(MaintenanceTask task, DateTime now)
    {
        var lastExecution = _history
            .Where(e => e.Task.Id == task.Id)
            .OrderByDescending(e => e.StartedAt)
            .FirstOrDefault();

        if (lastExecution == null)
        {
            return true; // Never executed
        }

        var timeSinceLastExecution = now - lastExecution.StartedAt;
        return timeSinceLastExecution >= task.Schedule;
    }

    /// <summary>
    /// Creates a compaction task for DAG snapshots.
    /// </summary>
    public static MaintenanceTask CreateCompactionTask(
        string name,
        TimeSpan schedule,
        Func<CancellationToken, Task<Result<CompactionResult>>> compactor)
    {
        return new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Compacts DAG snapshots to reduce storage",
            TaskType = MaintenanceTaskType.Compaction,
            Schedule = schedule,
            IsEnabled = true,
            Execute = async ct =>
            {
                var result = await compactor(ct);
                return result.IsSuccess
                    ? Result<object>.Success(result.Value)
                    : Result<object>.Failure(result.Error);
            }
        };
    }

    /// <summary>
    /// Creates an archiving task for old snapshots.
    /// </summary>
    public static MaintenanceTask CreateArchivingTask(
        string name,
        TimeSpan schedule,
        TimeSpan archiveAge,
        Func<TimeSpan, CancellationToken, Task<Result<ArchiveResult>>> archiver)
    {
        return new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Archives snapshots older than {archiveAge.TotalDays} days",
            TaskType = MaintenanceTaskType.Archiving,
            Schedule = schedule,
            IsEnabled = true,
            Execute = async ct =>
            {
                var result = await archiver(archiveAge, ct);
                return result.IsSuccess
                    ? Result<object>.Success(result.Value)
                    : Result<object>.Failure(result.Error);
            }
        };
    }

    /// <summary>
    /// Creates an anomaly detection task.
    /// </summary>
    public static MaintenanceTask CreateAnomalyDetectionTask(
        string name,
        TimeSpan schedule,
        Func<CancellationToken, Task<Result<AnomalyDetectionResult>>> detector)
    {
        return new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Detects anomalies in system metrics",
            TaskType = MaintenanceTaskType.AnomalyDetection,
            Schedule = schedule,
            IsEnabled = true,
            Execute = async ct =>
            {
                var result = await detector(ct);
                return result.IsSuccess
                    ? Result<object>.Success(result.Value)
                    : Result<object>.Failure(result.Error);
            }
        };
    }
}