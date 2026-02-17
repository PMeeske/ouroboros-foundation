namespace Ouroboros.Domain.Governance;

/// <summary>
/// Result of anomaly detection.
/// </summary>
public sealed record AnomalyDetectionResult
{
    /// <summary>
    /// Gets the anomalies detected.
    /// </summary>
    public IReadOnlyList<AnomalyAlert> Anomalies { get; init; } = Array.Empty<AnomalyAlert>();

    /// <summary>
    /// Gets the timestamp of the detection.
    /// </summary>
    public DateTime DetectedAt { get; init; } = DateTime.UtcNow;
}