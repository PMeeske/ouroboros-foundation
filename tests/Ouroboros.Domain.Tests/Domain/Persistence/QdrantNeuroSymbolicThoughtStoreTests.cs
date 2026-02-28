// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

#pragma warning disable CS0618 // Obsolete types under test

namespace Ouroboros.Tests.Domain.Persistence;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Persistence;
using Xunit;

/// <summary>
/// Tests for <see cref="QdrantNeuroSymbolicThoughtStore"/>.
/// Since QdrantClient is sealed and talks to infrastructure, we test static/internal
/// logic: CosineSimilarity, InferRelationType, deserialization null-safety, constructor
/// validation, and the SupportsSemanticSearch property.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantNeuroSymbolicThoughtStoreTests
{
    // ----------------------------------------------------------------
    // CosineSimilarity - static private helper, tested via reflection
    // ----------------------------------------------------------------

    private static double InvokeCosineSimilarity(float[] a, float[] b)
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("CosineSimilarity", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("CosineSimilarity should exist as a private static method");
        return (double)method!.Invoke(null, new object[] { a, b })!;
    }

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        // Arrange
        float[] vector = { 1f, 2f, 3f };

        // Act
        double similarity = InvokeCosineSimilarity(vector, vector);

        // Assert
        similarity.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        // Arrange
        float[] a = { 1f, 0f, 0f };
        float[] b = { 0f, 1f, 0f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        // Arrange
        float[] a = { 1f, 0f, 0f };
        float[] b = { -1f, 0f, 0f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().BeApproximately(-1.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ReturnsZero()
    {
        // Arrange
        float[] a = { 1f, 2f };
        float[] b = { 1f, 2f, 3f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().Be(0);
    }

    [Fact]
    public void CosineSimilarity_ZeroVectors_ReturnsZero()
    {
        // Arrange
        float[] a = { 0f, 0f, 0f };
        float[] b = { 0f, 0f, 0f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().Be(0);
    }

    [Fact]
    public void CosineSimilarity_OneZeroVector_ReturnsZero()
    {
        // Arrange
        float[] a = { 1f, 2f, 3f };
        float[] b = { 0f, 0f, 0f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().Be(0);
    }

    [Fact]
    public void CosineSimilarity_SimilarVectors_ReturnsHighValue()
    {
        // Arrange
        float[] a = { 1f, 2f, 3f };
        float[] b = { 1.1f, 2.1f, 3.1f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().BeGreaterThan(0.99);
    }

    [Fact]
    public void CosineSimilarity_ScaledVectors_ReturnsOne()
    {
        // Arrange - same direction, different magnitude
        float[] a = { 1f, 2f, 3f };
        float[] b = { 2f, 4f, 6f };

        // Act
        double similarity = InvokeCosineSimilarity(a, b);

        // Assert
        similarity.Should().BeApproximately(1.0, 0.001);
    }

    // ----------------------------------------------------------------
    // InferRelationType - private instance method, tested via reflection
    // ----------------------------------------------------------------

    private static string InvokeInferRelationType(
        QdrantNeuroSymbolicThoughtStore store,
        PersistedThought newThought,
        PersistedThought existingThought)
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("InferRelationType", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull("InferRelationType should exist as a private instance method");
        return (string)method!.Invoke(store, new object[] { newThought, existingThought })!;
    }

    private static PersistedThought CreateThought(
        string type,
        string content = "test",
        Guid? parentId = null)
    {
        return new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            Confidence = 0.8,
            Relevance = 0.7,
            Timestamp = DateTime.UtcNow,
            ParentThoughtId = parentId,
        };
    }

    private static QdrantNeuroSymbolicThoughtStore CreateStoreForReflection()
    {
        // Use the obsolete constructor for testing pure logic (no Qdrant connection needed)
#pragma warning disable CS0618
        return new QdrantNeuroSymbolicThoughtStore(
            new QdrantNeuroSymbolicConfig(
                Endpoint: "http://localhost:6334",
                VectorSize: 4));
#pragma warning restore CS0618
    }

    [Fact]
    public void InferRelationType_ObservationToAnalytical_ReturnsLeadsTo()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Observation");
        var newThought = CreateThought("Analytical");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.LeadsTo);
    }

    [Fact]
    public void InferRelationType_AnalyticalToDecision_ReturnsLeadsTo()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Analytical");
        var newThought = CreateThought("Decision");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.LeadsTo);
    }

    [Fact]
    public void InferRelationType_EmotionalToSelfReflection_ReturnsTriggers()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Emotional");
        var newThought = CreateThought("SelfReflection");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.Triggers);
    }

    [Fact]
    public void InferRelationType_MemoryRecallToAnything_ReturnsSupports()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("MemoryRecall");
        var newThought = CreateThought("SomeOtherType");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.Supports);
    }

    [Fact]
    public void InferRelationType_StrategicToDecision_ReturnsLeadsTo()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Strategic");
        var newThought = CreateThought("Decision");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.LeadsTo);
    }

    [Fact]
    public void InferRelationType_SynthesisToAnything_ReturnsAbstracts()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Synthesis");
        var newThought = CreateThought("Something");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.Abstracts);
    }

    [Fact]
    public void InferRelationType_CreativeToAnything_ReturnsElaborates()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Creative");
        var newThought = CreateThought("Other");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.Elaborates);
    }

    [Fact]
    public void InferRelationType_AnythingToSynthesis_ReturnsPartOf()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("Observation");
        var newThought = CreateThought("Synthesis");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.PartOf);
    }

    [Fact]
    public void InferRelationType_AnythingToDecision_ReturnsLeadsTo()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("SomeType");
        var newThought = CreateThought("Decision");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.LeadsTo);
    }

    [Fact]
    public void InferRelationType_ChildOfParent_ReturnsRefines()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("TypeA");
        var newThought = CreateThought("TypeB", parentId: existing.Id);

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.Refines);
    }

    [Fact]
    public void InferRelationType_NoSpecialRelation_ReturnsSimilarTo()
    {
        // Arrange
        var store = CreateStoreForReflection();
        var existing = CreateThought("TypeA");
        var newThought = CreateThought("TypeB");

        // Act
        string relationType = InvokeInferRelationType(store, newThought, existing);

        // Assert
        relationType.Should().Be(ThoughtRelation.Types.SimilarTo);
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_Obsolete_NullConfig_ThrowsArgumentNull()
    {
        // Act
#pragma warning disable CS0618
        Action act = () => new QdrantNeuroSymbolicThoughtStore((QdrantNeuroSymbolicConfig)null!);
#pragma warning restore CS0618

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ----------------------------------------------------------------
    // SupportsSemanticSearch property
    // ----------------------------------------------------------------

    [Fact]
    public async Task SupportsSemanticSearch_WithEmbeddingFunc_ReturnsTrue()
    {
        // Arrange
#pragma warning disable CS0618
        var store = new QdrantNeuroSymbolicThoughtStore(
            new QdrantNeuroSymbolicConfig(),
            embeddingFunc: _ => Task.FromResult(new float[] { 1f, 2f }));
#pragma warning restore CS0618

        try
        {
            // Assert
            store.SupportsSemanticSearch.Should().BeTrue();
        }
        finally
        {
            await store.DisposeAsync();
        }
    }

    [Fact]
    public async Task SupportsSemanticSearch_WithoutEmbeddingFunc_ReturnsFalse()
    {
        // Arrange
#pragma warning disable CS0618
        var store = new QdrantNeuroSymbolicThoughtStore(
            new QdrantNeuroSymbolicConfig(),
            embeddingFunc: null);
#pragma warning restore CS0618

        try
        {
            // Assert
            store.SupportsSemanticSearch.Should().BeFalse();
        }
        finally
        {
            await store.DisposeAsync();
        }
    }

    // ----------------------------------------------------------------
    // ThoughtRelation.Types constants
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("caused_by")]
    [InlineData("leads_to")]
    [InlineData("contradicts")]
    [InlineData("supports")]
    [InlineData("refines")]
    [InlineData("abstracts")]
    [InlineData("elaborates")]
    [InlineData("similar_to")]
    [InlineData("instance_of")]
    [InlineData("part_of")]
    [InlineData("triggers")]
    [InlineData("resolves")]
    public void ThoughtRelation_Types_ContainsExpectedValues(string expectedType)
    {
        // Arrange
        var allTypes = new[]
        {
            ThoughtRelation.Types.CausedBy,
            ThoughtRelation.Types.LeadsTo,
            ThoughtRelation.Types.Contradicts,
            ThoughtRelation.Types.Supports,
            ThoughtRelation.Types.Refines,
            ThoughtRelation.Types.Abstracts,
            ThoughtRelation.Types.Elaborates,
            ThoughtRelation.Types.SimilarTo,
            ThoughtRelation.Types.InstanceOf,
            ThoughtRelation.Types.PartOf,
            ThoughtRelation.Types.Triggers,
            ThoughtRelation.Types.Resolves,
        };

        // Assert
        allTypes.Should().Contain(expectedType);
    }

    // ----------------------------------------------------------------
    // ThoughtResult.Types constants
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("action")]
    [InlineData("response")]
    [InlineData("insight")]
    [InlineData("decision")]
    [InlineData("error")]
    [InlineData("deferred")]
    public void ThoughtResult_Types_ContainsExpectedValues(string expectedType)
    {
        // Arrange
        var allTypes = new[]
        {
            ThoughtResult.Types.Action,
            ThoughtResult.Types.Response,
            ThoughtResult.Types.Insight,
            ThoughtResult.Types.Decision,
            ThoughtResult.Types.Error,
            ThoughtResult.Types.Deferred,
        };

        // Assert
        allTypes.Should().Contain(expectedType);
    }
}
