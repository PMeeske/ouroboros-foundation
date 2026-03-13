// Copyright (c) Ouroboros. All rights reserved.

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Routes cognitive tasks to the appropriate model, providing task execution,
/// embedding generation, and multi-label classification capabilities.
/// </summary>
[ExcludeFromCodeCoverage]
public interface ICognitiveModelRouter
{
    /// <summary>
    /// Routes a cognitive task to the best available model and returns its output.
    /// </summary>
    /// <param name="cognitiveTask">The cognitive task category (e.g., "reasoning", "summarization").</param>
    /// <param name="input">The input to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The model's output text, or an error message on failure.</returns>
    Task<Result<string, string>> RouteTaskAsync(
        string cognitiveTask, string input, CancellationToken ct = default);

    /// <summary>
    /// Generates a task-specific embedding vector for the given input.
    /// </summary>
    /// <param name="cognitiveTask">The cognitive task category.</param>
    /// <param name="input">The input to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The embedding vector, or an error message on failure.</returns>
    Task<Result<float[], string>> GetTaskEmbeddingAsync(
        string cognitiveTask, string input, CancellationToken ct = default);

    /// <summary>
    /// Classifies the input against a set of candidate labels, returning
    /// the confidence score of the best matching label.
    /// </summary>
    /// <param name="cognitiveTask">The cognitive task category.</param>
    /// <param name="input">The input to classify.</param>
    /// <param name="labels">The candidate labels to score against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The highest classification confidence (0.0–1.0), or an error message on failure.</returns>
    Task<Result<double, string>> ClassifyAsync(
        string cognitiveTask, string input, List<string> labels, CancellationToken ct = default);
}
