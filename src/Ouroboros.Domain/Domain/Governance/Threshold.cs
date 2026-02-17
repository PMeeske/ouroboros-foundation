namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines a threshold that triggers policy actions.
/// </summary>
public sealed record Threshold
{
    /// <summary>
    /// Gets the metric name being monitored.
    /// </summary>
    public required string MetricName { get; init; }

    /// <summary>
    /// Gets the lower bound (violations occur below this value).
    /// </summary>
    public double? LowerBound { get; init; }

    /// <summary>
    /// Gets the upper bound (violations occur above this value).
    /// </summary>
    public double? UpperBound { get; init; }

    /// <summary>
    /// Gets the action to take when threshold is violated.
    /// </summary>
    public required PolicyAction Action { get; init; }

    /// <summary>
    /// Gets the severity level of threshold violations.
    /// </summary>
    public ThresholdSeverity Severity { get; init; } = ThresholdSeverity.Warning;
}