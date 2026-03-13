using Ouroboros.Core.DistinctionLearning;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public sealed class DistinctionStateTests
{
    [Fact]
    public void Initial_HasEmptyDistinctions()
    {
        var sut = DistinctionState.Initial();

        sut.ActiveDistinctions.Should().BeEmpty();
        sut.EpistemicCertainty.Should().Be(0.0);
        sut.CycleCount.Should().Be(0);
    }

    [Fact]
    public void Void_IsSameAsInitial()
    {
        _ = DistinctionState.Initial();
        var voidState = DistinctionState.Void();

        voidState.ActiveDistinctions.Should().BeEmpty();
        voidState.CycleCount.Should().Be(0);
    }

    [Fact]
    public void WithDistinction_AddsDistinction()
    {
        var sut = DistinctionState.Initial();
        var distinction = new ActiveDistinction("1", "test", 0.8, DateTime.UtcNow, "Manual");

        var updated = sut.WithDistinction(distinction);

        updated.ActiveDistinctions.Should().HaveCount(1);
        updated.ActiveDistinctions[0].Content.Should().Be("test");
    }

    [Fact]
    public void WithDistinction_DoesNotMutateOriginal()
    {
        var sut = DistinctionState.Initial();
        var distinction = new ActiveDistinction("1", "test", 0.8, DateTime.UtcNow, "Manual");

        sut.WithDistinction(distinction);

        sut.ActiveDistinctions.Should().BeEmpty();
    }

    [Fact]
    public void AddDistinction_CreatesNewDistinction()
    {
        var sut = DistinctionState.Initial();

        var updated = sut.AddDistinction("learned pattern", 0.7);

        updated.ActiveDistinctions.Should().HaveCount(1);
        updated.ActiveDistinctions[0].Content.Should().Be("learned pattern");
        updated.ActiveDistinctions[0].Fitness.Should().Be(0.7);
    }

    [Fact]
    public void AddDistinction_SetsManualStage()
    {
        var sut = DistinctionState.Initial();

        var updated = sut.AddDistinction("test", 0.5);

        updated.ActiveDistinctions[0].LearnedAtStage.Should().Be("Manual");
    }

    [Fact]
    public void GetActiveDistinctionNames_ReturnsContentList()
    {
        var sut = DistinctionState.Initial()
            .AddDistinction("alpha", 0.5)
            .AddDistinction("beta", 0.6);

        var names = sut.GetActiveDistinctionNames();

        names.Should().HaveCount(2);
        names.Should().Contain("alpha");
        names.Should().Contain("beta");
    }

    [Fact]
    public void FitnessScores_ReturnsDictionary()
    {
        var sut = DistinctionState.Initial()
            .AddDistinction("alpha", 0.5)
            .AddDistinction("beta", 0.8);

        var scores = sut.FitnessScores;

        scores.Should().HaveCount(2);
        scores["alpha"].Should().Be(0.5);
        scores["beta"].Should().Be(0.8);
    }

    [Fact]
    public void WithCertainty_ClampsToRange()
    {
        var sut = DistinctionState.Initial();

        sut.WithCertainty(1.5).EpistemicCertainty.Should().Be(1.0);
        sut.WithCertainty(-0.5).EpistemicCertainty.Should().Be(0.0);
        sut.WithCertainty(0.7).EpistemicCertainty.Should().Be(0.7);
    }

    [Fact]
    public void NextCycle_IncrementsCycleCount()
    {
        var sut = DistinctionState.Initial();

        var next = sut.NextCycle();

        next.CycleCount.Should().Be(1);
    }

    [Fact]
    public void NextCycle_MultipleCalls_IncrementsSeparately()
    {
        var sut = DistinctionState.Initial()
            .NextCycle()
            .NextCycle()
            .NextCycle();

        sut.CycleCount.Should().Be(3);
    }

    [Fact]
    public void ActiveDistinction_RecordProperties()
    {
        var now = DateTime.UtcNow;
        var sut = new ActiveDistinction("id1", "pattern", 0.9, now, "Recognition");

        sut.Id.Should().Be("id1");
        sut.Content.Should().Be("pattern");
        sut.Fitness.Should().Be(0.9);
        sut.LearnedAt.Should().Be(now);
        sut.LearnedAtStage.Should().Be("Recognition");
    }

    [Fact]
    public void DissolutionStrategy_HasAllExpectedValues()
    {
        Enum.GetValues<DissolutionStrategy>().Should().HaveCount(4);
    }

    [Fact]
    public void DistinctionLearningConstants_HasExpectedValues()
    {
        DistinctionLearningConstants.DefaultFitnessThreshold.Should().Be(0.3);
        DistinctionLearningConstants.DissolutionCycleInterval.Should().Be(10);
    }
}
