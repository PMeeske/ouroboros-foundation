// <copyright file="ZeroShiftOperator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// The ZeroShift operator performs resource-bounded, ethics-gated context transitions
/// in the cognitive physics metric space.
///
/// cost = semanticDistance(current, target) × compression
///
/// Rejects if: Resources &lt; cost, Cooldown &gt; 0, or ethics gate denies.
/// On success: Focus = target, Resources -= cost, History += target, Cooldown += cost × stabilityFactor.
/// </summary>
public sealed class ZeroShiftOperator
{
    private readonly Ouroboros.Domain.IEmbeddingModel _embeddingProvider;
    private readonly Func<string, string, ValueTask<EthicsGateResult>> _ethicsEvaluator;
    private readonly ZeroShiftConfig _config;

    /// <summary>Initialises the operator with the required embedding provider, optional ethics evaluator, and configuration.</summary>
    /// <param name="embeddingProvider">Provider used to compute semantic distance between contexts.</param>
    /// <param name="ethicsEvaluator">Optional delegate that evaluates ethical permissibility of transitions. Defaults to always-allow.</param>
    /// <param name="config">Optional shift configuration; uses <see cref="ZeroShiftConfig"/> defaults when null.</param>
    public ZeroShiftOperator(
        Ouroboros.Domain.IEmbeddingModel embeddingProvider,
        Func<string, string, ValueTask<EthicsGateResult>>? ethicsEvaluator = null,
        ZeroShiftConfig? config = null)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _ethicsEvaluator = ethicsEvaluator ?? ((_, _) => new ValueTask<EthicsGateResult>(EthicsGateResult.Allow()));
        _config = config ?? new ZeroShiftConfig();
    }

    /// <summary>
    /// Attempts a ZeroShift context transition from the current focus to the target domain.
    /// </summary>
    /// <param name="state">The current cognitive state.</param>
    /// <param name="target">The target conceptual domain.</param>
    /// <returns>The result of the shift attempt.</returns>
    public async ValueTask<ZeroShiftResult> ShiftAsync(CognitiveState state, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return ZeroShiftResult.Failed(state, "Target context cannot be empty.");

        if (state.Cooldown > 0)
            return ZeroShiftResult.Failed(state, $"Cooldown active: {state.Cooldown:F2} remaining.");

        // Ethics gate evaluation
        EthicsGateResult ethicsResult = await _ethicsEvaluator(state.Focus, target);

        if (ethicsResult.IsDenied)
            return ZeroShiftResult.Failed(state, $"Ethics gate denied: {ethicsResult.Reason}");

        if (ethicsResult.IsUncertain)
        {
            // Soft fail: apply resource penalty but do not transition
            double penalizedResources = Math.Max(0.0, state.Resources - _config.UncertaintyPenalty);
            CognitiveState penalizedState = state with { Resources = penalizedResources };
            return ZeroShiftResult.Failed(penalizedState, $"Ethics gate uncertain: {ethicsResult.Reason}");
        }

        // Compute semantic distance and cost
        float[] focusEmbedding = await _embeddingProvider.CreateEmbeddingsAsync(state.Focus);
        float[] targetEmbedding = await _embeddingProvider.CreateEmbeddingsAsync(target);
        double distance = SemanticDistance.Compute(focusEmbedding, targetEmbedding);
        double cost = distance * state.Compression;

        if (state.Resources < cost)
            return ZeroShiftResult.Failed(state, $"Insufficient resources: need {cost:F2}, have {state.Resources:F2}.");

        // Execute the shift
        CognitiveState newState = state with
        {
            Focus = target,
            Resources = state.Resources - cost,
            History = state.History.Add(target),
            Cooldown = cost * _config.StabilityFactor
        };

        return ZeroShiftResult.Succeeded(newState, cost);
    }
}
