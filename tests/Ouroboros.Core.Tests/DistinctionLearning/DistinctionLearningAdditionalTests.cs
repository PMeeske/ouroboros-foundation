using Ouroboros.Core.DistinctionLearning;

namespace Ouroboros.Core.Tests.DistinctionLearning;

/// <summary>
/// Additional tests for DistinctionStorageConfig, DistinctionWeightMetadata, ActiveDistinction.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionStorageConfigAdditionalTests
{
    [Fact]
    public void CustomConfig_SetsAllProperties()
    {
        var config = new DistinctionStorageConfig(
            "/custom/path",
            512 * 1024L,
            TimeSpan.FromDays(7));

        config.StoragePath.Should().Be("/custom/path");
        config.MaxTotalStorageBytes.Should().Be(512 * 1024L);
        config.DissolvedRetentionPeriod.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public void Default_StoragePath_ContainsOuroboros()
    {
        var config = DistinctionStorageConfig.Default;
        config.StoragePath.Should().Contain("Ouroboros");
    }

    [Fact]
    public void Default_MaxStorage_Is1GB()
    {
        var config = DistinctionStorageConfig.Default;
        config.MaxTotalStorageBytes.Should().Be(1024L * 1024 * 1024);
    }

    [Fact]
    public void Default_RetentionPeriod_Is30Days()
    {
        var config = DistinctionStorageConfig.Default;
        config.DissolvedRetentionPeriod.Should().Be(TimeSpan.FromDays(30));
    }
}

[Trait("Category", "Unit")]
public class DistinctionWeightMetadataAdditionalTests
{
    [Fact]
    public void RecordEquality_WorksForSameValues()
    {
        var time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new DistinctionWeightMetadata("w-1", "/path", 0.9, "Recognition", time, false, 4096);
        var b = new DistinctionWeightMetadata("w-1", "/path", 0.9, "Recognition", time, false, 4096);
        a.Should().Be(b);
    }

    [Fact]
    public void IsDissolved_True_IsTracked()
    {
        var metadata = new DistinctionWeightMetadata("w-1", "/path", 0.1, "Dissolution",
            DateTime.UtcNow, true, 1024);
        metadata.IsDissolved.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class ActiveDistinctionAdditionalTests
{
    [Fact]
    public void With_CreatesCopy()
    {
        var original = new ActiveDistinction("id", "content", 0.5, DateTime.UtcNow, "Stage");
        var modified = original with { Fitness = 0.9 };

        modified.Fitness.Should().Be(0.9);
        modified.Content.Should().Be(original.Content);
        original.Fitness.Should().Be(0.5);
    }
}

[Trait("Category", "Unit")]
public class DistinctionStateAdditionalTests
{
    [Fact]
    public void AddDistinction_MultipleTimes_AccumulatesDistinctions()
    {
        var state = DistinctionState.Initial()
            .AddDistinction("pattern1", 0.8)
            .AddDistinction("pattern2", 0.6)
            .AddDistinction("pattern3", 0.9);

        state.ActiveDistinctions.Should().HaveCount(3);
    }

    [Fact]
    public void GetActiveDistinctionNames_ReturnsAllNames()
    {
        var state = DistinctionState.Initial()
            .AddDistinction("alpha", 0.5)
            .AddDistinction("beta", 0.6);

        var names = state.GetActiveDistinctionNames();
        names.Should().Contain("alpha");
        names.Should().Contain("beta");
    }

    [Fact]
    public void FitnessScores_MapsContentToFitness()
    {
        var state = DistinctionState.Initial()
            .AddDistinction("a", 0.3)
            .AddDistinction("b", 0.7);

        var scores = state.FitnessScores;
        scores["a"].Should().Be(0.3);
        scores["b"].Should().Be(0.7);
    }

    [Fact]
    public void WithCertainty_BelowZero_ClampsToZero()
    {
        var state = DistinctionState.Initial().WithCertainty(-5.0);
        state.EpistemicCertainty.Should().Be(0.0);
    }

    [Fact]
    public void WithCertainty_AboveOne_ClampsToOne()
    {
        var state = DistinctionState.Initial().WithCertainty(10.0);
        state.EpistemicCertainty.Should().Be(1.0);
    }

    [Fact]
    public void NextCycle_UpdatesLastUpdated()
    {
        var state = DistinctionState.Initial();
        var before = DateTime.UtcNow;
        var next = state.NextCycle();
        var after = DateTime.UtcNow;

        next.LastUpdated.Should().BeOnOrAfter(before);
        next.LastUpdated.Should().BeOnOrBefore(after);
    }
}

[Trait("Category", "Unit")]
public class ObservationAdditionalTests
{
    [Fact]
    public void WithCertainPrior_DefaultContext_HasSourceKey()
    {
        var obs = Ouroboros.Core.DistinctionLearning.Observation.WithCertainPrior("test");
        obs.Context.Should().ContainKey("source");
        obs.Context["source"].Should().Be("default");
    }

    [Fact]
    public void WithUncertainPrior_CustomContext_HasSourceKey()
    {
        var obs = Ouroboros.Core.DistinctionLearning.Observation.WithUncertainPrior("test", "sensor");
        obs.Context["source"].Should().Be("sensor");
    }
}
