namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Configuration for chaos injection during exploration mode.
/// </summary>
/// <param name="ChaosCost">Resource cost for entering chaos mode.</param>
/// <param name="InstabilityFactor">Cooldown penalty for chaos injection.</param>
/// <param name="CompressionReduction">Temporary compression reduction during chaos.</param>
/// <param name="DistanceDistortion">Multiplier applied to distort semantic distance.</param>
public sealed record ChaosConfig(
    double ChaosCost = 10.0,
    double InstabilityFactor = 2.0,
    double CompressionReduction = 0.3,
    double DistanceDistortion = 0.5);