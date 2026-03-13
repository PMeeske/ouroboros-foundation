using System.Reactive.Linq;
using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class InteractionStreamTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithIdleState()
    {
        using var stream = new InteractionStream();

        stream.State.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void PublishTextInput_ShouldEmitOnTextInputs()
    {
        using var stream = new InteractionStream();
        TextInputEvent? received = null;
        using var sub = stream.TextInputs.Subscribe(e => received = e);

        stream.PublishTextInput("Hello", false);

        received.Should().NotBeNull();
        received!.Text.Should().Be("Hello");
        received.IsPartial.Should().BeFalse();
        received.Source.Should().Be(InteractionSource.User);
    }

    [Fact]
    public void PublishVoiceInput_ShouldEmitOnVoiceInputs()
    {
        using var stream = new InteractionStream();
        VoiceInputEvent? received = null;
        using var sub = stream.VoiceInputs.Subscribe(e => received = e);

        stream.PublishVoiceInput("Hello", 0.9, TimeSpan.FromSeconds(1), "en-US");

        received.Should().NotBeNull();
        received!.TranscribedText.Should().Be("Hello");
        received.Confidence.Should().Be(0.9);
    }

    [Fact]
    public void PublishAudioChunk_ShouldEmitOnAudioChunks()
    {
        using var stream = new InteractionStream();
        AudioChunkEvent? received = null;
        using var sub = stream.AudioChunks.Subscribe(e => received = e);

        stream.PublishAudioChunk(new byte[] { 1, 2, 3 }, "pcm16", 16000);

        received.Should().NotBeNull();
        received!.Format.Should().Be("pcm16");
        received.AudioData.Should().HaveCount(3);
    }

    [Fact]
    public void PublishResponse_ShouldEmitOnAgentResponses()
    {
        using var stream = new InteractionStream();
        AgentResponseEvent? received = null;
        using var sub = stream.AgentResponses.Subscribe(e => received = e);

        stream.PublishResponse("Reply text", true);

        received.Should().NotBeNull();
        received!.TextChunk.Should().Be("Reply text");
        received.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void PublishVoiceOutput_ShouldEmitOnVoiceOutputs()
    {
        using var stream = new InteractionStream();
        VoiceOutputEvent? received = null;
        using var sub = stream.VoiceOutputs.Subscribe(e => received = e);

        stream.PublishVoiceOutput(new byte[] { 0xFF }, "mp3", 1.0, true, "neutral", "Hi");

        received.Should().NotBeNull();
        received!.Format.Should().Be("mp3");
        received.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void PublishError_ShouldEmitOnErrors()
    {
        using var stream = new InteractionStream();
        ErrorEvent? received = null;
        using var sub = stream.Errors.Subscribe(e => received = e);

        stream.PublishError("Something broke", null, ErrorCategory.Unknown, true);

        received.Should().NotBeNull();
        received!.Message.Should().Be("Something broke");
        received.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void SendControl_ShouldEmitOnControlEvents()
    {
        using var stream = new InteractionStream();
        ControlEvent? received = null;
        using var sub = stream.ControlEvents.Subscribe(e => received = e);

        stream.SendControl(ControlAction.Reset, "Testing");

        received.Should().NotBeNull();
        received!.Action.Should().Be(ControlAction.Reset);
        received.Reason.Should().Be("Testing");
    }

    [Fact]
    public void SetPresenceState_ShouldUpdateState()
    {
        using var stream = new InteractionStream();

        stream.SetPresenceState(AgentPresenceState.Listening, "Audio started");

        stream.State.Should().Be(AgentPresenceState.Listening);
    }

    [Fact]
    public void SetPresenceState_SameState_ShouldNotEmit()
    {
        using var stream = new InteractionStream();
        int count = 0;
        using var sub = stream.PresenceChanges.Subscribe(_ => count++);

        stream.SetPresenceState(AgentPresenceState.Idle); // same as initial

        count.Should().Be(0);
    }

    [Fact]
    public void PublishHeartbeat_ShouldEmitOnHeartbeats()
    {
        using var stream = new InteractionStream();
        HeartbeatEvent? received = null;
        using var sub = stream.Heartbeats.Subscribe(e => received = e);

        stream.PublishHeartbeat();

        received.Should().NotBeNull();
        received!.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void Dispose_ShouldPreventFurtherPublishing()
    {
        var stream = new InteractionStream();
        TextInputEvent? received = null;
        var sub = stream.TextInputs.Subscribe(e => received = e);

        stream.Dispose();
        sub.Dispose();

        // After dispose, publishing should be a no-op (no exception)
        stream.PublishTextInput("after dispose");
        received.Should().BeNull();
    }

    [Fact]
    public void TimeSinceLastInteraction_ShouldBePositive()
    {
        using var stream = new InteractionStream();

        stream.TimeSinceLastInteraction.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }
}
