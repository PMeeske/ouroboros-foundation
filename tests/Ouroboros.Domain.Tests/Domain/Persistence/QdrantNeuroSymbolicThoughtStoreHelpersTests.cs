// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Persistence;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.Collections;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Persistence;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Xunit;
using QdrantValue = Qdrant.Client.Grpc.Value;

/// <summary>
/// Tests for QdrantNeuroSymbolicThoughtStore.Helpers.cs and Query.cs —
/// serialization helpers, embedding generation, session filter creation,
/// and thought point construction.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantNeuroSymbolicThoughtStoreHelpersTests
{
    private static QdrantNeuroSymbolicThoughtStore CreateStore(
        Func<string, Task<float[]>>? embeddingFunc = null)
    {
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");
        return new QdrantNeuroSymbolicThoughtStore(
            new QdrantClient("localhost"),
            registry.Object,
            new QdrantSettings { DefaultVectorSize = 4 },
            embeddingFunc);
    }

    private static PersistedThought CreateThought(
        string type = "Observation",
        string content = "test thought",
        Guid? parentId = null,
        string? topic = null)
    {
        return new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = type,
            Origin = "test",
            Content = content,
            Confidence = 0.8,
            Relevance = 0.7,
            Timestamp = DateTime.UtcNow,
            ParentThoughtId = parentId,
            Topic = topic,
        };
    }

    // ----------------------------------------------------------------
    // GenerateEmbeddingAsync via reflection
    // ----------------------------------------------------------------

    [Fact]
    public async Task GenerateEmbeddingAsync_WithEmbeddingFunc_ReturnsEmbedding()
    {
        float[] expected = { 1f, 2f, 3f, 4f };
        var store = CreateStore(embeddingFunc: _ => Task.FromResult(expected));

        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("GenerateEmbeddingAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var task = (Task<float[]>)method!.Invoke(store, new object[] { "test", CancellationToken.None })!;
        float[] result = await task;

        result.Should().BeEquivalentTo(expected);

        await store.DisposeAsync();
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WithoutEmbeddingFunc_ReturnsZeroVector()
    {
        var store = CreateStore(embeddingFunc: null);

        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("GenerateEmbeddingAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var task = (Task<float[]>)method!.Invoke(store, new object[] { "test", CancellationToken.None })!;
        float[] result = await task;

        result.Should().HaveCount(4); // _vectorSize = 4
        result.Should().AllSatisfy(v => v.Should().Be(0f));

        await store.DisposeAsync();
    }

    // ----------------------------------------------------------------
    // CreateSessionFilter via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void CreateSessionFilter_ReturnsFilterWithSessionId()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("CreateSessionFilter", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var filter = (Filter)method!.Invoke(null, new object[] { "session-123" })!;

        filter.Should().NotBeNull();
        filter.Must.Should().HaveCount(1);
        filter.Must[0].Field.Key.Should().Be("session_id");
        filter.Must[0].Field.Match.Keyword.Should().Be("session-123");
    }

    // ----------------------------------------------------------------
    // CreateThoughtPoint via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void CreateThoughtPoint_SetsAllPayloadFields()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("CreateThoughtPoint", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var thought = CreateThought(topic: "AI", type: "Analytical");
        float[] embedding = { 1f, 2f, 3f, 4f };

        var point = (PointStruct)method!.Invoke(null, new object[] { "session-1", thought, embedding })!;

        point.Should().NotBeNull();
        point.Id.Uuid.Should().Be(thought.Id.ToString());
        point.Payload.Should().ContainKey("session_id");
        point.Payload["session_id"].StringValue.Should().Be("session-1");
        point.Payload["type"].StringValue.Should().Be("Analytical");
        point.Payload["topic"].StringValue.Should().Be("AI");
    }

    [Fact]
    public void CreateThoughtPoint_WithParentId_IncludesParentThoughtId()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("CreateThoughtPoint", BindingFlags.NonPublic | BindingFlags.Static);

        Guid parentId = Guid.NewGuid();
        var thought = CreateThought(parentId: parentId);
        float[] embedding = { 1f, 2f, 3f, 4f };

        var point = (PointStruct)method!.Invoke(null, new object[] { "session-1", thought, embedding })!;

        point.Payload.Should().ContainKey("parent_thought_id");
        point.Payload["parent_thought_id"].StringValue.Should().Be(parentId.ToString());
    }

    [Fact]
    public void CreateThoughtPoint_WithoutParentId_OmitsParentThoughtId()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("CreateThoughtPoint", BindingFlags.NonPublic | BindingFlags.Static);

        var thought = CreateThought();
        float[] embedding = { 1f, 2f, 3f, 4f };

        var point = (PointStruct)method!.Invoke(null, new object[] { "session-1", thought, embedding })!;

        point.Payload.Should().NotContainKey("parent_thought_id");
    }

    // ----------------------------------------------------------------
    // DeserializeThought via reflection (with IDictionary)
    // ----------------------------------------------------------------

    [Fact]
    public void DeserializeThought_ValidPayload_ReturnsThought()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "DeserializeThought" && m.GetParameters()[0].ParameterType.Name.Contains("IDictionary"));

        Guid id = Guid.NewGuid();
        var payload = new Dictionary<string, QdrantValue>
        {
            ["id"] = new QdrantValue { StringValue = id.ToString() },
            ["type"] = new QdrantValue { StringValue = "Observation" },
            ["origin"] = new QdrantValue { StringValue = "test" },
            ["content"] = new QdrantValue { StringValue = "test content" },
            ["confidence"] = new QdrantValue { DoubleValue = 0.9 },
            ["relevance"] = new QdrantValue { DoubleValue = 0.8 },
            ["timestamp"] = new QdrantValue { StringValue = DateTime.UtcNow.ToString("O") },
        };

        var result = (PersistedThought?)method!.Invoke(null, new object[] { payload });

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Type.Should().Be("Observation");
        result.Content.Should().Be("test content");
    }

    [Fact]
    public void DeserializeThought_MissingRequiredField_ReturnsNull()
    {
        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "DeserializeThought" && m.GetParameters()[0].ParameterType.Name.Contains("IDictionary"));

        // Missing "id" field
        var payload = new Dictionary<string, QdrantValue>
        {
            ["type"] = new QdrantValue { StringValue = "Observation" },
        };

        var result = (PersistedThought?)method!.Invoke(null, new object[] { payload });

        result.Should().BeNull();
    }

    // ----------------------------------------------------------------
    // InferRelationType — additional edge cases
    // ----------------------------------------------------------------

    [Fact]
    public void InferRelationType_AnythingToDecision_ReturnsLeadsTo()
    {
        var store = CreateStore();

        MethodInfo? method = typeof(QdrantNeuroSymbolicThoughtStore)
            .GetMethod("InferRelationType", BindingFlags.NonPublic | BindingFlags.Static);

        var existing = CreateThought("Random");
        var newThought = CreateThought("Decision");

        string result = (string)method!.Invoke(null, new object[] { newThought, existing })!;
        result.Should().Be(ThoughtRelation.Types.LeadsTo);

        store.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // SupportsSemanticSearch
    // ----------------------------------------------------------------

    [Fact]
    public async Task SupportsSemanticSearch_WithFunc_ReturnsTrue()
    {
        var store = CreateStore(_ => Task.FromResult(new float[] { 1f }));
        store.SupportsSemanticSearch.Should().BeTrue();
        await store.DisposeAsync();
    }

    [Fact]
    public async Task SupportsSemanticSearch_WithoutFunc_ReturnsFalse()
    {
        var store = CreateStore(null);
        store.SupportsSemanticSearch.Should().BeFalse();
        await store.DisposeAsync();
    }

    // ----------------------------------------------------------------
    // DisposeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        var store = CreateStore();
        await store.DisposeAsync();

        Func<Task> act = async () => await store.DisposeAsync();
        await act.Should().NotThrowAsync();
    }
}
