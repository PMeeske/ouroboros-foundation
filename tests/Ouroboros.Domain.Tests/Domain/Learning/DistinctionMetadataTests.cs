using Ouroboros.Core.Learning;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Domain.Learning;

[Trait("Category", "Unit")]
public class DistinctionMetadataTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var id = DistinctionId.NewId();
        var createdAt = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc);

        var metadata = new DistinctionMetadata(
            Id: id,
            Circumstance: "arithmetic_operations",
            StoragePath: "/data/distinctions/arith_001.json",
            LearnedAtStage: 3,
            Fitness: 0.85,
            IsDissolved: false,
            CreatedAt: createdAt,
            DissolvedAt: null);

        metadata.Id.Should().Be(id);
        metadata.Circumstance.Should().Be("arithmetic_operations");
        metadata.StoragePath.Should().Be("/data/distinctions/arith_001.json");
        metadata.LearnedAtStage.Should().Be(3);
        metadata.Fitness.Should().Be(0.85);
        metadata.IsDissolved.Should().BeFalse();
        metadata.CreatedAt.Should().Be(createdAt);
        metadata.DissolvedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_Dissolved_HasDissolvedAt()
    {
        var createdAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dissolvedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var metadata = new DistinctionMetadata(
            DistinctionId.NewId(),
            "test_circumstance",
            "/data/test.json",
            1,
            0.3,
            true,
            createdAt,
            dissolvedAt);

        metadata.IsDissolved.Should().BeTrue();
        metadata.DissolvedAt.Should().Be(dissolvedAt);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = DistinctionId.NewId();
        var createdAt = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new DistinctionMetadata(id, "ctx", "/path", 2, 0.7, false, createdAt, null);
        var b = new DistinctionMetadata(id, "ctx", "/path", 2, 0.7, false, createdAt, null);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentId_NotEqual()
    {
        var createdAt = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new DistinctionMetadata(DistinctionId.NewId(), "ctx", "/path", 2, 0.7, false, createdAt, null);
        var b = new DistinctionMetadata(DistinctionId.NewId(), "ctx", "/path", 2, 0.7, false, createdAt, null);

        a.Should().NotBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentFitness_NotEqual()
    {
        var id = DistinctionId.NewId();
        var createdAt = DateTime.UtcNow;

        var a = new DistinctionMetadata(id, "ctx", "/path", 2, 0.7, false, createdAt, null);
        var b = new DistinctionMetadata(id, "ctx", "/path", 2, 0.9, false, createdAt, null);

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesFitness()
    {
        var metadata = new DistinctionMetadata(
            DistinctionId.NewId(), "ctx", "/path", 1, 0.5, false, DateTime.UtcNow, null);

        var modified = metadata with { Fitness = 0.95 };

        modified.Fitness.Should().Be(0.95);
        metadata.Fitness.Should().Be(0.5);
    }

    [Fact]
    public void WithExpression_DissolveDistinction()
    {
        var metadata = new DistinctionMetadata(
            DistinctionId.NewId(), "ctx", "/path", 1, 0.2, false, DateTime.UtcNow, null);

        var dissolved = metadata with
        {
            IsDissolved = true,
            DissolvedAt = DateTime.UtcNow,
        };

        dissolved.IsDissolved.Should().BeTrue();
        dissolved.DissolvedAt.Should().NotBeNull();
        metadata.IsDissolved.Should().BeFalse();
    }

    [Fact]
    public void WithExpression_ChangesLearnedAtStage()
    {
        var metadata = new DistinctionMetadata(
            DistinctionId.NewId(), "ctx", "/path", 1, 0.5, false, DateTime.UtcNow, null);

        var modified = metadata with { LearnedAtStage = 5 };

        modified.LearnedAtStage.Should().Be(5);
    }

    [Fact]
    public void ToString_ContainsCircumstance()
    {
        var metadata = new DistinctionMetadata(
            DistinctionId.NewId(), "my_circumstance", "/path", 1, 0.5, false, DateTime.UtcNow, null);

        metadata.ToString().Should().Contain("my_circumstance");
    }

    [Fact]
    public void GetHashCode_EqualRecords_HaveSameHashCode()
    {
        var id = DistinctionId.NewId();
        var createdAt = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new DistinctionMetadata(id, "ctx", "/path", 2, 0.7, false, createdAt, null);
        var b = new DistinctionMetadata(id, "ctx", "/path", 2, 0.7, false, createdAt, null);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
