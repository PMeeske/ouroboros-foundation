namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Multimodal perception event from any sensor.
/// </summary>
public abstract record PerceptionEvent(
    Guid Id,
    SensorModality Modality,
    DateTime Timestamp,
    double Confidence);