using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class TrainingExampleTests
{
    [Fact]
    public void Creation_SetsProperties()
    {
        var sut = new TrainingExample("input text", "output text", 1.5);

        sut.Input.Should().Be("input text");
        sut.Output.Should().Be("output text");
        sut.Weight.Should().Be(1.5);
    }

    [Fact]
    public void DefaultWeight_IsOne()
    {
        var sut = new TrainingExample("in", "out");

        sut.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Validate_ValidExample_ReturnsSuccess()
    {
        var sut = new TrainingExample("input", "output");

        var result = sut.Validate();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyInput_ReturnsFailure()
    {
        var sut = new TrainingExample("", "output");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Input");
    }

    [Fact]
    public void Validate_WhitespaceInput_ReturnsFailure()
    {
        var sut = new TrainingExample("   ", "output");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyOutput_ReturnsFailure()
    {
        var sut = new TrainingExample("input", "");

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Output");
    }

    [Fact]
    public void Validate_ZeroWeight_ReturnsFailure()
    {
        var sut = new TrainingExample("input", "output", 0.0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Weight");
    }

    [Fact]
    public void Validate_NegativeWeight_ReturnsFailure()
    {
        var sut = new TrainingExample("input", "output", -1.0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MergeStrategy_HasAllExpectedValues()
    {
        Enum.GetValues<MergeStrategy>().Should().HaveCount(4);
        Enum.IsDefined(MergeStrategy.Average).Should().BeTrue();
        Enum.IsDefined(MergeStrategy.Weighted).Should().BeTrue();
        Enum.IsDefined(MergeStrategy.TaskArithmetic).Should().BeTrue();
        Enum.IsDefined(MergeStrategy.TIES).Should().BeTrue();
    }
}
