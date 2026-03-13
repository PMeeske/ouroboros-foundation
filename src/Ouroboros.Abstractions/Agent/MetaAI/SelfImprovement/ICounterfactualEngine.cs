using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Interface for counterfactual reasoning and regret computation.
/// Simulates alternative action sequences and compares with actual outcomes.
/// </summary>
[ExcludeFromCodeCoverage]
public interface ICounterfactualEngine
{
    /// <summary>
    /// Simulates an alternative action and predicts its outcome.
    /// </summary>
    /// <param name="actualAction">The action that was actually taken.</param>
    /// <param name="alternativeAction">The alternative action to simulate.</param>
    /// <param name="context">Context in which the action occurred.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Counterfactual simulation result.</returns>
    Task<Result<CounterfactualSimulation, string>> SimulateAlternativeAsync(
        string actualAction, string alternativeAction, string context,
        CancellationToken ct = default);

    /// <summary>
    /// Computes the regret magnitude for a past decision.
    /// </summary>
    /// <param name="actionTaken">The action that was taken.</param>
    /// <param name="bestAlternative">The best alternative action identified.</param>
    /// <param name="outcome">The actual outcome observed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Regret magnitude (0.0 = no regret, 1.0 = maximum regret).</returns>
    Task<Result<double, string>> ComputeRegretAsync(
        string actionTaken, string bestAlternative, string outcome,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a contrastive explanation for why an expected outcome did not occur.
    /// </summary>
    /// <param name="actualOutcome">The actual outcome.</param>
    /// <param name="expectedOutcome">The expected outcome.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Contrastive explanation.</returns>
    Task<Result<ContrastiveExplanation, string>> ExplainContrastivelyAsync(
        string actualOutcome, string expectedOutcome,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the history of regret records.
    /// </summary>
    /// <param name="count">Number of records to retrieve.</param>
    /// <returns>List of regret records.</returns>
    List<RegretRecord> GetRegretHistory(int count = 20);
}

/// <summary>
/// Result of a counterfactual simulation.
/// </summary>
/// <param name="ActualAction">The action that was actually taken.</param>
/// <param name="AlternativeAction">The simulated alternative action.</param>
/// <param name="PredictedAlternativeOutcome">Predicted outcome of the alternative.</param>
/// <param name="OutcomeQualityDiff">Quality difference (positive = alternative was better).</param>
public sealed record CounterfactualSimulation(
    string ActualAction, string AlternativeAction,
    string PredictedAlternativeOutcome, double OutcomeQualityDiff);

/// <summary>
/// Contrastive explanation for outcome divergence.
/// </summary>
/// <param name="ActualOutcome">The actual outcome.</param>
/// <param name="ExpectedOutcome">The expected outcome.</param>
/// <param name="DifferentiatingFactors">Factors that caused the divergence.</param>
/// <param name="Explanation">Human-readable contrastive explanation.</param>
public sealed record ContrastiveExplanation(
    string ActualOutcome, string ExpectedOutcome,
    List<string> DifferentiatingFactors, string Explanation);

/// <summary>
/// Record of regret for a past decision.
/// </summary>
/// <param name="ActionTaken">The action that was taken.</param>
/// <param name="BestAlternative">The best alternative identified.</param>
/// <param name="RegretMagnitude">Magnitude of regret (0.0 to 1.0).</param>
/// <param name="Timestamp">When the regret was recorded.</param>
public sealed record RegretRecord(
    string ActionTaken, string BestAlternative,
    double RegretMagnitude, DateTime Timestamp);
