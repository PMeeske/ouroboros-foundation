// <copyright file="SemanticDistanceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class SemanticDistanceTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ShouldReturnOne()
    {
        float[] a = [1.0f, 2.0f, 3.0f];
        float[] b = [1.0f, 2.0f, 3.0f];

        double similarity = SemanticDistance.CosineSimilarity(a, b);

        similarity.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ShouldReturnZero()
    {
        float[] a = [1.0f, 0.0f];
        float[] b = [0.0f, 1.0f];

        double similarity = SemanticDistance.CosineSimilarity(a, b);

        similarity.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ShouldReturnNegativeOne()
    {
        float[] a = [1.0f, 0.0f];
        float[] b = [-1.0f, 0.0f];

        double similarity = SemanticDistance.CosineSimilarity(a, b);

        similarity.Should().BeApproximately(-1.0, 1e-10);
    }

    [Fact]
    public void Compute_IdenticalVectors_ShouldReturnZero()
    {
        float[] a = [1.0f, 2.0f, 3.0f];

        double distance = SemanticDistance.Compute(a, a);

        distance.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Compute_OrthogonalVectors_ShouldReturnOne()
    {
        float[] a = [1.0f, 0.0f];
        float[] b = [0.0f, 1.0f];

        double distance = SemanticDistance.Compute(a, b);

        distance.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ShouldThrow()
    {
        float[] a = [1.0f, 2.0f];
        float[] b = [1.0f, 2.0f, 3.0f];

        Action act = () => SemanticDistance.CosineSimilarity(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_EmptyVectors_ShouldReturnZero()
    {
        float[] a = [];
        float[] b = [];

        double similarity = SemanticDistance.CosineSimilarity(a, b);

        similarity.Should().Be(0.0);
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ShouldReturnZero()
    {
        float[] a = [0.0f, 0.0f];
        float[] b = [1.0f, 2.0f];

        double similarity = SemanticDistance.CosineSimilarity(a, b);

        similarity.Should().Be(0.0);
    }

    [Fact]
    public async Task ComputeAsync_ShouldUsEmbeddingProvider()
    {
        FakeEmbeddingProvider provider = new();
        provider.SetEmbedding("alpha", [1.0f, 0.0f]);
        provider.SetEmbedding("beta", [0.0f, 1.0f]);

        double distance = await SemanticDistance.ComputeAsync(provider, "alpha", "beta");

        distance.Should().BeApproximately(1.0, 1e-10);
    }
}
