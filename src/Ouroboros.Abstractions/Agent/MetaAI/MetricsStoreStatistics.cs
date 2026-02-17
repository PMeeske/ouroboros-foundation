namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Statistics about the metrics store.
/// </summary>
public sealed record MetricsStoreStatistics(
    int TotalResources,
    int TotalExecutions,
    double OverallSuccessRate,
    double AverageLatencyMs,
    DateTime? OldestMetric,
    DateTime? NewestMetric);