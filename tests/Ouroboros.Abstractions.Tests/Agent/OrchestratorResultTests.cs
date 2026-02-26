using Ouroboros.Agent;
using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorResultTests
{
    private static OrchestratorMetrics SampleMetrics =>
        OrchestratorMetrics.Initial("test");

    [Fact]
    public void Ok_CreatesSuccessfulResult()
    {
        // Act
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.FromSeconds(1));

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().Be("output");
        result.ErrorMessage.Should().BeNull();
        result.ExecutionTime.Should().Be(TimeSpan.FromSeconds(1));
        result.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Ok_WithMetadata_IncludesMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["key"] = "val" };

        // Act
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero, metadata);

        // Assert
        result.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        // Act
        var result = OrchestratorResult<string>.Failure(
            "something failed", SampleMetrics, TimeSpan.FromMilliseconds(100));

        // Assert
        result.Success.Should().BeFalse();
        result.Output.Should().BeNull();
        result.ErrorMessage.Should().Be("something failed");
    }

    [Fact]
    public void ToResult_OnSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var orchResult = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero);

        // Act
        var result = orchResult.ToResult();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("output");
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsFailureResult()
    {
        // Arrange
        var orchResult = OrchestratorResult<string>.Failure(
            "error msg", SampleMetrics, TimeSpan.Zero);

        // Act
        var result = orchResult.ToResult();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error msg");
    }

    [Fact]
    public void ToResult_OnFailure_NoErrorMessage_UsesDefaultMessage()
    {
        // Arrange
        var orchResult = new OrchestratorResult<string>(
            null, false, null, SampleMetrics, TimeSpan.Zero,
            new Dictionary<string, object>());

        // Act
        var result = orchResult.ToResult();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Operation failed");
    }

    [Fact]
    public void GetMetadata_ExistingKey_ReturnsValue()
    {
        // Arrange
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero,
            new Dictionary<string, object> { ["count"] = 42 });

        // Act & Assert
        result.GetMetadata<int>("count").Should().Be(42);
    }

    [Fact]
    public void GetMetadata_NonExistingKey_ReturnsDefault()
    {
        // Arrange
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero);

        // Act & Assert
        result.GetMetadata("missing", "default").Should().Be("default");
    }
}
