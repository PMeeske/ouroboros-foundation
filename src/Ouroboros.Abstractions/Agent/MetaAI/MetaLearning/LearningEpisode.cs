namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a recorded learning episode for meta-learning analysis.
/// </summary>
public sealed record LearningEpisode(
    Guid Id,
    string TaskType,
    string TaskDescription,
    LearningStrategy StrategyUsed,
    int ExamplesProvided,
    int IterationsRequired,
    double FinalPerformance,
    TimeSpan LearningDuration,
    List<PerformanceSnapshot> ProgressCurve,
    bool Successful,
    string? FailureReason,
    DateTime StartedAt,
    DateTime CompletedAt);