namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Unified perception event from any sensor.
/// </summary>
/// <param name="Source">Source sensor ID.</param>
/// <param name="Modality">Sensor modality.</param>
/// <param name="Perception">The perception event.</param>
/// <param name="Timestamp">When perceived.</param>
public sealed record UnifiedPerception(
    string Source,
    SensorModality Modality,
    PerceptionEvent Perception,
    DateTime Timestamp);