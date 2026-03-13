using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Calibration metrics for forecast accuracy.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ForecastCalibration(
    int TotalForecasts,
    int VerifiedForecasts,
    int FailedForecasts,
    double AverageConfidence,
    double AverageAccuracy,
    double BrierScore,
    double CalibrationError,
    Dictionary<string, double> MetricAccuracies);
