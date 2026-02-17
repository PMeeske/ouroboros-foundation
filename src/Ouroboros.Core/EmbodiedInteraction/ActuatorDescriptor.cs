namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Actuator descriptor in the body schema.
/// </summary>
/// <param name="Id">Actuator identifier.</param>
/// <param name="Modality">Output modality.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="IsActive">Whether actuator is active.</param>
/// <param name="Capabilities">Capabilities provided by this actuator.</param>
/// <param name="Properties">Additional properties.</param>
public sealed record ActuatorDescriptor(
    string Id,
    ActuatorModality Modality,
    string Name,
    bool IsActive,
    IReadOnlySet<Capability> Capabilities,
    IReadOnlyDictionary<string, object>? Properties = null)
{
    /// <summary>
    /// Creates a voice actuator descriptor.
    /// </summary>
    public static ActuatorDescriptor Voice(string id, string name = "Voice Output") =>
        new(id, ActuatorModality.Voice, name, true,
            new HashSet<Capability> { Capability.Speaking });

    /// <summary>
    /// Creates a text actuator descriptor.
    /// </summary>
    public static ActuatorDescriptor Text(string id, string name = "Text Output") =>
        new(id, ActuatorModality.Text, name, true,
            new HashSet<Capability> { Capability.Writing });
}