namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionActionTests
{
    [Fact]
    public void Constructor_ToolAction_SetsProperties()
    {
        // Act
        var action = new IntentionAction
        {
            ActionType = "tool",
            ToolName = "web_search",
            ToolInput = "query text",
        };

        // Assert
        action.ActionType.Should().Be("tool");
        action.ToolName.Should().Be("web_search");
        action.ToolInput.Should().Be("query text");
    }

    [Fact]
    public void Constructor_CodeModification_SetsProperties()
    {
        // Act
        var action = new IntentionAction
        {
            ActionType = "code_change",
            FilePath = "/src/MyClass.cs",
            OldCode = "var x = 1;",
            NewCode = "int x = 1;",
        };

        // Assert
        action.ActionType.Should().Be("code_change");
        action.FilePath.Should().Be("/src/MyClass.cs");
        action.OldCode.Should().Be("var x = 1;");
        action.NewCode.Should().Be("int x = 1;");
    }

    [Fact]
    public void Constructor_MessageAction_SetsProperties()
    {
        // Act
        var action = new IntentionAction
        {
            ActionType = "message",
            Message = "Hello user!",
        };

        // Assert
        action.ActionType.Should().Be("message");
        action.Message.Should().Be("Hello user!");
    }

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        // Act
        var action = new IntentionAction { ActionType = "test" };

        // Assert
        action.ToolName.Should().BeNull();
        action.ToolInput.Should().BeNull();
        action.FilePath.Should().BeNull();
        action.OldCode.Should().BeNull();
        action.NewCode.Should().BeNull();
        action.Message.Should().BeNull();
        action.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Parameters_CanStoreValues()
    {
        // Act
        var action = new IntentionAction
        {
            ActionType = "custom",
            Parameters = new Dictionary<string, object>
            {
                ["timeout"] = 30,
                ["retries"] = 3,
            },
        };

        // Assert
        action.Parameters.Should().HaveCount(2);
        action.Parameters["timeout"].Should().Be(30);
    }
}
