using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Text input perception.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record TextPerception(
    Guid Id,
    DateTime Timestamp,
    double Confidence,
    string Text,
    string? Source) : PerceptionEvent(Id, SensorModality.Text, Timestamp, Confidence);
