// <copyright file="IReasoner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Resilience;

using Ouroboros.Core.Monads;

/// <summary>
/// Unified reasoning interface that abstracts over LLM and symbolic reasoning
/// with automatic fallback and circuit breaking.
/// </summary>
public interface IReasoner
{
    /// <summary>
    /// Performs reasoning with automatic fallback based on service health.
    /// </summary>
    /// <param name="query">The reasoning query to process.</param>
    /// <param name="preferredMode">The preferred reasoning mode to use. Default is NeuralFirst.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing either the reasoning response or an error message.</returns>
    Task<Result<string, string>> ReasonAsync(
        string query,
        ReasoningMode preferredMode = ReasoningMode.NeuralFirst,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the current health status of the reasoning backends.
    /// </summary>
    /// <returns>Health information including circuit state and failure counts.</returns>
    ReasonerHealth GetHealth();
}