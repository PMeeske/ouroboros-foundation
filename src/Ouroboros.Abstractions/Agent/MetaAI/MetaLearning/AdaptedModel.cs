namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a model adapted to a new task using few-shot learning.
/// </summary>
public sealed record AdaptedModel(
    string TaskDescription,
    Skill AdaptedSkill,
    int ExamplesUsed,
    double EstimatedPerformance,
    double AdaptationTime,
    List<string> LearnedPatterns);