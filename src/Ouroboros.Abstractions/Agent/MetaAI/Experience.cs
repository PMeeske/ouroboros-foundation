namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents an experience stored in agent memory.
/// Contains the context, action taken, and outcomes.
/// </summary>
/// <param name="Id">Unique identifier for the experience.</param>
/// <param name="Timestamp">When the experience occurred.</param>
/// <param name="Context">Contextual information at the time.</param>
/// <param name="Action">The action that was taken.</param>
/// <param name="Outcome">The result or outcome.</param>
/// <param name="Success">Whether the experience was successful.</param>
/// <param name="Tags">Tags for categorization and retrieval.</param>
/// <param name="Goal">The goal that was being pursued.</param>
/// <param name="Execution">The execution result.</param>
/// <param name="Verification">The verification result.</param>
/// <param name="Plan">The plan that was executed (optional).</param>
/// <param name="Metadata">Additional metadata about the experience.</param>
public sealed record Experience(
    string Id,
    DateTime Timestamp,
    string Context,
    string Action,
    string Outcome,
    bool Success,
    IReadOnlyList<string> Tags,
    string Goal,
    PlanExecutionResult Execution,
    PlanVerificationResult Verification,
    Plan? Plan = null,
    IReadOnlyDictionary<string, object>? Metadata = null);