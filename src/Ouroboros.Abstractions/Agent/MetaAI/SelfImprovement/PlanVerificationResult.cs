namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Result of plan verification.
/// </summary>
/// <param name="Execution">The execution result that was verified.</param>
/// <param name="Verified">Whether the plan was verified successfully.</param>
/// <param name="QualityScore">Quality score from 0.0 to 1.0.</param>
/// <param name="Issues">List of issues found during verification.</param>

/// <param name="Improvements">List of suggested improvements.</param>
/// <param name="Timestamp">When verification occurred.</param>
public sealed record PlanVerificationResult(
    PlanExecutionResult Execution,
    bool Verified,
    double QualityScore,
    IReadOnlyList<string> Issues,

    IReadOnlyList<string> Improvements,
    DateTime? Timestamp);