// <copyright file="IAdvancedMeTTaEngine.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Advanced MeTTa engine with rule learning, theorem proving, and abductive reasoning.
/// </summary>
public interface IAdvancedMeTTaEngine : IMeTTaEngine
{
    /// <summary>
    /// Induces rules from observations using specified induction strategy.
    /// </summary>
    /// <param name="observations">List of facts to learn from.</param>
    /// <param name="strategy">Induction strategy to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing induced rules or an error message.</returns>
    Task<Result<List<Rule>, string>> InduceRulesAsync(
        List<Fact> observations,
        InductionStrategy strategy,
        CancellationToken ct = default);

    /// <summary>
    /// Proves a theorem using specified proof strategy.
    /// </summary>
    /// <param name="theorem">The theorem to prove.</param>
    /// <param name="axioms">List of axioms to use.</param>
    /// <param name="strategy">Proof strategy to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the proof trace or an error message.</returns>
    Task<Result<ProofTrace, string>> ProveTheoremAsync(
        string theorem,
        List<string> axioms,
        ProofStrategy strategy,
        CancellationToken ct = default);

    /// <summary>
    /// Generates hypotheses to explain an observation.
    /// </summary>
    /// <param name="observation">The observation to explain.</param>
    /// <param name="backgroundKnowledge">Background knowledge to use.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing generated hypotheses or an error message.</returns>
    Task<Result<List<Hypothesis>, string>> GenerateHypothesesAsync(
        string observation,
        List<string> backgroundKnowledge,
        CancellationToken ct = default);

    /// <summary>
    /// Infers the type of an atom in a given context.
    /// </summary>
    /// <param name="atom">The atom to type.</param>
    /// <param name="context">The type context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the typed atom or an error message.</returns>
    Task<Result<TypedAtom, string>> InferTypeAsync(
        string atom,
        TypeContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Applies forward chaining to derive new facts.
    /// </summary>
    /// <param name="rules">Rules to apply.</param>
    /// <param name="facts">Initial facts.</param>
    /// <param name="maxSteps">Maximum inference steps.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing derived facts or an error message.</returns>
    Task<Result<List<Fact>, string>> ForwardChainAsync(
        List<Rule> rules,
        List<Fact> facts,
        int maxSteps = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Applies backward chaining to prove a goal.
    /// </summary>
    /// <param name="goal">Goal to prove.</param>
    /// <param name="rules">Rules to use.</param>
    /// <param name="knownFacts">Known facts.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing facts needed to prove the goal or an error message.</returns>
    Task<Result<List<Fact>, string>> BackwardChainAsync(
        Fact goal,
        List<Rule> rules,
        List<Fact> knownFacts,
        CancellationToken ct = default);
}