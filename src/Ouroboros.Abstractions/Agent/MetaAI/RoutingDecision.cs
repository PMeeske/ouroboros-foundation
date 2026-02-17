namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a routing decision for handling uncertainty.
/// </summary>
/// <param name="ShouldProceed">Whether to proceed with the action.</param>
/// <param name="ConfidenceLevel">Confidence in the decision (0.0 to 1.0).</param>
/// <param name="RecommendedStrategy">Recommended fallback strategy if confidence is low.</param>
/// <param name="Reason">Explanation for the routing decision.</param>
/// <param name="RequiresHumanOversight">Whether human oversight is required.</param>
/// <param name="AlternativeActions">Suggested alternative actions if available.</param>
public sealed record RoutingDecision(
    bool ShouldProceed,
    double ConfidenceLevel,
    FallbackStrategy RecommendedStrategy,
    string Reason,
    bool RequiresHumanOversight,
    IReadOnlyList<string> AlternativeActions);