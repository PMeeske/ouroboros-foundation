using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ToolResultTests
{
    private static ToolCall CreateToolCall() => new("test-tool", "{}");

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var call = CreateToolCall();
        var duration = TimeSpan.FromMilliseconds(100);
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var sut = new ToolResult("output", call, ExecutionStatus.Success, duration, null, ts);

        sut.Output.Should().Be("output");
        sut.ToolCall.Should().Be(call);
        sut.Status.Should().Be(ExecutionStatus.Success);
        sut.Duration.Should().Be(duration);
        sut.ErrorMessage.Should().BeNull();
        sut.CompletedAt.Should().Be(ts);
    }

    [Fact]
    public void Constructor_WithoutTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = new ToolResult("output", CreateToolCall(), ExecutionStatus.Success, TimeSpan.Zero);
        var after = DateTime.UtcNow;

        sut.CompletedAt.Should().BeOnOrAfter(before);
        sut.CompletedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var call = CreateToolCall();
        var duration = TimeSpan.FromMilliseconds(50);

        var sut = ToolResult.Success("result data", call, duration);

        sut.Output.Should().Be("result data");
        sut.Status.Should().Be(ExecutionStatus.Success);
        sut.ToolCall.Should().Be(call);
        sut.Duration.Should().Be(duration);
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var call = CreateToolCall();
        var duration = TimeSpan.FromMilliseconds(10);

        var sut = ToolResult.Failure("something went wrong", call, duration);

        sut.Output.Should().BeEmpty();
        sut.Status.Should().Be(ExecutionStatus.Failed);
        sut.ErrorMessage.Should().Be("something went wrong");
        sut.Duration.Should().Be(duration);
    }
}
