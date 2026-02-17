// <copyright file="IOrchestrator.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// ==========================================================
// Base Orchestrator Interface
// Unified interface for all orchestrators in the suite
// ==========================================================

namespace Ouroboros.Agent;

/// <summary>
/// Base interface for all orchestrators in the Ouroboros suite.
/// Provides a unified contract for orchestration operations with consistent
/// configuration, metrics tracking, and error handling.
/// </summary>
/// <typeparam name="TInput">The input type for orchestration.</typeparam>
/// <typeparam name="TOutput">The output type from orchestration.</typeparam>
public interface IOrchestrator<TInput, TOutput>
{
    /// <summary>
    /// Gets the orchestrator's configuration.
    /// </summary>
    OrchestratorConfig Configuration { get; }

    /// <summary>
    /// Gets the orchestrator's current metrics.
    /// </summary>
    OrchestratorMetrics Metrics { get; }

    /// <summary>
    /// Executes the orchestration with the given input and context.
    /// </summary>
    /// <param name="input">The input to orchestrate.</param>
    /// <param name="context">Execution context with metadata and cancellation.</param>
    /// <returns>Result containing output, metrics, and execution details.</returns>
    Task<OrchestratorResult<TOutput>> ExecuteAsync(
        TInput input,
        OrchestratorContext? context = null);

    /// <summary>
    /// Validates that the orchestrator is ready to execute.
    /// </summary>
    /// <returns>Result indicating readiness with optional error message.</returns>
    Result<bool, string> ValidateReadiness();

    /// <summary>
    /// Gets detailed health information about the orchestrator.
    /// </summary>
    Task<Dictionary<string, object>> GetHealthAsync(CancellationToken ct = default);
}
