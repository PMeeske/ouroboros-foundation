namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents a scheduled maintenance task.
/// </summary>
public sealed record MaintenanceTask
{
    /// <summary>
    /// Gets the task identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the task name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the task description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the task type.
    /// </summary>
    public required MaintenanceTaskType TaskType { get; init; }

    /// <summary>
    /// Gets the schedule (how often to run).
    /// </summary>
    public required TimeSpan Schedule { get; init; }

    /// <summary>
    /// Gets a value indicating whether the task is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the task execution function.
    /// </summary>
    public required Func<CancellationToken, Task<Result<object>>> Execute { get; init; }
}