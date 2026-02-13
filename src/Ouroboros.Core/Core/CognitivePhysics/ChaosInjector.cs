// <copyright file="ChaosInjector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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

/// <summary>
/// Injects controlled chaos into the cognitive state for exploration mode.
/// Chaos may distort semantic distance, reduce compression temporarily,
/// and increase branching probability — at a resource and cooldown cost.
/// </summary>
public sealed class ChaosInjector
{
    private readonly ChaosConfig _config;

    public ChaosInjector(ChaosConfig? config = null)
    {
        _config = config ?? new ChaosConfig();
    }

    /// <summary>
    /// Gets the chaos configuration.
    /// </summary>
    public ChaosConfig Config => _config;

    /// <summary>
    /// Injects chaos into the given cognitive state if sufficient resources are available.
    /// </summary>
    /// <param name="state">The current cognitive state.</param>
    /// <returns>A result containing the chaotic state, or a failure if resources are insufficient.</returns>
    public Result<CognitiveState> Inject(CognitiveState state)
    {
        if (state.Resources < _config.ChaosCost)
        {
            return Result<CognitiveState>.Failure(
                $"Insufficient resources for chaos injection: need {_config.ChaosCost:F2}, have {state.Resources:F2}.");
        }

        double newCompression = Math.Max(0.1, state.Compression - _config.CompressionReduction);

        CognitiveState chaoticState = state with
        {
            Resources = state.Resources - _config.ChaosCost,
            Compression = newCompression,
            Cooldown = state.Cooldown + _config.InstabilityFactor
        };

        return Result<CognitiveState>.Success(chaoticState);
    }

    /// <summary>
    /// Distorts a semantic distance value for exploration purposes.
    /// </summary>
    /// <param name="distance">The original semantic distance.</param>
    /// <returns>The distorted distance.</returns>
    public double DistortDistance(double distance) =>
        distance * _config.DistanceDistortion;
}
