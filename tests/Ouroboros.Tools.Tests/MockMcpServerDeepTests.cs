namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for MockMcpServer covering tool listing, execution, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class MockMcpServerDeepTests
{
    [Fact]
    public void ListTools_ReturnsNonEmptyList()
    {
        var tools = MockMcpServer.ListTools();

        tools.Should().NotBeNull();
        tools.Should().NotBeEmpty();
    }

    [Fact]
    public void ListTools_ContainsDslSuggestionTool()
    {
        var tools = MockMcpServer.ListTools();

        tools.Should().Contain(t => t.Name == "dsl_suggestion");
    }

    [Fact]
    public void ListTools_ContainsCodeAnalysisTool()
    {
        var tools = MockMcpServer.ListTools();

        tools.Should().Contain(t => t.Name == "code_analysis");
    }

    [Fact]
    public void ListTools_AllToolsHaveDescriptions()
    {
        var tools = MockMcpServer.ListTools();

        foreach (var tool in tools)
        {
            tool.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void ListTools_AllToolsHaveInputSchemas()
    {
        var tools = MockMcpServer.ListTools();

        foreach (var tool in tools)
        {
            tool.InputSchema.Should().NotBeNull();
        }
    }

    [Fact]
    public void ListTools_ReturnsExactlyTwoTools()
    {
        var tools = MockMcpServer.ListTools();

        tools.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteTool_WithValidToolName_ReturnsSuccess()
    {
        var result = await MockMcpServer.ExecuteTool("any_tool", new { });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteTool_ResultContainsToolName()
    {
        var result = await MockMcpServer.ExecuteTool("my_custom_tool", new { });

        result.Result.Should().Contain("my_custom_tool");
    }

    [Fact]
    public async Task ExecuteTool_WithNullParameters_StillSucceeds()
    {
        var result = await MockMcpServer.ExecuteTool("test_tool", null!);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteTool_WithEmptyToolName_StillReturnsResult()
    {
        var result = await MockMcpServer.ExecuteTool("", new { });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteTool_ResultContainsExpectedPrefix()
    {
        var result = await MockMcpServer.ExecuteTool("test", new { });

        result.Result.Should().Contain("Executed");
    }

    [Fact]
    public async Task ExecuteTool_MultipleCalls_AllSucceed()
    {
        var result1 = await MockMcpServer.ExecuteTool("tool1", new { });
        var result2 = await MockMcpServer.ExecuteTool("tool2", new { });
        var result3 = await MockMcpServer.ExecuteTool("tool3", new { });

        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result3.Success.Should().BeTrue();
    }

    [Fact]
    public void ListTools_ReturnsNewListEachCall()
    {
        var list1 = MockMcpServer.ListTools();
        var list2 = MockMcpServer.ListTools();

        list1.Should().NotBeSameAs(list2);
    }

    [Fact]
    public void ListTools_ToolNamesAreUnique()
    {
        var tools = MockMcpServer.ListTools();
        var names = tools.Select(t => t.Name).ToList();

        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ListTools_DslSuggestionTool_HasCorrectDescription()
    {
        var tools = MockMcpServer.ListTools();
        var dslTool = tools.First(t => t.Name == "dsl_suggestion");

        dslTool.Description.Should().Contain("DSL");
    }
}
