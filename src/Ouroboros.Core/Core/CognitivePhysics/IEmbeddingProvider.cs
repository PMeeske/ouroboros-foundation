// <copyright file="IEmbeddingProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Provides embedding vectors for semantic distance computation
/// within the cognitive physics metric space.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Computes the embedding vector for a given text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>The embedding vector as a float array.</returns>
    ValueTask<float[]> GetEmbeddingAsync(string text);
}
