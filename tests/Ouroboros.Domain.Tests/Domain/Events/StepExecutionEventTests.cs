namespace Ouroboros.Tests.Domain.Events;

using Ouroboros.Domain.Events;

[Trait("Category", "Unit")]
public class StepExecutionEventTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var aliases = new[] { "MeTTaChat" };

        // Act
        var evt = new StepExecutionEvent(
            id, "MottoChat", aliases, "MeTTaCliSteps",
            "Runs the MeTTa chat", "arg1", timestamp, 150, true, null);

        // Assert
        evt.Id.Should().Be(id);
        evt.TokenName.Should().Be("MottoChat");
        evt.Aliases.Should().ContainSingle().Which.Should().Be("MeTTaChat");
        evt.SourceClass.Should().Be("MeTTaCliSteps");
        evt.Description.Should().Be("Runs the MeTTa chat");
        evt.Arguments.Should().Be("arg1");
        evt.DurationMs.Should().Be(150);
        evt.Success.Should().BeTrue();
        evt.Error.Should().BeNull();
    }

    [Fact]
    public void Start_CreatesEventWithDefaults()
    {
        // Act
        var evt = StepExecutionEvent.Start(
            "TestToken", new[] { "Alias1" }, "TestClass", "Does something", "args");

        // Assert
        evt.Id.Should().NotBeEmpty();
        evt.TokenName.Should().Be("TestToken");
        evt.Aliases.Should().Contain("Alias1");
        evt.SourceClass.Should().Be("TestClass");
        evt.Description.Should().Be("Does something");
        evt.Arguments.Should().Be("args");
        evt.DurationMs.Should().BeNull();
        evt.Success.Should().BeTrue();
        evt.Error.Should().BeNull();
    }

    [Fact]
    public void Start_WithoutArguments_SetsArgumentsNull()
    {
        // Act
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");

        // Assert
        evt.Arguments.Should().BeNull();
    }

    [Fact]
    public void WithCompletion_ReturnsNewEventWithDuration()
    {
        // Arrange
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");

        // Act
        var completed = evt.WithCompletion(250);

        // Assert
        completed.DurationMs.Should().Be(250);
        completed.Success.Should().BeTrue();
        completed.Error.Should().BeNull();
        completed.TokenName.Should().Be("Token");
    }

    [Fact]
    public void WithCompletion_FailedExecution_SetsErrorAndSuccess()
    {
        // Arrange
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");

        // Act
        var failed = evt.WithCompletion(500, success: false, error: "Timeout");

        // Assert
        failed.DurationMs.Should().Be(500);
        failed.Success.Should().BeFalse();
        failed.Error.Should().Be("Timeout");
    }

    [Fact]
    public void GetSynopsis_SuccessWithDuration_FormatsCorrectly()
    {
        // Arrange
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc", "arg1");
        var completed = evt.WithCompletion(100);

        // Act
        var synopsis = completed.GetSynopsis();

        // Assert
        synopsis.Should().Contain("Token");
        synopsis.Should().Contain("(arg1)");
        synopsis.Should().Contain("[100ms]");
    }

    [Fact]
    public void GetSynopsis_FailedExecution_ContainsErrorMarker()
    {
        // Arrange
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");
        var failed = evt.WithCompletion(50, false, "Connection refused");

        // Act
        var synopsis = failed.GetSynopsis();

        // Assert
        synopsis.Should().Contain("Connection refused");
    }

    [Fact]
    public void GetSynopsis_NoArguments_OmitsParentheses()
    {
        // Arrange
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");
        var completed = evt.WithCompletion(100);

        // Act
        var synopsis = completed.GetSynopsis();

        // Assert
        synopsis.Should().NotContain("()");
    }

    [Fact]
    public void InheritsFromPipelineEvent()
    {
        // Act
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Class", "Desc");

        // Assert
        evt.Should().BeAssignableTo<PipelineEvent>();
        evt.Kind.Should().Be("StepExecution");
    }
}
