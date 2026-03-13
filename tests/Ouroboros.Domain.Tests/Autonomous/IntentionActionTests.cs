namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionActionTests
{
    [Fact]
    public void Constructor_RequiredProperties_AreSet()
    {
        var action = new IntentionAction { ActionType = "tool" };

        action.ActionType.Should().Be("tool");
    }

    [Fact]
    public void Constructor_OptionalProperties_AreNull()
    {
        var action = new IntentionAction { ActionType = "tool" };

        action.ToolName.Should().BeNull();
        action.ToolInput.Should().BeNull();
        action.FilePath.Should().BeNull();
        action.OldCode.Should().BeNull();
        action.NewCode.Should().BeNull();
        action.Message.Should().BeNull();
        action.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ToolAction_SetsToolProperties()
    {
        var action = new IntentionAction
        {
            ActionType = "tool",
            ToolName = "web_search",
            ToolInput = "quantum computing latest"
        };

        action.ActionType.Should().Be("tool");
        action.ToolName.Should().Be("web_search");
        action.ToolInput.Should().Be("quantum computing latest");
    }

    [Fact]
    public void CodeChangeAction_SetsCodeProperties()
    {
        var action = new IntentionAction
        {
            ActionType = "code_change",
            FilePath = "/src/Program.cs",
            OldCode = "var x = 1;",
            NewCode = "var x = 2;"
        };

        action.FilePath.Should().Be("/src/Program.cs");
        action.OldCode.Should().Be("var x = 1;");
        action.NewCode.Should().Be("var x = 2;");
    }

    [Fact]
    public void MessageAction_SetsMessageProperty()
    {
        var action = new IntentionAction
        {
            ActionType = "message",
            Message = "Hello, user!"
        };

        action.Message.Should().Be("Hello, user!");
    }

    [Fact]
    public void Parameters_CanStoreArbitraryData()
    {
        var action = new IntentionAction
        {
            ActionType = "custom",
            Parameters = new Dictionary<string, object>
            {
                ["retries"] = 3,
                ["timeout"] = "30s"
            }
        };

        action.Parameters.Should().HaveCount(2);
        action.Parameters["retries"].Should().Be(3);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new IntentionAction { ActionType = "tool", ToolName = "web_search" };
        var modified = original with { ToolInput = "new input" };

        modified.ToolName.Should().Be("web_search");
        modified.ToolInput.Should().Be("new input");
        original.ToolInput.Should().BeNull();
    }
}
