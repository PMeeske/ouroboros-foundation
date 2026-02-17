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
    Dictionary<string, double> CustomMetrics);