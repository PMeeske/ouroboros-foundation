using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorResultAdditionalTests
{
    private static OrchestratorMetrics SampleMetrics =>
        OrchestratorMetrics.Initial("test");

    [Fact]
    public void GetMetadata_WrongType_ReturnsDefault()
    {
        // Arrange
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero,
            new Dictionary<string, object> { ["count"] = "not an int" });

        // Act & Assert
        result.GetMetadata<int>("count", -1).Should().Be(-1);
    }

    [Fact]
    public void Failure_WithNullMetadata_CreatesEmptyDictionary()
    {
        // Act
        var result = OrchestratorResult<string>.Failure(
            "error", SampleMetrics, TimeSpan.Zero, null);

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void ToResult_SuccessWithNullOutput_ReturnsFailure()
    {
        // Arrange - Success is true but Output is null
        var result = new OrchestratorResult<string>(
            null, true, null, SampleMetrics, TimeSpan.Zero,
            new Dictionary<string, object>());

        // Act
        var monadResult = result.ToResult();

        // Assert
        monadResult.IsFailure.Should().BeTrue();
        monadResult.Error.Should().Be("Operation failed");
    }

    [Fact]
    public void GetMetadata_NullDefaultValue_ReturnsNull()
    {
        // Arrange
        var result = OrchestratorResult<string>.Ok(
            "output", SampleMetrics, TimeSpan.Zero);

        // Act & Assert
        result.GetMetadata<string>("missing").Should().BeNull();
    }

    [Fact]
    public void Ok_WithNonStringType_WorksCorrectly()
    {
        // Act
        var result = OrchestratorResult<int>.Ok(42, SampleMetrics, TimeSpan.Zero);

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().Be(42);
        result.ToResult().IsSuccess.Should().BeTrue();
        result.ToResult().Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithMetadata_IncludesMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["trace"] = "abc" };

        // Act
        var result = OrchestratorResult<string>.Failure(
            "error", SampleMetrics, TimeSpan.Zero, metadata);

        // Assert
        result.Metadata.Should().ContainKey("trace");
        result.GetMetadata<string>("trace").Should().Be("abc");
    }
}
