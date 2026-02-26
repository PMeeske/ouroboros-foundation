using Ouroboros.Abstractions.Network;

namespace Ouroboros.Abstractions.Tests.Network;

[Trait("Category", "Unit")]
public class WalEntryTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var entry = new WalEntry(id, "NodeAdded", timestamp, "{\"data\":1}", 42L);

        // Assert
        entry.Id.Should().Be(id);
        entry.EntryType.Should().Be("NodeAdded");
        entry.Timestamp.Should().Be(timestamp);
        entry.DataJson.Should().Be("{\"data\":1}");
        entry.SequenceNumber.Should().Be(42L);
    }

    [Fact]
    public void Constructor_NullEntryType_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new WalEntry(
            Guid.NewGuid(), null!, DateTimeOffset.UtcNow, "{}", 0L);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullDataJson_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new WalEntry(
            Guid.NewGuid(), "NodeAdded", DateTimeOffset.UtcNow, null!, 0L);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = DateTimeOffset.UtcNow;

        var a = new WalEntry(id, "NodeAdded", ts, "{}", 1L);
        var b = new WalEntry(id, "NodeAdded", ts, "{}", 1L);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new WalEntry(
            Guid.NewGuid(), "NodeAdded", DateTimeOffset.UtcNow, "{}", 1L);

        // Act
        var modified = original with { SequenceNumber = 99L };

        // Assert
        modified.SequenceNumber.Should().Be(99L);
        modified.EntryType.Should().Be("NodeAdded");
    }
}
