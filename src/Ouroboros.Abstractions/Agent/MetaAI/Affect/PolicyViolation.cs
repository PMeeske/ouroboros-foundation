namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Result of a policy evaluation.
/// </summary>
public sealed record PolicyViolation(
    Guid RuleId,
    string RuleName,
    SignalType Signal,
    double ObservedValue,
    double LowerBound,
    double UpperBound,
    string ViolationType,
    HomeostasisAction RecommendedAction,
    double Severity,
    DateTime DetectedAt);