using System.Collections.Immutable;
using Ouroboros.Abstractions.Network;

namespace Ouroboros.Abstractions.Tests.Network;

[Trait("Category", "Unit")]
public class NetworkAdditionalTests
{
    [Fact]
    public void MonadNode_WithMultipleParentIds_AllAccessible()
    {
        // Arrange
        var parent1 = Guid.NewGuid();
        var parent2 = Guid.NewGuid();
        var parentIds = ImmutableArray.Create(parent1, parent2);

        // Act
        var node = new MonadNode(
            Guid.NewGuid(), "Type", "{}", DateTimeOffset.UtcNow,
            parentIds, "hash");

        // Assert
        node.ParentIds.Should().HaveCount(2);
        node.ParentIds.Should().Contain(parent1);
        node.ParentIds.Should().Contain(parent2);
    }

    [Fact]
    public void MonadNode_EmptyParentIds_IsValid()
    {
        // Act
        var node = new MonadNode(
            Guid.NewGuid(), "Type", "{}", DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty, "hash");

        // Assert
        node.ParentIds.Should().BeEmpty();
    }

    [Fact]
    public void TransitionEdge_WithMetadataEntries_AllAccessible()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        // Act
        var edge = new TransitionEdge(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Bind", DateTimeOffset.UtcNow, metadata);

        // Assert
        edge.Metadata.Should().HaveCount(2);
        edge.Metadata["key1"].Should().Be("value1");
    }

    [Fact]
    public void WalEntry_SequenceZero_IsValid()
    {
        // Act
        var entry = new WalEntry(
            Guid.NewGuid(), "NodeAdded", DateTimeOffset.UtcNow,
            "{}", 0L);

        // Assert
        entry.SequenceNumber.Should().Be(0L);
    }

    [Fact]
    public void WalEntry_NegativeSequenceNumber_IsAllowed()
    {
        // Act
        var entry = new WalEntry(
            Guid.NewGuid(), "EdgeRemoved", DateTimeOffset.UtcNow,
            "{\"id\": 1}", -1L);

        // Assert
        entry.SequenceNumber.Should().Be(-1L);
    }

    [Fact]
    public void MonadNode_ToString_ContainsTypeName()
    {
        // Arrange
        var node = new MonadNode(
            Guid.NewGuid(), "TestType", "{}", DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty, "hash");

        // Act
        var str = node.ToString();

        // Assert
        str.Should().Contain("TestType");
    }

    [Fact]
    public void TransitionEdge_ToString_ContainsTransitionType()
    {
        // Arrange
        var edge = new TransitionEdge(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "MapTransition", DateTimeOffset.UtcNow, new Dictionary<string, object>());

        // Act
        var str = edge.ToString();

        // Assert
        str.Should().Contain("MapTransition");
    }
}
