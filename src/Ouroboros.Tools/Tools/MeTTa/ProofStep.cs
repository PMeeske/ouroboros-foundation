namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a single proof step.
/// </summary>
/// <param name="Inference">Description of the inference.</param>
/// <param name="RuleApplied">Rule applied in this step.</param>
/// <param name="UsedFacts">Facts used in this step.</param>
public sealed record ProofStep(
    string Inference,
    Rule RuleApplied,
    List<Fact> UsedFacts);