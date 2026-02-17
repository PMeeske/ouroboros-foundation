namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Represents a valence signal measurement.
/// </summary>
public sealed record ValenceSignal(
    string Source,
    double Value,
    SignalType Type,
    DateTime Timestamp,
    TimeSpan? Duration);