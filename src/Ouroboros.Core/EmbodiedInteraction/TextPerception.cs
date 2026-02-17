namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Text input perception.
/// </summary>
public sealed record TextPerception(
    Guid Id,
    DateTime Timestamp,
    double Confidence,
    string Text,
    string? Source) : PerceptionEvent(Id, SensorModality.Text, Timestamp, Confidence);