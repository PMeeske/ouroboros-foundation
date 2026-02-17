namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Information about a sensor from the provider.
/// </summary>
/// <param name="SensorId">Unique sensor identifier.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="Modality">Sensor modality type.</param>
/// <param name="IsActive">Whether the sensor is currently active.</param>
/// <param name="Capabilities">Capabilities this sensor provides.</param>
/// <param name="Properties">Additional sensor properties.</param>
public sealed record SensorInfo(
    string SensorId,
    string Name,
    SensorModality Modality,
    bool IsActive,
    EmbodimentCapabilities Capabilities,
    IReadOnlyDictionary<string, object>? Properties = null);