namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Configuration for learning hyperparameters.
/// </summary>
public sealed record HyperparameterConfig(
    double LearningRate,
    int BatchSize,
    int MaxIterations,
    double QualityThreshold,
    double ExplorationRate,
    Dictionary<string, object> CustomParams);