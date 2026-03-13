using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class AgentPresenceControllerTests
{
    [Fact]
    public void Constructor_ShouldStartInIdleState()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void Constructor_NullStream_ShouldThrowArgumentNullException()
    {
        var act = () => new AgentPresenceController(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TransitionTo_NewState_ShouldUpdateCurrentState()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.TransitionTo(AgentPresenceState.Listening, "Test");

        controller.CurrentState.Should().Be(AgentPresenceState.Listening);
    }

    [Fact]
    public void TransitionTo_SameState_ShouldNotChange()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);
        StateChangeEventArgs? eventArgs = null;
        controller.StateChanged += (_, args) => eventArgs = args;

        controller.TransitionTo(AgentPresenceState.Idle, "Same state");

        eventArgs.Should().BeNull();
    }

    [Fact]
    public void TransitionTo_ShouldRaiseStateChangedEvent()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);
        StateChangeEventArgs? eventArgs = null;
        controller.StateChanged += (_, args) => eventArgs = args;

        controller.TransitionTo(AgentPresenceState.Processing, "Test transition");

        eventArgs.Should().NotBeNull();
        eventArgs!.PreviousState.Should().Be(AgentPresenceState.Idle);
        eventArgs.NewState.Should().Be(AgentPresenceState.Processing);
        eventArgs.Reason.Should().Be("Test transition");
    }

    [Fact]
    public void ForceState_ShouldSetStateDirectly()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.ForceState(AgentPresenceState.Speaking, "Forced");

        controller.CurrentState.Should().Be(AgentPresenceState.Speaking);
    }

    [Fact]
    public void SpeechCancellation_WhenNotSpeaking_ShouldBeNone()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.SpeechCancellation.Should().Be(CancellationToken.None);
    }

    [Fact]
    public void ProcessingCancellation_WhenProcessing_ShouldNotBeNone()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.TransitionTo(AgentPresenceState.Processing, "Start processing");

        controller.ProcessingCancellation.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public void ListeningCancellation_WhenListening_ShouldNotBeNone()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.TransitionTo(AgentPresenceState.Listening, "Start listening");

        controller.ListeningCancellation.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var stream = new InteractionStream();
        var controller = new AgentPresenceController(stream);

        var act = () =>
        {
            controller.Dispose();
            stream.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void TransitionTo_FromSpeakingToIdle_ShouldCancelSpeechToken()
    {
        using var stream = new InteractionStream();
        using var controller = new AgentPresenceController(stream);

        controller.TransitionTo(AgentPresenceState.Speaking, "Start speaking");
        var speechToken = controller.SpeechCancellation;

        controller.TransitionTo(AgentPresenceState.Idle, "Done speaking");

        speechToken.IsCancellationRequested.Should().BeTrue();
    }
}
