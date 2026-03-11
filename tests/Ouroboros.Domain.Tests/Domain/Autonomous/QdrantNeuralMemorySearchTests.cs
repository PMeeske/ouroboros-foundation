// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Autonomous;
using Qdrant.Client;
using Xunit;

/// <summary>
/// Tests for QdrantNeuralMemory and QdrantNeuralMemory.Search.cs.
/// Since QdrantClient is sealed infrastructure, we test constructor validation,
/// static helpers, and property behavior.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantNeuralMemorySearchTests
{
    private static Mock<IQdrantCollectionRegistry> CreateMockRegistry()
    {
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.NeuronMessages))
            .Returns("test_neuron_messages");
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.Intentions))
            .Returns("test_intentions");
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.Memories))
            .Returns("test_memories");
        return registry;
    }

    private static QdrantSettings CreateSettings(int vectorSize = 768)
    {
        return new QdrantSettings { DefaultVectorSize = vectorSize };
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var registry = CreateMockRegistry();

        Action act = () => new QdrantNeuralMemory(
            null!,
            registry.Object,
            CreateSettings());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNullException()
    {
        using var client = new QdrantClient("localhost");

        Action act = () => new QdrantNeuralMemory(
            client,
            null!,
            CreateSettings());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        Action act = () => new QdrantNeuralMemory(
            client,
            registry.Object,
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidArgs_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        memory.Should().NotBeNull();
    }

    // ----------------------------------------------------------------
    // EmbedFunction property
    // ----------------------------------------------------------------

    [Fact]
    public void EmbedFunction_DefaultNull()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        memory.EmbedFunction.Should().BeNull();
    }

    [Fact]
    public void EmbedFunction_CanBeSet()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        Func<string, CancellationToken, Task<float[]>> func = (_, _) => Task.FromResult(new float[] { 1f });
        memory.EmbedFunction = func;

        memory.EmbedFunction.Should().BeSameAs(func);
    }

    // ----------------------------------------------------------------
    // BuildSearchTextFromPayload via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void BuildSearchTextFromPayload_NeuronMessages_BuildsFromTopicAndContent()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("BuildSearchTextFromPayload", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var payload = new Dictionary<string, string>
        {
            ["topic"] = "test.topic",
            ["content"] = "some content",
        };

        string result = (string)method!.Invoke(memory, new object[] { "test_neuron_messages", payload })!;

        result.Should().Contain("test.topic");
        result.Should().Contain("some content");
    }

    [Fact]
    public void BuildSearchTextFromPayload_Intentions_BuildsFromTitleDescriptionRationale()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("BuildSearchTextFromPayload", BindingFlags.NonPublic | BindingFlags.Instance);

        var payload = new Dictionary<string, string>
        {
            ["title"] = "My Title",
            ["description"] = "My Desc",
            ["rationale"] = "My Rationale",
        };

        string result = (string)method!.Invoke(memory, new object[] { "test_intentions", payload })!;

        result.Should().Contain("My Title");
        result.Should().Contain("My Desc");
        result.Should().Contain("My Rationale");
    }

    [Fact]
    public void BuildSearchTextFromPayload_Memories_BuildsFromCategoryAndContent()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("BuildSearchTextFromPayload", BindingFlags.NonPublic | BindingFlags.Instance);

        var payload = new Dictionary<string, string>
        {
            ["category"] = "fact",
            ["content"] = "AI is interesting",
        };

        string result = (string)method!.Invoke(memory, new object[] { "test_memories", payload })!;

        result.Should().Contain("fact");
        result.Should().Contain("AI is interesting");
    }

    [Fact]
    public void BuildSearchTextFromPayload_UnknownCollection_FallsBackToContent()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("BuildSearchTextFromPayload", BindingFlags.NonPublic | BindingFlags.Instance);

        var payload = new Dictionary<string, string>
        {
            ["content"] = "fallback content",
        };

        string result = (string)method!.Invoke(memory, new object[] { "unknown_collection", payload })!;

        result.Should().Be("fallback content");
    }

    // ----------------------------------------------------------------
    // GetEmbeddingAsync via reflection
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetEmbeddingAsync_NullEmbedFunction_ReturnsNull()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());
        memory.EmbedFunction = null;

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("GetEmbeddingAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var task = (Task<float[]?>)method!.Invoke(memory, new object[] { "test", CancellationToken.None })!;
        float[]? result = await task;

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmbeddingAsync_WithEmbedFunction_ReturnsEmbedding()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        using var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());
        memory.EmbedFunction = (text, ct) => Task.FromResult(new float[] { 0.1f, 0.2f, 0.3f });

        MethodInfo? method = typeof(QdrantNeuralMemory)
            .GetMethod("GetEmbeddingAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var task = (Task<float[]?>)method!.Invoke(memory, new object[] { "test", CancellationToken.None })!;
        float[]? result = await task;

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    // ----------------------------------------------------------------
    // Dispose
    // ----------------------------------------------------------------

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        Action act = () => memory.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var memory = new QdrantNeuralMemory(client, registry.Object, CreateSettings());

        memory.Dispose();

        Action act = () => memory.Dispose();
        act.Should().NotThrow();
    }
}
