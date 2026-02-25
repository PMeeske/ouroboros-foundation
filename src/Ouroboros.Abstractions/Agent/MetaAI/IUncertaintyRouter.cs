// <copyright file="IUncertaintyRouter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

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
