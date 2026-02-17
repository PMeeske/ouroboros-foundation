namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents an action that can be taken in the world model.
/// </summary>
public sealed record AgentAction(
    string Name,
    Dictionary<string, object>? Parameters = null);