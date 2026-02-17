namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Action request to an actuator.
/// </summary>
/// <param name="TargetActuator">Target actuator ID.</param>
/// <param name="Modality">Output modality.</param>
/// <param name="Content">Content to output.</param>
/// <param name="Parameters">Additional parameters.</param>
public sealed record ActionRequest(
    string TargetActuator,
    ActuatorModality Modality,
    object Content,
    IReadOnlyDictionary<string, object>? Parameters = null);