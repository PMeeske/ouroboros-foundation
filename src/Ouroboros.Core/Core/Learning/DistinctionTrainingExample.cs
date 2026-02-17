using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Learning;

/// <summary>
/// Training example for distinction-based learning.
/// </summary>
/// <param name="Circumstance">The circumstance in which the distinction was made.</param>
/// <param name="DistinctionMade">The distinction itself: "This, not that".</param>
/// <param name="Stage">The dream stage at which this distinction occurs.</param>
/// <param name="ContextEmbedding">Semantic embedding of the context.</param>
/// <param name="ImportanceWeight">How important this distinction is (0.0 to 1.0).</param>
public sealed record DistinctionTrainingExample(
    string Circumstance,
    string DistinctionMade,
    DreamStage Stage,
    float[] ContextEmbedding,
    double ImportanceWeight);