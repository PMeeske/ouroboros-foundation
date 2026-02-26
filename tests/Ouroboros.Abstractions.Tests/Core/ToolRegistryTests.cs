using Ouroboros.Abstractions.Core;

namespace Ouroboros.Abstractions.Tests.Core;

[Trait("Category", "Unit")]
public class ToolRegistryTests
{
    [Fact]
    public void Count_Default_ReturnsZero()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Assert
        registry.Count.Should().Be(0);
    }

    [Fact]
    public void Count_IsVirtual_CanBeOverridden()
    {
        // Arrange
        var registry = new TestToolRegistry();

        // Assert
        registry.Count.Should().Be(5);
    }

    [Fact]
    public void ToolRegistry_CanBeInstantiated()
    {
        // Act
        var registry = new ToolRegistry();

        // Assert
        registry.Should().NotBeNull();
    }

    private class TestToolRegistry : ToolRegistry
    {
        public override int Count => 5;
    }
}
