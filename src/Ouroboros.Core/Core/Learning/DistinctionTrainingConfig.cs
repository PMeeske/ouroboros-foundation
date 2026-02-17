using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Learning;

/// <summary>
/// Configuration for distinction-based training.
/// </summary>
/// <param name="MaxSteps">Maximum training steps. Default: 100.</param>
/// <param name="LearningRate">Learning rate for training. Default: 1e-4.</param>
/// <param name="DistinctionWeight">Weight for this specific distinction. Default: 1.0.</param>
/// <param name="UseContrastiveLoss">Whether to use contrastive loss for "this, not that". Default: true.</param>
/// <param name="TargetStage">Target dream stage for training. Default: Recognition.</param>
public sealed record DistinctionTrainingConfig(
    int MaxSteps = 100,
    double LearningRate = 1e-4,
    double DistinctionWeight = 1.0,
    bool UseContrastiveLoss = true,
    DreamStage TargetStage = DreamStage.Recognition)
{
    /// <summary>
    /// Creates a configuration optimized for a specific dream stage.
    /// </summary>
    /// <param name="stage">The dream stage.</param>
    /// <returns>A training configuration tailored to the stage.</returns>
    public static DistinctionTrainingConfig ForStage(DreamStage stage) => stage switch
    {
        DreamStage.Void => new(MaxSteps: 10, LearningRate: 1e-5, DistinctionWeight: 0.1),
        DreamStage.Distinction => new(MaxSteps: 50, LearningRate: 5e-5, DistinctionWeight: 0.3),
        DreamStage.SubjectEmerges => new(MaxSteps: 100, LearningRate: 1e-4, DistinctionWeight: 0.5),
        DreamStage.WorldCrystallizes => new(MaxSteps: 150, LearningRate: 1e-4, DistinctionWeight: 0.7),
        DreamStage.Forgetting => new(MaxSteps: 200, LearningRate: 5e-5, DistinctionWeight: 0.8),
        DreamStage.Questioning => new(MaxSteps: 100, LearningRate: 1e-4, DistinctionWeight: 0.6),
        DreamStage.Recognition => new(MaxSteps: 200, LearningRate: 1e-4, DistinctionWeight: 1.0),
        DreamStage.Dissolution => new(MaxSteps: 50, LearningRate: 5e-5, DistinctionWeight: 0.2),
        DreamStage.NewDream => new(MaxSteps: 10, LearningRate: 1e-5, DistinctionWeight: 0.1),
        _ => new()
    };
}