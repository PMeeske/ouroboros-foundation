namespace Ouroboros.Diagnostics;

/// <summary>
/// Represents a collected metric with metadata.
/// </summary>
public class Metric
{
    /// <summary>
    /// Gets or initializes the metric name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the metric type.
    /// </summary>
    public MetricType Type { get; init; }

    /// <summary>
    /// Gets or initializes the metric value.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Gets or initializes the metric labels for dimensional data.
    /// </summary>
    public Dictionary<string, string> Labels { get; init; } = new();

    /// <summary>
    /// Gets or initializes the timestamp when the metric was collected.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}