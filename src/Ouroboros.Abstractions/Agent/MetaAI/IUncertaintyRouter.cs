// <copyright file="IUncertaintyRouter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Strategies for handling uncertain situations when primary approach fails.
/// </summary>
public enum FallbackStrategy
{
    /// <summary>Retry the same approach.</summary>
    Retry = 0,

    /// <summary>Escalate to human oversight.</summary>
    EscalateToHuman = 1,

    /// <summary>Use a simpler, more conservative approach.</summary>
    UseConservativeApproach = 2,

    /// <summary>Defer the decision for later.</summary>
    Defer = 3,

    /// <summary>Abort the operation.</summary>
    Abort = 4,

    /// <summary>Ask for clarification or more information.</summary>
    RequestClarification = 5,
}

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

/// <summary>
/// Interface for routing decisions under uncertainty.
/// Determines how to handle situations where the agent lacks sufficient confidence.
/// </summary>
public interface IUncertaintyRouter
{
    /// <summary>
    /// Routes a decision based on uncertainty level.
    /// </summary>
    /// <param name="context">Current context information.</param>
    /// <param name="proposedAction">The action being considered.</param>
    /// <param name="confidenceLevel">Confidence in the proposed action (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Routing decision indicating how to proceed.</returns>
    Task<RoutingDecision> RouteDecisionAsync(
        string context,
        string proposedAction,
        double confidenceLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Determines if human oversight is required for a given situation.
    /// </summary>
    /// <param name="context">Current context information.</param>
    /// <param name="riskLevel">Risk level of the action (0.0 to 1.0).</param>
    /// <param name="confidenceLevel">Confidence level (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if human oversight is required.</returns>
    Task<bool> RequiresHumanOversightAsync(
        string context,
        double riskLevel,
        double confidenceLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the recommended fallback strategy for a given confidence level.
    /// </summary>
    /// <param name="confidenceLevel">Current confidence level (0.0 to 1.0).</param>
    /// <param name="attemptCount">Number of attempts made so far.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Recommended fallback strategy.</returns>
    Task<FallbackStrategy> GetFallbackStrategyAsync(
        double confidenceLevel,
        int attemptCount,
        CancellationToken ct = default);
}
