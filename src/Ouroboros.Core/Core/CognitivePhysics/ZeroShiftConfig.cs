namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Configuration for the ZeroShift operator.
/// </summary>
/// <param name="StabilityFactor">Multiplier applied to cost to determine cooldown increment.</param>
/// <param name="UncertaintyPenalty">Resource penalty applied on uncertain ethics evaluations.</param>
public sealed record ZeroShiftConfig(
    double StabilityFactor = 0.5,
    double UncertaintyPenalty = 5.0);