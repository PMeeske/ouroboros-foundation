namespace Ouroboros.Agent.MetaAI.SelfModel;

/// <summary>
/// Represents a forecast made by the agent.
/// </summary>
public sealed record Forecast(
    Guid Id,
    string Description,
    string MetricName,
    double PredictedValue,
    double Confidence,
    DateTime PredictionTime,
    DateTime TargetTime,
    ForecastStatus Status,
    double? ActualValue,
    Dictionary<string, object> Metadata);