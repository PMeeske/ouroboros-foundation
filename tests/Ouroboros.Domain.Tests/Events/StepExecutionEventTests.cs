using Ouroboros.Domain.Events;

namespace Ouroboros.Tests.Events;

[Trait("Category", "Unit")]
public class StepExecutionEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var aliases = new[] { "Alias1", "Alias2" };

        var evt = new StepExecutionEvent(
            id, "TokenA", aliases, "SourceClass", "Does something", "arg1", timestamp, 100L, true, null);

        evt.Id.Should().Be(id);
        evt.TokenName.Should().Be("TokenA");
        evt.Aliases.Should().BeEquivalentTo(aliases);
        evt.SourceClass.Should().Be("SourceClass");
        evt.Description.Should().Be("Does something");
        evt.Arguments.Should().Be("arg1");
        evt.Timestamp.Should().Be(timestamp);
        evt.DurationMs.Should().Be(100L);
        evt.Success.Should().BeTrue();
        evt.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_Defaults_ShouldHaveSuccessTrueAndNullDuration()
    {
        var evt = new StepExecutionEvent(
            Guid.NewGuid(), "Token", Array.Empty<string>(), "Src", "Desc", null, DateTime.UtcNow);

        evt.DurationMs.Should().BeNull();
        evt.Success.Should().BeTrue();
        evt.Error.Should().BeNull();
    }

    [Fact]
    public void Start_ShouldCreateEventWithDefaults()
    {
        var evt = StepExecutionEvent.Start("MyToken", new[] { "Alt" }, "MyClass", "Run step", "arg=1");

        evt.TokenName.Should().Be("MyToken");
        evt.Aliases.Should().ContainSingle().Which.Should().Be("Alt");
        evt.SourceClass.Should().Be("MyClass");
        evt.Description.Should().Be("Run step");
        evt.Arguments.Should().Be("arg=1");
        evt.Id.Should().NotBeEmpty();
        evt.DurationMs.Should().BeNull();
        evt.Success.Should().BeTrue();
    }

    [Fact]
    public void Start_WithoutArguments_ShouldSetArgumentsNull()
    {
        var evt = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Src", "Desc");

        evt.Arguments.Should().BeNull();
    }

    [Fact]
    public void WithCompletion_Success_ShouldReturnNewEventWithDuration()
    {
        var original = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Src", "Desc");

        var completed = original.WithCompletion(250L);

        completed.DurationMs.Should().Be(250L);
        completed.Success.Should().BeTrue();
        completed.Error.Should().BeNull();
        completed.TokenName.Should().Be(original.TokenName);
        completed.Id.Should().Be(original.Id);
    }

    [Fact]
    public void WithCompletion_Failure_ShouldSetErrorAndSuccessFalse()
    {
        var original = StepExecutionEvent.Start("Token", Array.Empty<string>(), "Src", "Desc");

        var completed = original.WithCompletion(500L, false, "Something broke");

        completed.DurationMs.Should().Be(500L);
        completed.Success.Should().BeFalse();
        completed.Error.Should().Be("Something broke");
    }

    [Fact]
    public void GetSynopsis_Success_NoArgs_ShouldFormatCorrectly()
    {
        var evt = new StepExecutionEvent(
            Guid.NewGuid(), "MyToken", Array.Empty<string>(), "Src", "Desc", null, DateTime.UtcNow, 100L, true, null);

        var synopsis = evt.GetSynopsis();

        synopsis.Should().Contain("MyToken");
        synopsis.Should().Contain("[100ms]");
        synopsis.Should().NotContain("(");
    }

    [Fact]
    public void GetSynopsis_Success_WithArgs_ShouldIncludeArgs()
    {
        var evt = new StepExecutionEvent(
            Guid.NewGuid(), "Token", Array.Empty<string>(), "Src", "Desc", "x=1", DateTime.UtcNow, 50L, true, null);

        var synopsis = evt.GetSynopsis();

        synopsis.Should().Contain("(x=1)");
        synopsis.Should().Contain("[50ms]");
    }

    [Fact]
    public void GetSynopsis_Failure_ShouldIncludeError()
    {
        var evt = new StepExecutionEvent(
            Guid.NewGuid(), "Token", Array.Empty<string>(), "Src", "Desc", null, DateTime.UtcNow, 200L, false, "timeout");

        var synopsis = evt.GetSynopsis();

        synopsis.Should().Contain("timeout");
    }

    [Fact]
    public void GetSynopsis_NoDuration_ShouldOmitDuration()
    {
        var evt = new StepExecutionEvent(
            Guid.NewGuid(), "Token", Array.Empty<string>(), "Src", "Desc", null, DateTime.UtcNow);

        var synopsis = evt.GetSynopsis();

        synopsis.Should().NotContain("[");
        synopsis.Should().NotContain("ms]");
    }

    [Fact]
    public void Kind_ShouldBeStepExecution()
    {
        var evt = StepExecutionEvent.Start("T", Array.Empty<string>(), "S", "D");

        evt.Kind.Should().Be("StepExecution");
    }
}
