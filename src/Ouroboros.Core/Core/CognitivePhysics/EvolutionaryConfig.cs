namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Configuration for evolutionary compression adaptation.
/// </summary>
/// <param name="LearningRate">Rate at which compression improves on success.</param>
/// <param name="PenaltyFactor">Rate at which compression degrades on failure.</param>
/// <param name="MinCompression">Lower bound for compression (most efficient).</param>
/// <param name="MaxCompression">Upper bound for compression (least efficient).</param>
public sealed record EvolutionaryConfig(
    double LearningRate = 0.05,
    double PenaltyFactor = 0.1,
    double MinCompression = 0.1,
    double MaxCompression = 1.0);