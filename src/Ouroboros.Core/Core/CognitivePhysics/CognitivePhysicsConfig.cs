namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Configuration for the Cognitive Physics Engine.
/// </summary>
public sealed record CognitivePhysicsConfig(
    ZeroShiftConfig ZeroShift,
    ChaosConfig Chaos,
    EvolutionaryConfig Evolution)
{
    /// <summary>Returns a default configuration with standard zero-shift, chaos, and evolutionary settings.</summary>
    public static CognitivePhysicsConfig Default => new(
        new ZeroShiftConfig(),
        new ChaosConfig(),
        new EvolutionaryConfig());
}
