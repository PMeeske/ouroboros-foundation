namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class MockMcpServerTests
{
    [Fact]
    public void ListTools_ReturnsExpectedTools()
    {
        // Act
        var tools = MockMcpServer.ListTools();

        // Assert
        tools.Should().HaveCount(2);
        tools[0].Name.Should().Be("dsl_suggestion");
        tools[1].Name.Should().Be("code_analysis");
    }

    [Fact]
    public async Task ExecuteTool_ReturnsSuccessfulResult()
    {
        // Act
        var result = await MockMcpServer.ExecuteTool("test_tool", new { });

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Contain("Executed test_tool");
    }

    [Fact]
    public async Task ExecuteTool_WithDifferentNames_ReturnsResultWithName()
    {
        // Act
        var result = await MockMcpServer.ExecuteTool("my_tool", new { param1 = "value" });

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Contain("my_tool");
    }
}
