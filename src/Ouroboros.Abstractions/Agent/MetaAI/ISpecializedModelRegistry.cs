// Copyright (c) Ouroboros. All rights reserved.

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Registry for mapping cognitive tasks to specialized model endpoints.
/// Supports priority-based routing with a configurable fallback model.
/// </summary>
public interface ISpecializedModelRegistry
{
    /// <summary>
    /// Registers a model endpoint for a specific cognitive task.
    /// </summary>
    /// <param name="cognitiveTask">The cognitive task category (e.g., "reasoning", "coding").</param>
    /// <param name="modelEndpoint">The model endpoint URI.</param>
    /// <param name="modelName">Human-readable model name.</param>
    /// <param name="priority">Priority weight for selection (higher = preferred). Defaults to 1.0.</param>
    void RegisterModel(string cognitiveTask, string modelEndpoint, string modelName, double priority = 1.0);

    /// <summary>
    /// Returns the highest-priority model endpoint for the given cognitive task,
    /// or <c>null</c> if no model is registered for that task.
    /// </summary>
    /// <param name="cognitiveTask">The cognitive task to look up.</param>
    /// <returns>The best model endpoint, or <c>null</c> if none is registered.</returns>
    string? GetBestModel(string cognitiveTask);

    /// <summary>
    /// Returns all registered task-to-model mappings with their priorities.
    /// </summary>
    /// <returns>A list of (Task, Model, Priority) tuples.</returns>
    List<(string Task, string Model, double Priority)> GetAllRegistrations();

    /// <summary>
    /// Sets the fallback model endpoint used when no task-specific model is available.
    /// </summary>
    /// <param name="modelEndpoint">The fallback model endpoint URI.</param>
    void SetFallbackModel(string modelEndpoint);
}
