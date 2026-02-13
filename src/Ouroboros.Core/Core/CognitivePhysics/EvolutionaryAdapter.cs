// <copyright file="EvolutionaryAdapter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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

/// <summary>
/// Adapts the compression coefficient of cognitive state based on reasoning outcomes.
/// Successful reasoning decreases compression (more efficient); failure increases it.
///
/// Success: Compression -= learningRate × coherenceScore
/// Failure: Compression += penaltyFactor
/// Bounded by: 0.1 ≤ Compression ≤ 1.0
/// </summary>
public sealed class EvolutionaryAdapter
{
    private readonly EvolutionaryConfig _config;

    public EvolutionaryAdapter(EvolutionaryConfig? config = null)
    {
        _config = config ?? new EvolutionaryConfig();
    }

    /// <summary>
    /// Adapts compression downward on successful reasoning.
    /// </summary>
    /// <param name="state">The current cognitive state.</param>
    /// <param name="coherenceScore">A coherence score in [0, 1] measuring reasoning quality.</param>
    /// <returns>A new state with improved compression.</returns>
    public CognitiveState OnSuccess(CognitiveState state, double coherenceScore)
    {
        double clampedCoherence = Math.Clamp(coherenceScore, 0.0, 1.0);
        double newCompression = state.Compression - (_config.LearningRate * clampedCoherence);
        newCompression = Math.Clamp(newCompression, _config.MinCompression, _config.MaxCompression);

        return state with { Compression = newCompression };
    }

    /// <summary>
    /// Adapts compression upward on reasoning failure.
    /// </summary>
    /// <param name="state">The current cognitive state.</param>
    /// <returns>A new state with degraded compression.</returns>
    public CognitiveState OnFailure(CognitiveState state)
    {
        double newCompression = state.Compression + _config.PenaltyFactor;
        newCompression = Math.Clamp(newCompression, _config.MinCompression, _config.MaxCompression);

        return state with { Compression = newCompression };
    }
}
