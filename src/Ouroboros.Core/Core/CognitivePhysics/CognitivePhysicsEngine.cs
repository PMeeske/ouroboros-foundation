// <copyright file="CognitivePhysicsEngine.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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

/// <summary>
/// The Cognitive Physics Engine (CPE) orchestrates reasoning as resource-bounded,
/// state-driven transformations through a metric conceptual space.
///
/// It composes ZeroShift transitions, ethics gates, superposition branching,
/// chaos injection, and evolutionary adaptation into Step pipelines.
/// </summary>
public sealed class CognitivePhysicsEngine
{
    private readonly ZeroShiftOperator _zeroShift;
    private readonly SuperpositionEngine _superposition;
    private readonly ChaosInjector _chaos;
    private readonly EvolutionaryAdapter _evolution;

    public CognitivePhysicsEngine(
        IEmbeddingProvider embeddingProvider,
        IEthicsGate ethicsGate,
        CognitivePhysicsConfig? config = null)
    {
        CognitivePhysicsConfig cfg = config ?? CognitivePhysicsConfig.Default;

        _zeroShift = new ZeroShiftOperator(embeddingProvider, ethicsGate, cfg.ZeroShift);
        _superposition = new SuperpositionEngine(embeddingProvider, ethicsGate);
        _chaos = new ChaosInjector(cfg.Chaos);
        _evolution = new EvolutionaryAdapter(cfg.Evolution);
    }

    /// <summary>
    /// Gets the ZeroShift operator for direct shift operations.
    /// </summary>
    public ZeroShiftOperator ZeroShift => _zeroShift;

    /// <summary>
    /// Gets the superposition engine for branching operations.
    /// </summary>
    public SuperpositionEngine Superposition => _superposition;

    /// <summary>
    /// Gets the chaos injector for exploration operations.
    /// </summary>
    public ChaosInjector Chaos => _chaos;

    /// <summary>
    /// Gets the evolutionary adapter for compression adaptation.
    /// </summary>
    public EvolutionaryAdapter Evolution => _evolution;

    /// <summary>
    /// Creates a Step that performs a ZeroShift to the specified target context.
    /// </summary>
    /// <param name="target">The target conceptual domain.</param>
    /// <returns>A Step transforming CognitiveState via ZeroShift.</returns>
    public Step<CognitiveState, Result<CognitiveState>> ShiftStep(string target) =>
        async state =>
        {
            ZeroShiftResult result = await _zeroShift.ShiftAsync(state, target);
            return result.Success
                ? Result<CognitiveState>.Success(result.State)
                : Result<CognitiveState>.Failure(result.FailureReason ?? "Shift failed.");
        };

    /// <summary>
    /// Creates a Step that applies evolutionary adaptation on success.
    /// </summary>
    /// <param name="coherenceScore">The coherence score for the successful reasoning.</param>
    /// <returns>A Step that adapts compression on the cognitive state.</returns>
    public Step<CognitiveState, CognitiveState> AdaptOnSuccessStep(double coherenceScore) =>
        state => Task.FromResult(_evolution.OnSuccess(state, coherenceScore));

    /// <summary>
    /// Creates a Step that applies evolutionary adaptation on failure.
    /// </summary>
    /// <returns>A Step that degrades compression on the cognitive state.</returns>
    public Step<CognitiveState, CognitiveState> AdaptOnFailureStep() =>
        state => Task.FromResult(_evolution.OnFailure(state));

    /// <summary>
    /// Creates a Step that injects chaos into the cognitive state.
    /// </summary>
    /// <returns>A Step that applies chaos injection.</returns>
    public Step<CognitiveState, Result<CognitiveState>> ChaosStep() =>
        state => Task.FromResult(_chaos.Inject(state));

    /// <summary>
    /// Creates a Step that enters superposition with the given target contexts.
    /// </summary>
    /// <param name="targets">The contexts to branch into.</param>
    /// <returns>A Step producing a list of weighted branches.</returns>
    public Step<CognitiveState, ImmutableList<CognitiveBranch>> EntangleStep(
        IReadOnlyList<string> targets) =>
        async state => await _superposition.EntangleAsync(state, targets);

    /// <summary>
    /// Creates a Step that collapses superposition branches back to a single state.
    /// </summary>
    /// <param name="origin">The original focus before superposition.</param>
    /// <returns>A Step selecting the best branch.</returns>
    public Step<ImmutableList<CognitiveBranch>, Option<CognitiveState>> CollapseStep(
        string origin) =>
        async branches => await _superposition.CollapseAsync(origin, branches);

    /// <summary>
    /// Creates a Step that ticks the cooldown by the specified elapsed time.
    /// </summary>
    /// <param name="elapsed">Time units to decrement from cooldown.</param>
    /// <returns>A Step that applies a cooldown tick.</returns>
    public Step<CognitiveState, CognitiveState> TickStep(double elapsed = 1.0) =>
        state => Task.FromResult(state.Tick(elapsed));

    /// <summary>
    /// Composes a multi-target reasoning pipeline:
    /// tick cooldown → shift to first target → adapt on result → shift to next, etc.
    /// </summary>
    /// <param name="state">The initial cognitive state.</param>
    /// <param name="targets">Ordered list of target contexts.</param>
    /// <returns>The final cognitive state after all transitions.</returns>
    public async Task<Result<CognitiveState>> ExecuteTrajectoryAsync(
        CognitiveState state,
        IReadOnlyList<string> targets)
    {
        CognitiveState current = state;

        foreach (string target in targets)
        {
            current = current.Tick();

            ZeroShiftResult result = await _zeroShift.ShiftAsync(current, target);

            if (!result.Success)
            {
                current = _evolution.OnFailure(result.State);
                return Result<CognitiveState>.Failure(
                    result.FailureReason ?? $"Failed to shift to '{target}'.");
            }

            double coherence = 1.0 - (result.Cost / Math.Max(current.Compression, 0.1));
            coherence = Math.Clamp(coherence, 0.0, 1.0);
            current = _evolution.OnSuccess(result.State, coherence);
        }

        return Result<CognitiveState>.Success(current);
    }
}
