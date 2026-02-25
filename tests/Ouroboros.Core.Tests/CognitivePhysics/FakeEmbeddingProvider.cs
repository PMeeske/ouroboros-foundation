// <copyright file="FakeEmbeddingProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.CognitivePhysics;

namespace Ouroboros.Tests.CognitivePhysics;

/// <summary>
/// A deterministic fake embedding provider for testing.
/// Returns configurable embeddings per text key, or a hash-based default.
/// </summary>
internal sealed class FakeEmbeddingProvider : IEmbeddingProvider
{
    private readonly Dictionary<string, float[]> _embeddings = new();

    public void SetEmbedding(string text, float[] embedding) =>
        _embeddings[text] = embedding;

    public ValueTask<float[]> GetEmbeddingAsync(string text)
    {
        if (_embeddings.TryGetValue(text, out float[]? embedding))
            return ValueTask.FromResult(embedding);

        // Generate a deterministic pseudo-embedding from hash
        long hash = unchecked((long)text.GetHashCode());
        float[] generated =
        [
            (float)Math.Sin(hash),
            (float)Math.Cos(hash),
            (float)Math.Sin(hash * 2L),
            (float)Math.Cos(hash * 2L)
        ];
        return ValueTask.FromResult(generated);
    }
}
