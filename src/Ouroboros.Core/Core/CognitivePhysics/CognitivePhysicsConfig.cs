namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Configuration for the Cognitive Physics Engine.
/// </summary>
public sealed record CognitivePhysicsConfig(
    ZeroShiftConfig ZeroShift,
    ChaosConfig Chaos,
    EvolutionaryConfig Evolution)
{
    public static CognitivePhysicsConfig Default => new(
        new ZeroShiftConfig(),
        new ChaosConfig(),
        new EvolutionaryConfig());
}