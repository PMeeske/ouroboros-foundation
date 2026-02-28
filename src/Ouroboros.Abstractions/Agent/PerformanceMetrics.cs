namespace Ouroboros.Agent;

/// <summary>
/// Performance metrics for model/tool execution tracking.
/// </summary>
public sealed record PerformanceMetrics(
    string ResourceName,
    int ExecutionCount,
    double AverageLatencyMs,
    double SuccessRate,
    DateTime LastUsed,
    Dictionary<string, double> CustomMetrics)
{
    /// <summary>
    /// Creates initial metrics with zero values for a new resource.
    /// </summary>
    public static PerformanceMetrics Initial(string resourceName) =>
        new(
            resourceName ?? throw new ArgumentNullException(nameof(resourceName)),
            ExecutionCount: 0,
            AverageLatencyMs: 0.0,
            SuccessRate: 0.0,
            LastUsed: DateTime.UtcNow,
            CustomMetrics: new Dictionary<string, double>());
}
