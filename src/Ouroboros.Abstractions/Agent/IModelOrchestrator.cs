#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ==========================================================
// Model Orchestrator Interface
// Defines contract for intelligent model and tool selection
// based on prompt analysis and performance metrics
// ==========================================================

namespace Ouroboros.Agent;

/// <summary>
/// Orchestrates model and tool selection based on prompt analysis and performance metrics.
/// Implements intelligent routing to optimize for quality, cost, and performance.
/// </summary>
public interface IModelOrchestrator
{
    /// <summary>
    /// Analyzes a prompt and selects the optimal model and tool configuration.
    /// </summary>
    /// <param name="prompt">The input prompt to analyze</param>
    /// <param name="context">Optional contextual information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Orchestrator decision with selected model and tools</returns>
    Task<Result<OrchestratorDecision, string>> SelectModelAsync(
        string prompt,
        Dictionary<string, object>? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Classifies a prompt into a use case for model selection.
    /// </summary>
    /// <param name="prompt">The prompt to classify</param>
    /// <returns>Classified use case</returns>
    UseCase ClassifyUseCase(string prompt);

    /// <summary>
    /// Registers a model with its capabilities for orchestration.
    /// </summary>
    /// <param name="capability">Model capability metadata</param>
    void RegisterModel(ModelCapability capability);

    /// <summary>
    /// Records performance metrics for a model or tool execution.
    /// </summary>
    /// <param name="resourceName">Name of the model or tool</param>
    /// <param name="latencyMs">Execution time in milliseconds</param>
    /// <param name="success">Whether execution succeeded</param>
    void RecordMetric(string resourceName, double latencyMs, bool success);

    /// <summary>
    /// Gets current performance metrics for all resources.
    /// </summary>
    /// <returns>Dictionary of resource names to their metrics</returns>
    IReadOnlyDictionary<string, PerformanceMetrics> GetMetrics();
}
