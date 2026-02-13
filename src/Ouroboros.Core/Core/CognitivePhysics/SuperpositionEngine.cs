// <copyright file="SuperpositionEngine.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Manages cognitive superposition — temporary multi-context reasoning where
/// multiple branches compete via coherence and ethical alignment scoring.
/// </summary>
public sealed class SuperpositionEngine
{
    private readonly IEthicsGate _ethicsGate;
    private readonly IEmbeddingProvider _embeddingProvider;

    public SuperpositionEngine(IEmbeddingProvider embeddingProvider, IEthicsGate ethicsGate)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _ethicsGate = ethicsGate ?? throw new ArgumentNullException(nameof(ethicsGate));
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

        ImmutableList<CognitiveBranch>.Builder builder = ImmutableList.CreateBuilder<CognitiveBranch>();

        foreach (string target in targets)
        {
            CognitiveState branchState = state with
            {
                Focus = target,
                History = state.History.Add(target),
                Entanglement = entanglement
            };
            builder.Add(new CognitiveBranch(branchState, equalWeight));
        }

        return builder.ToImmutable();
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

        foreach (CognitiveBranch branch in branches)
        {
            double coherence = 1.0 - await SemanticDistance.ComputeAsync(
                _embeddingProvider, origin, branch.State.Focus);

            EthicsGateResult ethics = await _ethicsGate.EvaluateAsync(origin, branch.State.Focus);
            double ethicsScore = ethics.IsAllowed ? 1.0 : ethics.IsUncertain ? 0.5 : 0.0;

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
