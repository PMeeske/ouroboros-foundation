namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Outcome of an actuator action.
/// </summary>
/// <param name="ActuatorId">Actuator that executed the action.</param>
/// <param name="ActionType">Type of action executed.</param>
/// <param name="Success">Whether the action succeeded.</param>
/// <param name="Duration">How long the action took.</param>
/// <param name="Result">Result data if any.</param>
/// <param name="Error">Error message if failed.</param>
public sealed record ActionOutcome(
    string ActuatorId,
    string ActionType,
    bool Success,
    TimeSpan Duration,
    object? Result = null,
    string? Error = null);