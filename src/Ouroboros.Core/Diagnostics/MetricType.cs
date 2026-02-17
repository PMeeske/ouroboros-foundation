namespace Ouroboros.Diagnostics;

/// <summary>
/// Metric type enumeration.
/// </summary>
public enum MetricType
{
    /// <summary>
    /// Counter metric that only increases.
    /// </summary>
    Counter,

    /// <summary>
    /// Gauge metric that can increase or decrease.
    /// </summary>
    Gauge,

    /// <summary>
    /// Histogram metric for value distributions.
    /// </summary>
    Histogram,

    /// <summary>
    /// Summary metric for statistical observations.
    /// </summary>
    Summary,
}