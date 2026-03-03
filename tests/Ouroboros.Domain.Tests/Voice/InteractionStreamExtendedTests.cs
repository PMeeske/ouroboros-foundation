using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public sealed class InteractionStreamExtendedTests : IDisposable
{
    private readonly InteractionStream _sut = new();

    [Fact]
    public void Initial_State_Is_Idle()
    {
        _sut.State.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void PublishTextInput_Emits_On_TextInputs()
    {
        TextInputEvent? received = null;
        using IDisposable sub = _sut.TextInputs.Subscribe(e => received = e);

        _sut.PublishTextInput("hello", false);

        received.Should().NotBeNull();
        received!.Text.Should().Be("hello");
        received.IsPartial.Should().BeFalse();
        received.Source.Should().Be(InteractionSource.User);
    }

    [Fact]
    public void PublishVoiceInput_Emits_On_VoiceInputs()
    {
        VoiceInputEvent? received = null;
        using IDisposable sub = _sut.VoiceInputs.Subscribe(e => received = e);

        _sut.PublishVoiceInput("testing", 0.95, TimeSpan.FromSeconds(1), "en", false);

        received.Should().NotBeNull();
        received!.TranscribedText.Should().Be("testing");
        received.Confidence.Should().Be(0.95);
        received.DetectedLanguage.Should().Be("en");
    }

    [Fact]
    public void PublishAudioChunk_Emits_On_AudioChunks()
    {
        AudioChunkEvent? received = null;
        using IDisposable sub = _sut.AudioChunks.Subscribe(e => received = e);

        byte[] data = new byte[] { 1, 2, 3 };
        _sut.PublishAudioChunk(data, "pcm16", 16000, false);

        received.Should().NotBeNull();
        received!.AudioData.Should().BeEquivalentTo(data);
        received.Format.Should().Be("pcm16");
        received.SampleRate.Should().Be(16000);
    }

    [Fact]
    public void PublishThinking_Emits_On_AgentThoughts()
    {
        AgentThinkingEvent? received = null;
        using IDisposable sub = _sut.AgentThoughts.Subscribe(e => received = e);

        _sut.PublishThinking("I think therefore I am", ThinkingPhase.Reasoning, true);

        received.Should().NotBeNull();
        received!.ThoughtChunk.Should().Be("I think therefore I am");
        received.Phase.Should().Be(ThinkingPhase.Reasoning);
        received.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void PublishResponse_Emits_On_AgentResponses()
    {
        AgentResponseEvent? received = null;
        using IDisposable sub = _sut.AgentResponses.Subscribe(e => received = e);

        _sut.PublishResponse("Hello!", true, ResponseType.Direct, true);

        received.Should().NotBeNull();
        received!.TextChunk.Should().Be("Hello!");
        received.IsComplete.Should().BeTrue();
        received.Type.Should().Be(ResponseType.Direct);
        received.IsSentenceEnd.Should().BeTrue();
    }

    [Fact]
    public void PublishVoiceOutput_Emits_On_VoiceOutputs()
    {
        VoiceOutputEvent? received = null;
        using IDisposable sub = _sut.VoiceOutputs.Subscribe(e => received = e);

        byte[] audio = new byte[] { 10, 20 };
        _sut.PublishVoiceOutput(audio, "wav", 1.5, true, "happy", "test");

        received.Should().NotBeNull();
        received!.AudioChunk.Should().BeEquivalentTo(audio);
        received.Format.Should().Be("wav");
        received.DurationSeconds.Should().Be(1.5);
        received.IsComplete.Should().BeTrue();
        received.Emotion.Should().Be("happy");
        received.Text.Should().Be("test");
    }

    [Fact]
    public void PublishTextOutput_Emits_On_TextOutputs()
    {
        TextOutputEvent? received = null;
        using IDisposable sub = _sut.TextOutputs.Subscribe(e => received = e);

        _sut.PublishTextOutput("result", OutputStyle.Emphasis, false);

        received.Should().NotBeNull();
        received!.Text.Should().Be("result");
        received.Style.Should().Be(OutputStyle.Emphasis);
        received.Append.Should().BeFalse();
    }

    [Fact]
    public void SendControl_Emits_On_ControlEvents()
    {
        ControlEvent? received = null;
        using IDisposable sub = _sut.ControlEvents.Subscribe(e => received = e);

        _sut.SendControl(ControlAction.Reset, "testing reset");

        received.Should().NotBeNull();
        received!.Action.Should().Be(ControlAction.Reset);
        received.Reason.Should().Be("testing reset");
    }

    [Fact]
    public void PublishHeartbeat_Emits_On_Heartbeats()
    {
        HeartbeatEvent? received = null;
        using IDisposable sub = _sut.Heartbeats.Subscribe(e => received = e);

        _sut.PublishHeartbeat();

        received.Should().NotBeNull();
        received!.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void PublishError_Emits_On_Errors()
    {
        ErrorEvent? received = null;
        using IDisposable sub = _sut.Errors.Subscribe(e => received = e);

        Exception ex = new InvalidOperationException("test error");
        _sut.PublishError("something failed", ex, ErrorCategory.Processing, true);

        received.Should().NotBeNull();
        received!.Message.Should().Be("something failed");
        received.Exception.Should().BeSameAs(ex);
        received.Category.Should().Be(ErrorCategory.Processing);
        received.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void SetPresenceState_Changes_State()
    {
        _sut.SetPresenceState(AgentPresenceState.Processing, "started");

        _sut.State.Should().Be(AgentPresenceState.Processing);
    }

    [Fact]
    public void SetPresenceState_SameState_NoEmit()
    {
        int emitCount = 0;
        using IDisposable sub = _sut.PresenceChanges.Subscribe(_ => emitCount++);

        _sut.SetPresenceState(AgentPresenceState.Idle, "still idle");

        emitCount.Should().Be(0);
    }

    [Fact]
    public void After_Dispose_PublishTextInput_NoOp()
    {
        TextInputEvent? received = null;
        using IDisposable sub = _sut.TextInputs.Subscribe(e => received = e);

        _sut.Dispose();
        _sut.PublishTextInput("should be ignored");

        // No exception thrown - it's just a no-op after dispose
    }

    [Fact]
    public void TimeSinceLastInteraction_IsPositive()
    {
        _sut.TimeSinceLastInteraction.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void UserInput_Merges_Text_And_Voice()
    {
        List<InteractionEvent> events = new();
        using IDisposable sub = _sut.UserInput.Subscribe(e => events.Add(e));

        _sut.PublishTextInput("text");
        _sut.PublishVoiceInput("voice");

        events.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
