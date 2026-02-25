namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines a resource quota limit.
/// </summary>
public sealed record ResourceQuota
{
    /// <summary>
    /// Gets the resource name (e.g., "cpu", "memory", "storage", "requests_per_hour").
    /// </summary>
    public required string ResourceName { get; init; }

    /// <summary>
    /// Gets the maximum allowed value.
    /// </summary>
    public required double MaxValue { get; init; }

    /// <summary>
    /// Gets the current usage value.
    /// </summary>
    public double CurrentValue { get; init; } = 0.0;

    /// <summary>
    /// Gets the unit of measurement.
    /// </summary>
    public required string Unit { get; init; }

    /// <summary>
    /// Gets the time window for the quota (null means no time window).
    /// </summary>
    public TimeSpan? TimeWindow { get; init; }

    /// <summary>
    /// Gets a value indicating whether the quota is currently exceeded.
    /// </summary>
    public bool IsExceeded => CurrentValue > MaxValue;

    /// <summary>
    /// Gets the utilization percentage.
    /// </summary>
    public double UtilizationPercent => MaxValue > 0 ? (CurrentValue / MaxValue) * 100.0 : 0.0;
}