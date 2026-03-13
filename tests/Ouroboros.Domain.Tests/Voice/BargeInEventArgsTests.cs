using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class BargeInEventArgsTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var args = new BargeInEventArgs(
            AgentPresenceState.Speaking, "Hey stop", BargeInType.SpeechInterrupt);

        args.InterruptedState.Should().Be(AgentPresenceState.Speaking);
        args.UserInput.Should().Be("Hey stop");
        args.Type.Should().Be(BargeInType.SpeechInterrupt);
        args.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_NullUserInput_ShouldBeNull()
    {
        var args = new BargeInEventArgs(
            AgentPresenceState.Processing, null, BargeInType.ProcessingCancel);

        args.UserInput.Should().BeNull();
        args.Type.Should().Be(BargeInType.ProcessingCancel);
    }

    [Fact]
    public void InheritsEventArgs_ShouldBeEventArgs()
    {
        var args = new BargeInEventArgs(AgentPresenceState.Speaking, "text", BargeInType.SpeechInterrupt);

        args.Should().BeAssignableTo<EventArgs>();
    }
}
