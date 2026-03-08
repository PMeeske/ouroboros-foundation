using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class AdapterConfigTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var sut = AdapterConfig.Default();

        sut.Rank.Should().Be(8);
        sut.LearningRate.Should().Be(3e-4);
        sut.MaxSteps.Should().Be(1000);
        sut.TargetModules.Should().Be("q_proj,v_proj");
        sut.UseRSLoRA.Should().BeFalse();
    }

    [Fact]
    public void LowRank_HasRank4()
    {
        var sut = AdapterConfig.LowRank();

        sut.Rank.Should().Be(4);
    }

    [Fact]
    public void HighRank_HasRank16()
    {
        var sut = AdapterConfig.HighRank();

        sut.Rank.Should().Be(16);
    }

    [Fact]
    public void Validate_ValidConfig_ReturnsSuccess()
    {
        var sut = AdapterConfig.Default();

        var result = sut.Validate();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(sut);
    }

    [Fact]
    public void Validate_ZeroRank_ReturnsFailure()
    {
        var sut = new AdapterConfig(Rank: 0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Rank");
    }

    [Fact]
    public void Validate_NegativeRank_ReturnsFailure()
    {
        var sut = new AdapterConfig(Rank: -1);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroLearningRate_ReturnsFailure()
    {
        var sut = new AdapterConfig(LearningRate: 0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Learning rate");
    }

    [Fact]
    public void Validate_NegativeLearningRate_ReturnsFailure()
    {
        var sut = new AdapterConfig(LearningRate: -0.001);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroMaxSteps_ReturnsFailure()
    {
        var sut = new AdapterConfig(MaxSteps: 0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Max steps");
    }

    [Fact]
    public void Validate_EmptyTargetModules_ReturnsFailure()
    {
        var sut = new AdapterConfig(TargetModules: "");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Target modules");
    }

    [Fact]
    public void Validate_WhitespaceTargetModules_ReturnsFailure()
    {
        var sut = new AdapterConfig(TargetModules: "   ");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CustomConfig_SetsAllProperties()
    {
        var sut = new AdapterConfig(
            Rank: 32,
            LearningRate: 1e-3,
            MaxSteps: 500,
            TargetModules: "all",
            UseRSLoRA: true);

        sut.Rank.Should().Be(32);
        sut.LearningRate.Should().Be(1e-3);
        sut.MaxSteps.Should().Be(500);
        sut.TargetModules.Should().Be("all");
        sut.UseRSLoRA.Should().BeTrue();
    }
}
