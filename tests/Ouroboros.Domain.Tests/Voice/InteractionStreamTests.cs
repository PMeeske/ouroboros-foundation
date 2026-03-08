namespace Ouroboros.Tests.Voice;

using Ouroboros.Domain.Voice;

[Trait("Category", "Unit")]
public class InteractionStreamTests : IDisposable
{
    private readonly InteractionStream _sut = new();

    [Fact]
    public void State_InitiallyIdle()
    {
        _sut.State.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void PublishTextInput_EmitsTextInputEvent()
    {
        // Arrange
        TextInputEvent? received = null;
        using var sub = _sut.TextInputs.Subscribe(e => received = e);

        // Act
        _sut.PublishTextInput("Hello", isPartial: false);

        // Assert
        received.Should().NotBeNull();
        received!.Text.Should().Be("Hello");
        received.IsPartial.Should().BeFalse();
        received.Source.Should().Be(InteractionSource.User);
    }

    [Fact]
    public void PublishVoiceInput_EmitsVoiceInputEvent()
    {
        // Arrange
        VoiceInputEvent? received = null;
        using var sub = _sut.VoiceInputs.Subscribe(e => received = e);

        // Act
        _sut.PublishVoiceInput("Transcribed text", confidence: 0.95, language: "en-US");

        // Assert
        received.Should().NotBeNull();
        received!.TranscribedText.Should().Be("Transcribed text");
        received.Confidence.Should().Be(0.95);
        received.DetectedLanguage.Should().Be("en-US");
    }

    [Fact]
    public void PublishAudioChunk_EmitsAudioChunkEvent()
    {
        // Arrange
        AudioChunkEvent? received = null;
        using var sub = _sut.AudioChunks.Subscribe(e => received = e);

        byte[] data = new byte[] { 1, 2, 3, 4 };

        // Act
        _sut.PublishAudioChunk(data, "pcm16", sampleRate: 44100, isFinal: true);

        // Assert
        received.Should().NotBeNull();
        received!.AudioData.Should().Equal(data);
        received.Format.Should().Be("pcm16");
        received.SampleRate.Should().Be(44100);
        received.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void PublishThinking_EmitsAgentThinkingEvent()
    {
        // Arrange
        AgentThinkingEvent? received = null;
        using var sub = _sut.AgentThoughts.Subscribe(e => received = e);

        // Act
        _sut.PublishThinking("Processing...", ThinkingPhase.Reasoning, isComplete: true);

        // Assert
        received.Should().NotBeNull();
        received!.ThoughtChunk.Should().Be("Processing...");
        received.Phase.Should().Be(ThinkingPhase.Reasoning);
        received.IsComplete.Should().BeTrue();
        received.Source.Should().Be(InteractionSource.Agent);
    }

    [Fact]
    public void PublishResponse_EmitsAgentResponseEvent()
    {
        // Arrange
        AgentResponseEvent? received = null;
        using var sub = _sut.AgentResponses.Subscribe(e => received = e);

        // Act
        _sut.PublishResponse("Here is my answer", isComplete: true, type: ResponseType.Direct, isSentenceEnd: true);

        // Assert
        received.Should().NotBeNull();
        received!.TextChunk.Should().Be("Here is my answer");
        received.IsComplete.Should().BeTrue();
        received.Type.Should().Be(ResponseType.Direct);
        received.IsSentenceEnd.Should().BeTrue();
    }

    [Fact]
    public void PublishVoiceOutput_EmitsVoiceOutputEvent()
    {
        // Arrange
        VoiceOutputEvent? received = null;
        using var sub = _sut.VoiceOutputs.Subscribe(e => received = e);

        byte[] audio = new byte[] { 10, 20, 30 };

        // Act
        _sut.PublishVoiceOutput(audio, "wav", durationSeconds: 1.5, isComplete: false, emotion: "neutral", text: "Hello");

        // Assert
        received.Should().NotBeNull();
        received!.AudioChunk.Should().Equal(audio);
        received.Format.Should().Be("wav");
        received.DurationSeconds.Should().Be(1.5);
        received.Emotion.Should().Be("neutral");
        received.Text.Should().Be("Hello");
    }

    [Fact]
    public void PublishTextOutput_EmitsTextOutputEvent()
    {
        // Arrange
        TextOutputEvent? received = null;
        using var sub = _sut.TextOutputs.Subscribe(e => received = e);

        // Act
        _sut.PublishTextOutput("Display this", OutputStyle.Emphasis, append: false);

        // Assert
        received.Should().NotBeNull();
        received!.Text.Should().Be("Display this");
        received.Style.Should().Be(OutputStyle.Emphasis);
        received.Append.Should().BeFalse();
    }

    [Fact]
    public void SetPresenceState_ChangesState()
    {
        // Arrange
        PresenceStateEvent? received = null;
        using var sub = _sut.PresenceChanges.Subscribe(e => received = e);

        // Act
        _sut.SetPresenceState(AgentPresenceState.Processing, "test transition");

        // Assert
        received.Should().NotBeNull();
        received!.State.Should().Be(AgentPresenceState.Processing);
        received.PreviousState.Should().Be(AgentPresenceState.Idle);
        received.Reason.Should().Be("test transition");

        _sut.State.Should().Be(AgentPresenceState.Processing);
    }

    [Fact]
    public void SetPresenceState_SameState_DoesNotEmitEvent()
    {
        // Arrange
        int eventCount = 0;
        using var sub = _sut.PresenceChanges.Subscribe(_ => eventCount++);

        // Act
        _sut.SetPresenceState(AgentPresenceState.Idle); // Same as initial state

        // Assert
        eventCount.Should().Be(0);
    }

    [Fact]
    public void SendControl_EmitsControlEvent()
    {
        // Arrange
        ControlEvent? received = null;
        using var sub = _sut.ControlEvents.Subscribe(e => received = e);

        // Act
        _sut.SendControl(ControlAction.InterruptSpeech, "User interruption");

        // Assert
        received.Should().NotBeNull();
        received!.Action.Should().Be(ControlAction.InterruptSpeech);
        received.Reason.Should().Be("User interruption");
    }

    [Fact]
    public void PublishHeartbeat_EmitsHeartbeatEvent()
    {
        // Arrange
        HeartbeatEvent? received = null;
        using var sub = _sut.Heartbeats.Subscribe(e => received = e);

        // Act
        _sut.PublishHeartbeat();

        // Assert
        received.Should().NotBeNull();
        received!.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void PublishError_EmitsErrorEvent()
    {
        // Arrange
        ErrorEvent? received = null;
        using var sub = _sut.Errors.Subscribe(e => received = e);
        var exception = new InvalidOperationException("test error");

        // Act
        _sut.PublishError("Something went wrong", exception, ErrorCategory.Network, isRecoverable: true);

        // Assert
        received.Should().NotBeNull();
        received!.Message.Should().Be("Something went wrong");
        received.Exception.Should().Be(exception);
        received.Category.Should().Be(ErrorCategory.Network);
        received.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void Dispose_SubsequentPublishes_DoNotThrow()
    {
        // Arrange
        _sut.Dispose();

        // Act & Assert - should not throw
        _sut.PublishTextInput("test");
        _sut.PublishVoiceInput("test");
        _sut.PublishThinking("test");
        _sut.PublishResponse("test");
        _sut.PublishHeartbeat();
    }

    [Fact]
    public void TimeSinceLastInteraction_InitiallySmall()
    {
        // Act
        var elapsed = _sut.TimeSinceLastInteraction;

        // Assert
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
