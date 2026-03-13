// <copyright file="SuperpositionEngine.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Manages cognitive superposition — temporary multi-context reasoning where
/// multiple branches compete via coherence and ethical alignment scoring.
/// </summary>
public sealed class SuperpositionEngine
{
    private readonly Func<string, string, ValueTask<EthicsGateResult>> _ethicsEvaluator;
    private readonly Ouroboros.Domain.IEmbeddingModel _embeddingProvider;

    /// <summary>Initialises the engine with the required embedding provider and optional ethics evaluator.</summary>
    /// <param name="embeddingProvider">Provider used for semantic coherence scoring during collapse.</param>
    /// <param name="ethicsEvaluator">Optional delegate that evaluates ethical permissibility of transitions. Defaults to always-allow.</param>
    public SuperpositionEngine(
        Ouroboros.Domain.IEmbeddingModel embeddingProvider,
        Func<string, string, ValueTask<EthicsGateResult>>? ethicsEvaluator = null)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _ethicsEvaluator = ethicsEvaluator ?? ((_, _) => new ValueTask<EthicsGateResult>(EthicsGateResult.Allow()));
    }

    /// <summary>
    /// Enters superposition by forking the current state into multiple branches,
    /// one per target context.
    /// </summary>
    /// <param name="state">The source cognitive state.</param>
    /// <param name="targets">The target contexts to branch into.</param>
    /// <returns>A list of weighted cognitive branches.</returns>
    public async ValueTask<ImmutableList<CognitiveBranch>> EntangleAsync(
        CognitiveState state,
        IReadOnlyList<string> targets)
    {
        if (targets.Count == 0)
            return ImmutableList<CognitiveBranch>.Empty;

        ImmutableHashSet<string> entanglement = state.Entanglement.Union(targets);
        double equalWeight = 1.0 / targets.Count;

        return targets.Select(target => new CognitiveBranch(
            state with
            {
                Focus = target,
                History = state.History.Add(target),
                Entanglement = entanglement
            },
            equalWeight)).ToImmutableList();
    }

    /// <summary>
    /// Collapses superposition branches by scoring each on coherence (semantic proximity
    /// to origin) and ethical alignment, then selecting the highest-weighted branch.
    /// </summary>
    /// <param name="origin">The original focus before superposition.</param>
    /// <param name="branches">The competing branches.</param>
    /// <returns>The collapsed state from the winning branch, or None if all branches fail.</returns>
    public async ValueTask<Option<CognitiveState>> CollapseAsync(
        string origin,
        ImmutableList<CognitiveBranch> branches)
    {
        if (branches.IsEmpty)
            return Option<CognitiveState>.None();

        CognitiveBranch? best = null;
        double bestScore = double.MinValue;

        float[] originEmbedding = await _embeddingProvider.CreateEmbeddingsAsync(origin).ConfigureAwait(false);

        foreach (CognitiveBranch branch in branches)
        {
            EthicsGateResult ethics = await _ethicsEvaluator(origin, branch.State.Focus).ConfigureAwait(false);

            // Denied branches are excluded entirely — they cannot win collapse
            if (ethics.IsDenied)
                continue;

            float[] branchEmbedding = await _embeddingProvider.CreateEmbeddingsAsync(branch.State.Focus).ConfigureAwait(false);
            double coherence = 1.0 - SemanticDistance.Compute(originEmbedding, branchEmbedding);

            double ethicsScore = ethics.IsAllowed ? 1.0 : 0.5;

            double score = (coherence + ethicsScore) * branch.Weight;

            if (score > bestScore)
            {
                bestScore = score;
                best = branch;
            }
        }

        if (best is null)
            return Option<CognitiveState>.None();

        // Clear entanglement on collapse
        CognitiveState collapsed = best.State with { Entanglement = ImmutableHashSet<string>.Empty };
        return Option<CognitiveState>.Some(collapsed);
    }
}
