using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class StateChangeEventArgsTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle, AgentPresenceState.Listening, "Audio detected");

        args.PreviousState.Should().Be(AgentPresenceState.Idle);
        args.NewState.Should().Be(AgentPresenceState.Listening);
        args.Reason.Should().Be("Audio detected");
        args.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_NullReason_ShouldBeNull()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Speaking, AgentPresenceState.Idle, null);

        args.Reason.Should().BeNull();
    }

    [Fact]
    public void InheritsEventArgs_ShouldBeEventArgs()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle, AgentPresenceState.Processing, "test");

        args.Should().BeAssignableTo<EventArgs>();
    }
}
