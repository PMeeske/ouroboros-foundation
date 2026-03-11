// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using System;
using System.Reflection;
using FluentAssertions;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Xunit;

/// <summary>
/// Tests for QdrantEmbodimentVectorStore — constructor validation,
/// static helper method, and record types used for embodiment vectors.
/// Since QdrantClient is sealed infrastructure, we test constructor validation,
/// the static GetPayload helper via reflection, and verify record structures.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantEmbodimentVectorStoreTests
{
    private static Mock<IQdrantCollectionRegistry> CreateMockRegistry()
    {
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.EmbodimentPerceptions))
            .Returns("test_perceptions");
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.EmbodimentStates))
            .Returns("test_states");
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.EmbodimentAffordances))
            .Returns("test_affordances");
        return registry;
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var registry = CreateMockRegistry();

        Action act = () => new QdrantEmbodimentVectorStore(null!, registry.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNullException()
    {
        using var client = new QdrantClient("localhost");

        Action act = () => new QdrantEmbodimentVectorStore(client, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidArgs_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        var act = () =>
        {
            var store = new QdrantEmbodimentVectorStore(client, registry.Object);
            store.DisposeAsync().AsTask().GetAwaiter().GetResult();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_CustomVectorSize_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        var act = () =>
        {
            var store = new QdrantEmbodimentVectorStore(client, registry.Object, vectorSize: 384);
            store.DisposeAsync().AsTask().GetAwaiter().GetResult();
        };

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // GetPayload static helper via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void GetPayload_ExistingKey_ReturnsValue()
    {
        MethodInfo? method = typeof(QdrantEmbodimentVectorStore)
            .GetMethod("GetPayload", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("GetPayload should exist as a private static method");

        var point = new ScoredPoint();
        point.Payload["test_key"] = new Value { StringValue = "test_value" };

        string result = (string)method!.Invoke(null, new object[] { point, "test_key" })!;

        result.Should().Be("test_value");
    }

    [Fact]
    public void GetPayload_MissingKey_ReturnsEmpty()
    {
        MethodInfo? method = typeof(QdrantEmbodimentVectorStore)
            .GetMethod("GetPayload", BindingFlags.NonPublic | BindingFlags.Static);

        var point = new ScoredPoint();

        string result = (string)method!.Invoke(null, new object[] { point, "nonexistent" })!;

        result.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // DisposeAsync
    // ----------------------------------------------------------------

    [Fact]
    public void DisposeAsync_ReturnsCompletedTask()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var store = new QdrantEmbodimentVectorStore(client, registry.Object);

        var task = store.DisposeAsync();

        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void DisposeAsync_CalledTwice_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var store = new QdrantEmbodimentVectorStore(client, registry.Object);

        store.DisposeAsync().AsTask().GetAwaiter().GetResult();

        Action act = () => store.DisposeAsync().AsTask().GetAwaiter().GetResult();
        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // Record types used by the store
    // ----------------------------------------------------------------

    [Fact]
    public void StoredPerceptionMeta_ConstructsCorrectly()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var meta = new StoredPerceptionMeta(id, ts, SensorModality.Audio, "understood", 0.95);

        meta.Id.Should().Be(id);
        meta.Timestamp.Should().Be(ts);
        meta.DominantModality.Should().Be(SensorModality.Audio);
        meta.IntegratedUnderstanding.Should().Be("understood");
        meta.Confidence.Should().Be(0.95);
    }

    [Fact]
    public void RecalledPerception_ConstructsCorrectly()
    {
        var meta = new StoredPerceptionMeta(Guid.NewGuid(), DateTime.UtcNow, SensorModality.Text, "test", 0.8);
        var recalled = new RecalledPerception(meta, 0.92f);

        recalled.Perception.Should().BeSameAs(meta);
        recalled.Score.Should().Be(0.92f);
    }

    [Fact]
    public void EmbodimentStateSnapshot_ConstructsCorrectly()
    {
        var id = Guid.NewGuid();
        var sensors = new HashSet<SensorModality> { SensorModality.Audio, SensorModality.Visual };

        var snapshot = new EmbodimentStateSnapshot(
            id, DateTime.UtcNow, EmbodimentState.Active, "Active state", 0.8, sensors, "user input");

        snapshot.Id.Should().Be(id);
        snapshot.State.Should().Be(EmbodimentState.Active);
        snapshot.Description.Should().Be("Active state");
        snapshot.EnergyLevel.Should().Be(0.8);
        snapshot.ActiveSensors.Should().HaveCount(2);
        snapshot.AttentionTarget.Should().Be("user input");
    }

    [Fact]
    public void RecalledStateSnapshot_ConstructsCorrectly()
    {
        var snapshot = new EmbodimentStateSnapshot(
            Guid.NewGuid(), DateTime.UtcNow, EmbodimentState.Dormant, "desc", 0.5,
            new HashSet<SensorModality>(), null);
        var recalled = new RecalledStateSnapshot(snapshot, 0.88f);

        recalled.Snapshot.Should().BeSameAs(snapshot);
        recalled.Score.Should().Be(0.88f);
    }

    [Fact]
    public void AffordanceRecord_ConstructsCorrectly()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var aff = new AffordanceRecord(id, "Can search", AffordanceType.Custom, "must be online", ts);

        aff.Id.Should().Be(id);
        aff.Description.Should().Be("Can search");
        aff.Type.Should().Be(AffordanceType.Custom);
        aff.Constraints.Should().Be("must be online");
        aff.CreatedAt.Should().Be(ts);
    }

    [Fact]
    public void AffordanceRecord_NullConstraints_Allowed()
    {
        var aff = new AffordanceRecord(Guid.NewGuid(), "desc", AffordanceType.Custom, null, DateTime.UtcNow);

        aff.Constraints.Should().BeNull();
    }

    [Fact]
    public void ScoredAffordance_ConstructsCorrectly()
    {
        var aff = new AffordanceRecord(Guid.NewGuid(), "desc", AffordanceType.Custom, null, DateTime.UtcNow);
        var scored = new ScoredAffordance(aff, 0.75f);

        scored.Affordance.Should().BeSameAs(aff);
        scored.Score.Should().Be(0.75f);
    }

    [Fact]
    public void EmbodimentVectorCounts_ConstructsCorrectly()
    {
        var counts = new EmbodimentVectorCounts(100L, 50L, 25L);

        counts.Perceptions.Should().Be(100L);
        counts.StateSnapshots.Should().Be(50L);
        counts.Affordances.Should().Be(25L);
    }
}
