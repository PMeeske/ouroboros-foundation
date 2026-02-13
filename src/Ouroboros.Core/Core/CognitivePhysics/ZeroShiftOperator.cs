// <copyright file="ZeroShiftOperator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Result of a ZeroShift context transition operation.
/// </summary>
public sealed record ZeroShiftResult
{
    public required bool Success { get; init; }
    public required CognitiveState State { get; init; }
    public required double Cost { get; init; }
    public string? FailureReason { get; init; }

    public static ZeroShiftResult Succeeded(CognitiveState state, double cost) =>
        new() { Success = true, State = state, Cost = cost };

    public static ZeroShiftResult Failed(CognitiveState state, string reason) =>
        new() { Success = false, State = state, Cost = 0.0, FailureReason = reason };
}

/// <summary>
/// Configuration for the ZeroShift operator.
/// </summary>
/// <param name="StabilityFactor">Multiplier applied to cost to determine cooldown increment.</param>
/// <param name="UncertaintyPenalty">Resource penalty applied on uncertain ethics evaluations.</param>
public sealed record ZeroShiftConfig(
    double StabilityFactor = 0.5,
    double UncertaintyPenalty = 5.0);

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
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IEthicsGate _ethicsGate;
    private readonly ZeroShiftConfig _config;

    public ZeroShiftOperator(
        IEmbeddingProvider embeddingProvider,
        IEthicsGate ethicsGate,
        ZeroShiftConfig? config = null)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _ethicsGate = ethicsGate ?? throw new ArgumentNullException(nameof(ethicsGate));
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
        EthicsGateResult ethicsResult = await _ethicsGate.EvaluateAsync(state.Focus, target);

        if (ethicsResult.IsDenied)
            return ZeroShiftResult.Failed(state, $"Ethics gate denied: {ethicsResult.Reason}");

        if (ethicsResult.IsUncertain)
        {
            // Soft fail: apply resource penalty but do not transition
            double penalizedResources = state.Resources - _config.UncertaintyPenalty;
            CognitiveState penalizedState = state with { Resources = penalizedResources };
            return ZeroShiftResult.Failed(penalizedState, $"Ethics gate uncertain: {ethicsResult.Reason}");
        }

        // Compute semantic distance and cost
        double distance = await SemanticDistance.ComputeAsync(_embeddingProvider, state.Focus, target);
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
