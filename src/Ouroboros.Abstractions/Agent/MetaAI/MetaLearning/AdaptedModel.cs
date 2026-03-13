using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a model adapted to a new task using few-shot learning.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record AdaptedModel(
    string TaskDescription,
    Skill AdaptedSkill,
    int ExamplesUsed,
    double EstimatedPerformance,
    double AdaptationTime,
    List<string> LearnedPatterns);
