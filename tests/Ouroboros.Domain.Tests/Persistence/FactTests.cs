using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class FactTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var fact = new Fact(id, "Water boils at 100C", "Physics textbook", 0.99, timestamp);

        fact.Id.Should().Be(id);
        fact.Content.Should().Be("Water boils at 100C");
        fact.Source.Should().Be("Physics textbook");
        fact.Confidence.Should().Be(0.99);
        fact.Timestamp.Should().Be(timestamp);
        fact.Metadata.Should().BeNull();
    }

    [Fact]
    public void Construction_WithMetadata_SetsMetadata()
    {
        var metadata = new Dictionary<string, object> { ["chapter"] = 3 };

        var fact = new Fact(Guid.NewGuid(), "Content", "Source", 0.9, DateTime.UtcNow, metadata);

        fact.Metadata.Should().ContainKey("chapter");
    }

    [Theory]
    [InlineData(0.8, true)]
    [InlineData(0.9, true)]
    [InlineData(1.0, true)]
    [InlineData(0.79, false)]
    [InlineData(0.5, false)]
    [InlineData(0.0, false)]
    public void IsHighConfidence_ReturnsCorrectValue(double confidence, bool expected)
    {
        var fact = new Fact(Guid.NewGuid(), "C", "S", confidence, DateTime.UtcNow);

        fact.IsHighConfidence.Should().Be(expected);
    }

    [Fact]
    public void IsRecent_WithinLast30Days_ReturnsTrue()
    {
        var fact = new Fact(Guid.NewGuid(), "C", "S", 0.9, DateTime.UtcNow.AddDays(-15));

        fact.IsRecent.Should().BeTrue();
    }

    [Fact]
    public void IsRecent_OlderThan30Days_ReturnsFalse()
    {
        var fact = new Fact(Guid.NewGuid(), "C", "S", 0.9, DateTime.UtcNow.AddDays(-31));

        fact.IsRecent.Should().BeFalse();
    }

    [Fact]
    public void IsRecent_Exactly30DaysAgo_ReturnsTrue()
    {
        var fact = new Fact(Guid.NewGuid(), "C", "S", 0.9, DateTime.UtcNow.AddDays(-30));

        fact.IsRecent.Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var f1 = new Fact(id, "C", "S", 0.9, ts);
        var f2 = new Fact(id, "C", "S", 0.9, ts);

        f1.Should().Be(f2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var fact = new Fact(Guid.NewGuid(), "Original", "Source", 0.5, DateTime.UtcNow);

        var modified = fact with { Confidence = 0.95, Content = "Updated" };

        modified.Confidence.Should().Be(0.95);
        modified.Content.Should().Be("Updated");
        fact.Confidence.Should().Be(0.5);
    }
}
