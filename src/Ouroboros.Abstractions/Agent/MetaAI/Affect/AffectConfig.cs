namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Configuration for affect monitoring.
/// </summary>
public sealed record AffectConfig(
    double StressThreshold = 0.7,
    double ConfidenceDecayRate = 0.01,
    double CuriosityBoostFactor = 0.2,
    int SignalHistorySize = 1000,
    int FourierWindowSize = 64);