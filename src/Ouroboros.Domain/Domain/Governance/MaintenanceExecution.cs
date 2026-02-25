namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents a maintenance task execution.
/// </summary>
public sealed record MaintenanceExecution
{
    /// <summary>
    /// Gets the task that was executed.
    /// </summary>
    public required MaintenanceTask Task { get; init; }

    /// <summary>
    /// Gets when the execution started.
    /// </summary>
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets when the execution completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Gets the execution status.
    /// </summary>
    public MaintenanceStatus Status { get; init; }

    /// <summary>
    /// Gets the result message.
    /// </summary>
    public string? ResultMessage { get; init; }

    /// <summary>
    /// Gets additional metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}