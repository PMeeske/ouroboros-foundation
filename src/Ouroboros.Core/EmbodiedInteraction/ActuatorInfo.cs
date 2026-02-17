namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Information about an actuator from the provider.
/// </summary>
/// <param name="ActuatorId">Unique actuator identifier.</param>
/// <param name="Name">Human-readable name.</param>
/// <param name="Modality">Actuator modality type.</param>
/// <param name="IsActive">Whether the actuator is currently active.</param>
/// <param name="Capabilities">Capabilities this actuator provides.</param>
/// <param name="SupportedActions">List of supported action types.</param>
/// <param name="Properties">Additional actuator properties.</param>
public sealed record ActuatorInfo(
    string ActuatorId,
    string Name,
    ActuatorModality Modality,
    bool IsActive,
    EmbodimentCapabilities Capabilities,
    IReadOnlyList<string> SupportedActions,
    IReadOnlyDictionary<string, object>? Properties = null);