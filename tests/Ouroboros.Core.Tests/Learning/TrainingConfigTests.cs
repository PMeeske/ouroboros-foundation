using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class TrainingConfigTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var sut = TrainingConfig.Default();

        sut.BatchSize.Should().Be(4);
        sut.Epochs.Should().Be(1);
        sut.IncrementalUpdate.Should().BeFalse();
    }

    [Fact]
    public void Fast_HasLargerBatchSize()
    {
        var sut = TrainingConfig.Fast();

        sut.BatchSize.Should().Be(8);
        sut.Epochs.Should().Be(1);
    }

    [Fact]
    public void Thorough_HasMultipleEpochs()
    {
        var sut = TrainingConfig.Thorough();

        sut.BatchSize.Should().Be(4);
        sut.Epochs.Should().Be(3);
    }

    [Fact]
    public void Validate_ValidConfig_ReturnsSuccess()
    {
        var sut = TrainingConfig.Default();

        var result = sut.Validate();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroBatchSize_ReturnsFailure()
    {
        var sut = new TrainingConfig(BatchSize: 0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Batch size");
    }

    [Fact]
    public void Validate_NegativeBatchSize_ReturnsFailure()
    {
        var sut = new TrainingConfig(BatchSize: -1);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_ZeroEpochs_ReturnsFailure()
    {
        var sut = new TrainingConfig(Epochs: 0);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Epochs");
    }

    [Fact]
    public void Validate_NegativeEpochs_ReturnsFailure()
    {
        var sut = new TrainingConfig(Epochs: -5);

        var result = sut.Validate();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CustomConfig_SetsAllProperties()
    {
        var sut = new TrainingConfig(BatchSize: 16, Epochs: 10, IncrementalUpdate: true);

        sut.BatchSize.Should().Be(16);
        sut.Epochs.Should().Be(10);
        sut.IncrementalUpdate.Should().BeTrue();
    }
}
