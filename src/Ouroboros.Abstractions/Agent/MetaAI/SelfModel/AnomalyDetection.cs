namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Anomaly detection result.
/// </summary>
public sealed record AnomalyDetection(
    string MetricName,
    double ObservedValue,
    double ExpectedValue,
    double Deviation,
    bool IsAnomaly,
    string Severity,
    DateTime DetectedAt,
    List<string> PossibleCauses);