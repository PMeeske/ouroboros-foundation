namespace Ouroboros.Tests.Domain.Persistence;

using Ouroboros.Domain.Persistence;

[Trait("Category", "Unit")]
public class FactTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var fact = new Fact(id, "The sky is blue", "observation", 0.95, timestamp, metadata);

        // Assert
        fact.Id.Should().Be(id);
        fact.Content.Should().Be("The sky is blue");
        fact.Source.Should().Be("observation");
        fact.Confidence.Should().Be(0.95);
        fact.Timestamp.Should().Be(timestamp);
        fact.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void IsHighConfidence_Above08_ReturnsTrue()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.85, DateTime.UtcNow);

        // Assert
        fact.IsHighConfidence.Should().BeTrue();
    }

    [Fact]
    public void IsHighConfidence_Exactly08_ReturnsTrue()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.8, DateTime.UtcNow);

        // Assert
        fact.IsHighConfidence.Should().BeTrue();
    }

    [Fact]
    public void IsHighConfidence_Below08_ReturnsFalse()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.79, DateTime.UtcNow);

        // Assert
        fact.IsHighConfidence.Should().BeFalse();
    }

    [Fact]
    public void IsRecent_WithinLast30Days_ReturnsTrue()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.5, DateTime.UtcNow.AddDays(-15));

        // Assert
        fact.IsRecent.Should().BeTrue();
    }

    [Fact]
    public void IsRecent_OlderThan30Days_ReturnsFalse()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.5, DateTime.UtcNow.AddDays(-31));

        // Assert
        fact.IsRecent.Should().BeFalse();
    }

    [Fact]
    public void Metadata_DefaultsToNull()
    {
        // Act
        var fact = new Fact(Guid.NewGuid(), "content", "src", 0.5, DateTime.UtcNow);

        // Assert
        fact.Metadata.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameFacts_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var fact1 = new Fact(id, "content", "src", 0.9, timestamp);
        var fact2 = new Fact(id, "content", "src", 0.9, timestamp);

        // Assert
        fact1.Should().Be(fact2);
    }
}
