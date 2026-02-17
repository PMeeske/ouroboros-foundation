namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents an observation of variables at a point in time.
/// </summary>
/// <param name="Values">The observed values for each variable.</param>
/// <param name="Timestamp">When the observation was made.</param>
/// <param name="Context">Optional context information about the observation.</param>
public sealed record Observation(
    Dictionary<string, object> Values,
    DateTime Timestamp,
    string? Context);