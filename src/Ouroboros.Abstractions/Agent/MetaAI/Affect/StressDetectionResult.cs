namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Result of stress detection using signal analysis.
/// </summary>
public sealed record StressDetectionResult(
    double StressLevel,
    double Frequency,
    double Amplitude,
    bool IsAnomalous,
    List<double> SpectralPeaks,
    string Analysis,
    DateTime DetectedAt);