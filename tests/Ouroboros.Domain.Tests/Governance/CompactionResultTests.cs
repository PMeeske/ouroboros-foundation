using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class CompactionResultTests
{
    [Fact]
    public void Construction_DefaultValues_AreCorrect()
    {
        var result = new CompactionResult();

        result.SnapshotsCompacted.Should().Be(0);
        result.BytesSaved.Should().Be(0);
        result.CompactedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var compactedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = new CompactionResult
        {
            SnapshotsCompacted = 100,
            BytesSaved = 1_048_576,
            CompactedAt = compactedAt
        };

        result.SnapshotsCompacted.Should().Be(100);
        result.BytesSaved.Should().Be(1_048_576);
        result.CompactedAt.Should().Be(compactedAt);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = DateTime.UtcNow;
        var r1 = new CompactionResult { SnapshotsCompacted = 5, BytesSaved = 100, CompactedAt = ts };
        var r2 = new CompactionResult { SnapshotsCompacted = 5, BytesSaved = 100, CompactedAt = ts };

        r1.Should().Be(r2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var result = new CompactionResult { SnapshotsCompacted = 5 };
        var modified = result with { BytesSaved = 2048 };

        modified.BytesSaved.Should().Be(2048);
        modified.SnapshotsCompacted.Should().Be(5);
        result.BytesSaved.Should().Be(0);
    }
}
