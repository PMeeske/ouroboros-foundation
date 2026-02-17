namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents a state transition observed in the environment.
/// </summary>
public sealed record WorldTransition(
    WorldState FromState,
    AgentAction Action,
    WorldState ToState,
    double Reward);