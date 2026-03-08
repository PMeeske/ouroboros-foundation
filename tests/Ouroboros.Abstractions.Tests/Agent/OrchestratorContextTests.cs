using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorContextTests
{
    [Fact]
    public void Create_GeneratesOperationId()
    {
        // Act
        var context = OrchestratorContext.Create();

        // Assert
        context.OperationId.Should().NotBeNullOrEmpty();
        context.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithMetadata_SetsMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var context = OrchestratorContext.Create(metadata);

        // Assert
        context.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void GetMetadata_ExistingKey_ReturnsValue()
    {
        // Arrange
        var context = OrchestratorContext.Create(
            new Dictionary<string, object> { ["count"] = 42 });

        // Act & Assert
        context.GetMetadata<int>("count").Should().Be(42);
    }

    [Fact]
    public void GetMetadata_NonExistingKey_ReturnsDefault()
    {
        // Arrange
        var context = OrchestratorContext.Create();

        // Act & Assert
        context.GetMetadata("missing", "fallback").Should().Be("fallback");
    }

    [Fact]
    public void GetMetadata_WrongType_ReturnsDefault()
    {
        // Arrange
        var context = OrchestratorContext.Create(
            new Dictionary<string, object> { ["key"] = "string value" });

        // Act & Assert
        context.GetMetadata<int>("key", -1).Should().Be(-1);
    }

    [Fact]
    public void WithMetadata_AddsKeyToNewContext()
    {
        // Arrange
        var original = OrchestratorContext.Create();

        // Act
        var updated = original.WithMetadata("newKey", "newValue");

        // Assert
        updated.GetMetadata<string>("newKey").Should().Be("newValue");
        original.Metadata.Should().NotContainKey("newKey");
    }

    [Fact]
    public void WithMetadata_OverwritesExistingKey()
    {
        // Arrange
        var original = OrchestratorContext.Create(
            new Dictionary<string, object> { ["key"] = "old" });

        // Act
        var updated = original.WithMetadata("key", "new");

        // Assert
        updated.GetMetadata<string>("key").Should().Be("new");
        original.GetMetadata<string>("key").Should().Be("old");
    }

    [Fact]
    public void WithMetadata_PreservesOperationId()
    {
        // Arrange
        var original = OrchestratorContext.Create();

        // Act
        var updated = original.WithMetadata("key", "value");

        // Assert
        updated.OperationId.Should().Be(original.OperationId);
    }
}
