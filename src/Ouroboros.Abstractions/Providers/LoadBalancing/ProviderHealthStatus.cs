namespace Ouroboros.Providers.LoadBalancing;

/// <summary>
/// Health status of a provider including performance metrics and availability.
/// </summary>
public sealed record ProviderHealthStatus(
    string ProviderId,
    bool IsHealthy,
    double SuccessRate,
    double AverageLatencyMs,
    int ConsecutiveFailures,
    DateTime? LastFailureTime,
    DateTime? CooldownUntil,
    int TotalRequests,
    int SuccessfulRequests,
    DateTime LastChecked)
{
    /// <summary>
    /// Gets a value indicating whether the provider is in cooldown (rate limited).
    /// </summary>
    public bool IsInCooldown => CooldownUntil.HasValue && CooldownUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Calculates composite health score (0.0 to 1.0).
    /// </summary>
    public double HealthScore
    {
        get
        {
            if (!IsHealthy || IsInCooldown) return 0.0;

            // Weight success rate more heavily, normalize latency
            double latencyScore = Math.Max(0, 1.0 - (AverageLatencyMs / 10000.0));
            return (SuccessRate * 0.7) + (latencyScore * 0.3);
        }
    }
}