// <copyright file="SemanticDistance.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Numerics.Tensors;
using System.Runtime.CompilerServices;

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Provides SIMD-accelerated semantic distance computations between embedding vectors,
/// backed by <see cref="TensorPrimitives"/> from System.Numerics.Tensors.
/// </summary>
public static class SemanticDistance
{
    /// <summary>
    /// Computes cosine similarity between two embedding vectors.
    /// Returns a value in [-1, 1] where 1 means identical direction.
    /// Returns 0 for empty or zero-magnitude vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length == 0 && b.Length == 0)
        {
            return 0.0;
        }

        double result = TensorPrimitives.CosineSimilarity(a, b);
        return double.IsNaN(result) ? 0.0 : result;
    }

    /// <summary>
    /// Computes semantic distance as (1 - cosine_similarity) / 2, normalized to [0, 1].
    /// 0 = identical, 1 = maximally different.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Compute(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        => (1.0 - CosineSimilarity(a, b)) / 2.0;
}
