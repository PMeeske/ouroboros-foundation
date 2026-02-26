using Ouroboros.Abstractions.Network;

namespace Ouroboros.Abstractions.Tests.Network;

[Trait("Category", "Unit")]
public class TransitionEdgeTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow;
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var edge = new TransitionEdge(id, sourceId, targetId, "Bind", created, metadata);

        // Assert
        edge.Id.Should().Be(id);
        edge.SourceId.Should().Be(sourceId);
        edge.TargetId.Should().Be(targetId);
        edge.TransitionType.Should().Be("Bind");
        edge.CreatedAt.Should().Be(created);
        edge.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Constructor_NullTransitionType_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TransitionEdge(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            null!, DateTimeOffset.UtcNow, new Dictionary<string, object>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullMetadata_CreatesEmptyDictionary()
    {
        // Act
        var edge = new TransitionEdge(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Map", DateTimeOffset.UtcNow, null!);

        // Assert
        edge.Metadata.Should().NotBeNull();
        edge.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var src = Guid.NewGuid();
        var tgt = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow;
        var meta = new Dictionary<string, object>();

        var a = new TransitionEdge(id, src, tgt, "Bind", created, meta);
        var b = new TransitionEdge(id, src, tgt, "Bind", created, meta);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TransitionEdge(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Bind", DateTimeOffset.UtcNow, new Dictionary<string, object>());

        // Act
        var modified = original with { TransitionType = "Map" };

        // Assert
        modified.TransitionType.Should().Be("Map");
        modified.Id.Should().Be(original.Id);
    }
}
