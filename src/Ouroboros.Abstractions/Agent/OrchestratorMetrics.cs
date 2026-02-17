namespace Ouroboros.Agent;

/// <summary>
/// Unified metrics for orchestrator performance tracking.
/// </summary>
public sealed record OrchestratorMetrics(
    string OrchestratorName,
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    double AverageLatencyMs,
    double SuccessRate,
    DateTime LastExecuted,
    Dictionary<string, double> CustomMetrics)
{
    /// <summary>
    /// Calculates success rate.
    /// </summary>
    public double CalculatedSuccessRate =>
        TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions : 0.0;

    /// <summary>
    /// Gets a custom metric or returns default.
    /// </summary>
    public double GetCustomMetric(string key, double defaultValue = 0.0) =>
        CustomMetrics.TryGetValue(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Creates initial metrics for a new orchestrator.
    /// </summary>
    public static OrchestratorMetrics Initial(string orchestratorName) =>
        new OrchestratorMetrics(
            orchestratorName,
            TotalExecutions: 0,
            SuccessfulExecutions: 0,
            FailedExecutions: 0,
            AverageLatencyMs: 0.0,
            SuccessRate: 0.0,
            LastExecuted: DateTime.UtcNow,
            CustomMetrics: new Dictionary<string, double>());

    /// <summary>
    /// Records a new execution result.
    /// </summary>
    public OrchestratorMetrics RecordExecution(double latencyMs, bool success)
    {
        int newTotal = TotalExecutions + 1;
        int newSuccess = SuccessfulExecutions + (success ? 1 : 0);
        int newFailed = FailedExecutions + (success ? 0 : 1);
        double newAvgLatency = ((AverageLatencyMs * TotalExecutions) + latencyMs) / newTotal;
        double newSuccessRate = (double)newSuccess / newTotal;

        return this with
        {
            TotalExecutions = newTotal,
            SuccessfulExecutions = newSuccess,
            FailedExecutions = newFailed,
            AverageLatencyMs = newAvgLatency,
            SuccessRate = newSuccessRate,
            LastExecuted = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds or updates a custom metric.
    /// </summary>
    public OrchestratorMetrics WithCustomMetric(string key, double value)
    {
        var newCustomMetrics = new Dictionary<string, double>(CustomMetrics)
        {
            [key] = value
        };
        return this with { CustomMetrics = newCustomMetrics };
    }
}