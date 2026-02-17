namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Report on learning efficiency and bottlenecks.
/// </summary>
public sealed record LearningEfficiencyReport(
    double AverageIterationsToLearn,
    double AverageExamplesNeeded,
    double SuccessRate,
    double LearningSpeedTrend,
    Dictionary<string, double> EfficiencyByTaskType,
    List<string> Bottlenecks,
    List<string> Recommendations);