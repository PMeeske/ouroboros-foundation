namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents a planned sequence of actions from model-based planning.
/// </summary>
public sealed record ActionPlan(
    List<AgentAction> Actions,
    double ExpectedReward,
    double Confidence,
    int LookaheadDepth);