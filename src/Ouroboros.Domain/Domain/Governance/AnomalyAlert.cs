namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents an anomaly alert.
/// </summary>
public sealed record AnomalyAlert
{
    /// <summary>
    /// Gets the alert identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the metric name where the anomaly was detected.
    /// </summary>
    public required string MetricName { get; init; }

    /// <summary>
    /// Gets the anomaly description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the severity level.
    /// </summary>
    public AlertSeverity Severity { get; init; } = AlertSeverity.Warning;

    /// <summary>
    /// Gets the expected value or range.
    /// </summary>
    public object? ExpectedValue { get; init; }

    /// <summary>
    /// Gets the actual observed value.
    /// </summary>
    public object? ObservedValue { get; init; }

    /// <summary>
    /// Gets when the anomaly was detected.
    /// </summary>
    public DateTime DetectedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the alert is resolved.
    /// </summary>
    public bool IsResolved { get; init; }

    /// <summary>
    /// Gets when the alert was resolved.
    /// </summary>
    public DateTime? ResolvedAt { get; init; }

    /// <summary>
    /// Gets the resolution description.
    /// </summary>
    public string? Resolution { get; init; }
}