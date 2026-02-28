// <copyright file="FakeEmbeddingProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Domain;

namespace Ouroboros.Tests.CognitivePhysics;

/// <summary>
/// A deterministic fake embedding model for testing.
/// Returns configurable embeddings per text key, or a hash-based default.
/// </summary>
internal sealed class FakeEmbeddingProvider : IEmbeddingModel
{
    private readonly Dictionary<string, float[]> _embeddings = new();

    public void SetEmbedding(string text, float[] embedding) =>
        _embeddings[text] = embedding;

    public Task<float[]> CreateEmbeddingsAsync(string input, CancellationToken ct = default)
    {
        if (_embeddings.TryGetValue(input, out float[]? embedding))
            return Task.FromResult(embedding);

        // Generate a deterministic pseudo-embedding from hash
        long hash = unchecked((long)input.GetHashCode());
        float[] generated =
        [
            (float)Math.Sin(hash),
            (float)Math.Cos(hash),
            (float)Math.Sin(hash * 2L),
            (float)Math.Cos(hash * 2L)
        ];
        return Task.FromResult(generated);
    }
}
