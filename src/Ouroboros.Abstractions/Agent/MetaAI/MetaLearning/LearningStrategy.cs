namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a learning strategy for a specific type of task.
/// </summary>
public sealed record LearningStrategy(
    string Name,
    string Description,
    LearningApproach Approach,
    HyperparameterConfig Hyperparameters,
    List<string> SuitableTaskTypes,
    double ExpectedEfficiency,
    Dictionary<string, object> CustomConfig);