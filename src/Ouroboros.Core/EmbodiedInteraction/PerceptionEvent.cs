using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Multimodal perception event from any sensor.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract record PerceptionEvent(
    Guid Id,
    SensorModality Modality,
    DateTime Timestamp,
    double Confidence);
