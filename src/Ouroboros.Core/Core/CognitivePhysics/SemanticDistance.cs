// <copyright file="SemanticDistance.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Computes semantic distance between concepts using cosine similarity
/// over embedding vectors. The metric space is normalized to [0, 1].
/// </summary>
public static class SemanticDistance
{
    /// <summary>
    /// Computes the cosine similarity between two vectors.
    /// </summary>
    /// <returns>A value in [-1, 1] where 1 means identical direction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same dimensionality.");

        if (a.Length == 0)
            return 0.0;

        double dot = 0.0, normA = 0.0, normB = 0.0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * (double)b[i];
            normA += a[i] * (double)a[i];
            normB += b[i] * (double)b[i];
        }

        double denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator < 1e-15 ? 0.0 : dot / denominator;
    }

    /// <summary>
    /// Computes the normalized semantic distance between two embedding vectors.
    /// Returns a value in [0, 1] where 0 means identical and 1 means maximally distant.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Compute(ReadOnlySpan<float> a, ReadOnlySpan<float> b) =>
        Math.Clamp(1.0 - CosineSimilarity(a, b), 0.0, 1.0);

    /// <summary>
    /// Computes the semantic distance between two concepts using an embedding provider.
    /// </summary>
    public static async ValueTask<double> ComputeAsync(
        IEmbeddingProvider provider,
        string from,
        string to)
    {
        float[] embeddingA = await provider.GetEmbeddingAsync(from);
        float[] embeddingB = await provider.GetEmbeddingAsync(to);
        return Compute(embeddingA, embeddingB);
    }
}
