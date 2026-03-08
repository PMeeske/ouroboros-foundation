using System.Collections.Immutable;
using Ouroboros.Abstractions.Network;

namespace Ouroboros.Abstractions.Tests.Network;

[Trait("Category", "Unit")]
public class MonadNodeTests
{
    private static MonadNode CreateSampleNode(
        Guid? id = null,
        string typeName = "TestType",
        string payloadJson = "{}") =>
        new MonadNode(
            id ?? Guid.NewGuid(),
            typeName,
            payloadJson,
            DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty,
            "hash123");

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parentIds = ImmutableArray.Create(Guid.NewGuid());
        var created = DateTimeOffset.UtcNow;

        // Act
        var node = new MonadNode(id, "Type", "{\"key\":1}", created, parentIds, "abc");

        // Assert
        node.Id.Should().Be(id);
        node.TypeName.Should().Be("Type");
        node.PayloadJson.Should().Be("{\"key\":1}");
        node.CreatedAt.Should().Be(created);
        node.ParentIds.Should().HaveCount(1);
        node.Hash.Should().Be("abc");
    }

    [Fact]
    public void Constructor_NullTypeName_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonadNode(
            Guid.NewGuid(), null!, "{}", DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty, "hash");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullPayloadJson_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonadNode(
            Guid.NewGuid(), "Type", null!, DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty, "hash");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullHash_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MonadNode(
            Guid.NewGuid(), "Type", "{}", DateTimeOffset.UtcNow,
            ImmutableArray<Guid>.Empty, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow;
        var parents = ImmutableArray<Guid>.Empty;

        var a = new MonadNode(id, "Type", "{}", created, parents, "hash");
        var b = new MonadNode(id, "Type", "{}", created, parents, "hash");

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentIds_AreNotEqual()
    {
        // Arrange
        var a = CreateSampleNode(Guid.NewGuid());
        var b = CreateSampleNode(Guid.NewGuid());

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = CreateSampleNode();

        // Act
        var modified = original with { TypeName = "ModifiedType" };

        // Assert
        modified.TypeName.Should().Be("ModifiedType");
        modified.Id.Should().Be(original.Id);
    }
}
