using Ouroboros.Core.DistinctionLearning;

namespace Ouroboros.Core.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public class ActiveDistinctionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var distinction = new ActiveDistinction("d-1", "color is red", 0.85, now, "Recognition");

        distinction.Id.Should().Be("d-1");
        distinction.Content.Should().Be("color is red");
        distinction.Fitness.Should().Be(0.85);
        distinction.LearnedAt.Should().Be(now);
        distinction.LearnedAtStage.Should().Be("Recognition");
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new ActiveDistinction("d-1", "content", 0.5, time, "stage");
        var b = new ActiveDistinction("d-1", "content", 0.5, time, "stage");
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class DissolutionStrategyEnumTests
{
    [Theory]
    [InlineData(DissolutionStrategy.FitnessThreshold)]
    [InlineData(DissolutionStrategy.OldestFirst)]
    [InlineData(DissolutionStrategy.LeastRecentlyUsed)]
    [InlineData(DissolutionStrategy.All)]
    public void AllValues_AreDefined(DissolutionStrategy strategy)
    {
        Enum.IsDefined(typeof(DissolutionStrategy), strategy).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class DistinctionLearningConstantsTests
{
    [Fact]
    public void DefaultFitnessThreshold_IsExpectedValue()
    {
        DistinctionLearningConstants.DefaultFitnessThreshold.Should().Be(0.3);
    }

    [Fact]
    public void DissolutionCycleInterval_IsExpectedValue()
    {
        DistinctionLearningConstants.DissolutionCycleInterval.Should().Be(10);
    }
}

[Trait("Category", "Unit")]
public class DistinctionStorageConfigTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var config = new DistinctionStorageConfig("/tmp/test", 1024, TimeSpan.FromDays(7));
        config.StoragePath.Should().Be("/tmp/test");
        config.MaxTotalStorageBytes.Should().Be(1024);
        config.DissolvedRetentionPeriod.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Default_HasReasonableValues()
    {
        var config = DistinctionStorageConfig.Default;
        config.StoragePath.Should().NotBeNullOrEmpty();
        config.MaxTotalStorageBytes.Should().Be(1024L * 1024 * 1024);
        config.DissolvedRetentionPeriod.Should().Be(TimeSpan.FromDays(30));
    }
}

[Trait("Category", "Unit")]
public class DistinctionWeightMetadataTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var metadata = new DistinctionWeightMetadata(
            "w-1", "/path/to/weights", 0.9, "Recognition", now, false, 4096);

        metadata.Id.Should().Be("w-1");
        metadata.Path.Should().Be("/path/to/weights");
        metadata.Fitness.Should().Be(0.9);
        metadata.LearnedAtStage.Should().Be("Recognition");
        metadata.CreatedAt.Should().Be(now);
        metadata.IsDissolved.Should().BeFalse();
        metadata.SizeBytes.Should().Be(4096);
    }
}
