using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public sealed class AgentPresenceControllerTests : IDisposable
{
    private readonly InteractionStream _stream;
    private readonly AgentPresenceController _sut;

    public AgentPresenceControllerTests()
    {
        _stream = new InteractionStream();
        _sut = new AgentPresenceController(_stream);
    }

    [Fact]
    public void Constructor_NullStream_Throws()
    {
        Action act = () => new AgentPresenceController(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Initial_State_Is_Idle()
    {
        _sut.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void TransitionTo_Changes_State()
    {
        _sut.TransitionTo(AgentPresenceState.Processing, "test");

        _sut.CurrentState.Should().Be(AgentPresenceState.Processing);
    }

    [Fact]
    public void TransitionTo_SameState_NoChange()
    {
        StateChangeEventArgs? args = null;
        _sut.StateChanged += (_, e) => args = e;

        _sut.TransitionTo(AgentPresenceState.Idle, "already idle");

        args.Should().BeNull(); // no event fired for same state
    }

    [Fact]
    public void TransitionTo_Fires_StateChanged_Event()
    {
        StateChangeEventArgs? capturedArgs = null;
        _sut.StateChanged += (_, e) => capturedArgs = e;

        _sut.TransitionTo(AgentPresenceState.Listening, "test transition");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.PreviousState.Should().Be(AgentPresenceState.Idle);
        capturedArgs.NewState.Should().Be(AgentPresenceState.Listening);
        capturedArgs.Reason.Should().Be("test transition");
    }

    [Fact]
    public void TransitionTo_Processing_Creates_CancellationToken()
    {
        _sut.TransitionTo(AgentPresenceState.Processing, "start processing");

        _sut.ProcessingCancellation.Should().NotBe(CancellationToken.None);
        _sut.ProcessingCancellation.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void TransitionTo_Speaking_Creates_CancellationToken()
    {
        _sut.TransitionTo(AgentPresenceState.Speaking, "start speaking");

        _sut.SpeechCancellation.Should().NotBe(CancellationToken.None);
        _sut.SpeechCancellation.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void TransitionTo_Listening_Creates_CancellationToken()
    {
        _sut.TransitionTo(AgentPresenceState.Listening, "start listening");

        _sut.ListeningCancellation.Should().NotBe(CancellationToken.None);
        _sut.ListeningCancellation.CanBeCanceled.Should().BeTrue();
    }

    [Fact]
    public void TransitionFrom_Speaking_Cancels_Speech()
    {
        _sut.TransitionTo(AgentPresenceState.Speaking, "start");
        CancellationToken speechToken = _sut.SpeechCancellation;

        _sut.TransitionTo(AgentPresenceState.Idle, "done");

        speechToken.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void TransitionFrom_Processing_Cancels_Processing()
    {
        _sut.TransitionTo(AgentPresenceState.Processing, "start");
        CancellationToken processingToken = _sut.ProcessingCancellation;

        _sut.TransitionTo(AgentPresenceState.Idle, "done");

        processingToken.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void ForceState_Overrides_CurrentState()
    {
        _sut.TransitionTo(AgentPresenceState.Processing, "busy");
        _sut.ForceState(AgentPresenceState.Idle, "forced reset");

        _sut.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void ForceState_Cleans_Up_All_Tokens()
    {
        _sut.TransitionTo(AgentPresenceState.Speaking, "start");
        CancellationToken speechToken = _sut.SpeechCancellation;

        _sut.ForceState(AgentPresenceState.Processing, "forced");

        speechToken.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public void Dispose_Can_Be_Called_Multiple_Times()
    {
        _sut.Dispose();
        Action act = () => _sut.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void State_Observable_Emits_Changes()
    {
        List<AgentPresenceState> states = new();
        using IDisposable sub = _sut.State.Subscribe(s => states.Add(s));

        _sut.TransitionTo(AgentPresenceState.Listening, "listen");
        _sut.TransitionTo(AgentPresenceState.Processing, "process");

        states.Should().Contain(AgentPresenceState.Idle); // initial
        states.Should().Contain(AgentPresenceState.Listening);
        states.Should().Contain(AgentPresenceState.Processing);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _stream.Dispose();
    }
}
