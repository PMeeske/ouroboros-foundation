namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Sensor descriptor in the body schema.
/// </summary>
/// <param name="Id">Sensor identifier.</param>
/// <param name="Modality">Sensor modality.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="IsActive">Whether sensor is active.</param>
/// <param name="Capabilities">Capabilities provided by this sensor.</param>
/// <param name="Properties">Additional properties.</param>
public sealed record SensorDescriptor(
    string Id,
    SensorModality Modality,
    string Name,
    bool IsActive,
    IReadOnlySet<Capability> Capabilities,
    IReadOnlyDictionary<string, object>? Properties = null)
{
    /// <summary>
    /// Creates an audio sensor descriptor.
    /// </summary>
    public static SensorDescriptor Audio(string id, string name = "Microphone") =>
        new(id, SensorModality.Audio, name, true,
            new HashSet<Capability> { Capability.Hearing });

    /// <summary>
    /// Creates a visual sensor descriptor.
    /// </summary>
    public static SensorDescriptor Visual(string id, string name = "Camera") =>
        new(id, SensorModality.Visual, name, true,
            new HashSet<Capability> { Capability.Seeing });

    /// <summary>
    /// Creates a text sensor descriptor.
    /// </summary>
    public static SensorDescriptor Text(string id, string name = "Text Input") =>
        new(id, SensorModality.Text, name, true,
            new HashSet<Capability> { Capability.Reading });
}