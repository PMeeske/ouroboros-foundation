using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ArchiveResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var result = new ArchiveResult
        {
            ArchiveLocation = "/archive/2025"
        };

        result.ArchiveLocation.Should().Be("/archive/2025");
        result.SnapshotsArchived.Should().Be(0);
        result.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var archivedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var result = new ArchiveResult
        {
            SnapshotsArchived = 42,
            ArchiveLocation = "/cold-storage",
            ArchivedAt = archivedAt
        };

        result.SnapshotsArchived.Should().Be(42);
        result.ArchiveLocation.Should().Be("/cold-storage");
        result.ArchivedAt.Should().Be(archivedAt);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = DateTime.UtcNow;
        var r1 = new ArchiveResult { ArchiveLocation = "/a", SnapshotsArchived = 5, ArchivedAt = ts };
        var r2 = new ArchiveResult { ArchiveLocation = "/a", SnapshotsArchived = 5, ArchivedAt = ts };

        r1.Should().Be(r2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var result = new ArchiveResult { ArchiveLocation = "/a" };
        var modified = result with { SnapshotsArchived = 10 };

        modified.SnapshotsArchived.Should().Be(10);
        result.SnapshotsArchived.Should().Be(0);
    }
}
