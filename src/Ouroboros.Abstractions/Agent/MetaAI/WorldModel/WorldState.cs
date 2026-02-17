namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents a state in the world model.
/// </summary>
public sealed record WorldState(
    Guid Id,
    Dictionary<string, object> Features,
    DateTime Timestamp);