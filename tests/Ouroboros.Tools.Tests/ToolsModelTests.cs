using Ouroboros.Tools;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class DslSuggestionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var suggestion = new DslSuggestion("UseDraft", "Helps structure initial response", 0.85);
        suggestion.Step.Should().Be("UseDraft");
        suggestion.Explanation.Should().Be("Helps structure initial response");
        suggestion.Confidence.Should().Be(0.85);
    }
}

[Trait("Category", "Unit")]
public class ToolExecutionResultTests
{
    [Fact]
    public void Success_SetsProperties()
    {
        var result = new ToolExecutionResult(true, "output data");
        result.Success.Should().BeTrue();
        result.Result.Should().Be("output data");
    }

    [Fact]
    public void Failure_SetsProperties()
    {
        var result = new ToolExecutionResult(false, "error occurred");
        result.Success.Should().BeFalse();
        result.Result.Should().Be("error occurred");
    }
}

[Trait("Category", "Unit")]
public class ToolInfoTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var schema = new { type = "object" };
        var info = new ToolInfo("my-tool", "Does something useful", schema);
        info.Name.Should().Be("my-tool");
        info.Description.Should().Be("Does something useful");
        info.InputSchema.Should().Be(schema);
    }
}
