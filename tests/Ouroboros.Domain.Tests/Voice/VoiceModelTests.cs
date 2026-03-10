namespace Ouroboros.Tests.Voice;

using Ouroboros.Domain.Voice;

[Trait("Category", "Unit")]
public class VoiceModelTests
{
    // ========================================================================
    // InteractionEvent (base record) tests via concrete subtypes
    // ========================================================================

    [Fact]
    public void InteractionEvent_DefaultId_IsNotEmpty()
    {
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello" };
        evt.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void InteractionEvent_DefaultTimestamp_IsRecentUtc()
    {
        var before = DateTimeOffset.UtcNow;
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello" };
        var after = DateTimeOffset.UtcNow;

        evt.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void InteractionEvent_CorrelationId_DefaultsToNull()
    {
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello" };
        evt.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void InteractionEvent_CorrelationId_CanBeSet()
    {
        var correlationId = Guid.NewGuid();
        var evt = new TextInputEvent
        {
            Source = InteractionSource.User,
            Text = "hello",
            CorrelationId = correlationId,
        };
        evt.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void InteractionEvent_TwoInstances_HaveDistinctIds()
    {
        var evt1 = new TextInputEvent { Source = InteractionSource.User, Text = "a" };
        var evt2 = new TextInputEvent { Source = InteractionSource.User, Text = "b" };
        evt1.Id.Should().NotBe(evt2.Id);
    }

    [Fact]
    public void InteractionEvent_CustomId_IsPreserved()
    {
        var id = Guid.NewGuid();
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello", Id = id };
        evt.Id.Should().Be(id);
    }

    // ========================================================================
    // TextInputEvent
    // ========================================================================

    [Fact]
    public void TextInputEvent_Construction_SetsRequiredProperties()
    {
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello world" };

        evt.Text.Should().Be("hello world");
        evt.Source.Should().Be(InteractionSource.User);
    }

    [Fact]
    public void TextInputEvent_IsPartial_DefaultsToFalse()
    {
        var evt = new TextInputEvent { Source = InteractionSource.User, Text = "hello" };
        evt.IsPartial.Should().BeFalse();
    }

    [Fact]
    public void TextInputEvent_IsPartial_CanBeSetToTrue()
    {
        var evt = new TextInputEvent
        {
            Source = InteractionSource.User,
            Text = "hel",
            IsPartial = true,
        };
        evt.IsPartial.Should().BeTrue();
    }

    [Fact]
    public void TextInputEvent_WithExpression_CreatesModifiedCopy()
    {
        var original = new TextInputEvent { Source = InteractionSource.User, Text = "hello" };
        var modified = original with { Text = "world" };

        modified.Text.Should().Be("world");
        original.Text.Should().Be("hello");
        modified.Id.Should().Be(original.Id);
    }

    // ========================================================================
    // TextOutputEvent
    // ========================================================================

    [Fact]
    public void TextOutputEvent_Construction_SetsRequiredProperties()
    {
        var evt = new TextOutputEvent { Source = InteractionSource.Agent, Text = "response" };

        evt.Text.Should().Be("response");
        evt.Source.Should().Be(InteractionSource.Agent);
    }

    [Fact]
    public void TextOutputEvent_Style_DefaultsToNormal()
    {
        var evt = new TextOutputEvent { Source = InteractionSource.Agent, Text = "hi" };
        evt.Style.Should().Be(OutputStyle.Normal);
    }

    [Fact]
    public void TextOutputEvent_Append_DefaultsToTrue()
    {
        var evt = new TextOutputEvent { Source = InteractionSource.Agent, Text = "hi" };
        evt.Append.Should().BeTrue();
    }

    [Fact]
    public void TextOutputEvent_CustomStyle_IsPreserved()
    {
        var evt = new TextOutputEvent
        {
            Source = InteractionSource.Agent,
            Text = "error!",
            Style = OutputStyle.Error,
            Append = false,
        };

        evt.Style.Should().Be(OutputStyle.Error);
        evt.Append.Should().BeFalse();
    }

    // ========================================================================
    // VoiceInputEvent
    // ========================================================================

    [Fact]
    public void VoiceInputEvent_Construction_SetsRequiredProperties()
    {
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "hello agent",
        };

        evt.TranscribedText.Should().Be("hello agent");
        evt.Source.Should().Be(InteractionSource.User);
    }

    [Fact]
    public void VoiceInputEvent_Confidence_DefaultsToOne()
    {
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "hello",
        };
        evt.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void VoiceInputEvent_Duration_DefaultsToZero()
    {
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "hello",
        };
        evt.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void VoiceInputEvent_DetectedLanguage_DefaultsToNull()
    {
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "hello",
        };
        evt.DetectedLanguage.Should().BeNull();
    }

    [Fact]
    public void VoiceInputEvent_IsInterim_DefaultsToFalse()
    {
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "hello",
        };
        evt.IsInterim.Should().BeFalse();
    }

    [Fact]
    public void VoiceInputEvent_AllProperties_CanBeSet()
    {
        var duration = TimeSpan.FromSeconds(2.5);
        var evt = new VoiceInputEvent
        {
            Source = InteractionSource.User,
            TranscribedText = "bonjour",
            Confidence = 0.85,
            Duration = duration,
            DetectedLanguage = "fr-FR",
            IsInterim = true,
        };

        evt.Confidence.Should().Be(0.85);
        evt.Duration.Should().Be(duration);
        evt.DetectedLanguage.Should().Be("fr-FR");
        evt.IsInterim.Should().BeTrue();
    }

    // ========================================================================
    // VoiceOutputEvent
    // ========================================================================

    [Fact]
    public void VoiceOutputEvent_Construction_SetsRequiredProperties()
    {
        var audio = new byte[] { 1, 2, 3 };
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = audio,
            Format = "pcm16",
        };

        evt.AudioChunk.Should().BeEquivalentTo(audio);
        evt.Format.Should().Be("pcm16");
    }

    [Fact]
    public void VoiceOutputEvent_SampleRate_DefaultsTo24000()
    {
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = new byte[] { 0 },
            Format = "wav",
        };
        evt.SampleRate.Should().Be(24000);
    }

    [Fact]
    public void VoiceOutputEvent_DurationSeconds_DefaultsToZero()
    {
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = new byte[] { 0 },
            Format = "wav",
        };
        evt.DurationSeconds.Should().Be(0.0);
    }

    [Fact]
    public void VoiceOutputEvent_IsComplete_DefaultsToFalse()
    {
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = new byte[] { 0 },
            Format = "wav",
        };
        evt.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void VoiceOutputEvent_OptionalProperties_DefaultToNull()
    {
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = new byte[] { 0 },
            Format = "wav",
        };
        evt.Emotion.Should().BeNull();
        evt.Text.Should().BeNull();
    }

    [Fact]
    public void VoiceOutputEvent_AllProperties_CanBeSet()
    {
        var audio = new byte[] { 10, 20, 30 };
        var evt = new VoiceOutputEvent
        {
            Source = InteractionSource.Agent,
            AudioChunk = audio,
            Format = "mp3",
            SampleRate = 48000,
            DurationSeconds = 1.5,
            IsComplete = true,
            Emotion = "cheerful",
            Text = "Hello there!",
        };

        evt.Format.Should().Be("mp3");
        evt.SampleRate.Should().Be(48000);
        evt.DurationSeconds.Should().Be(1.5);
        evt.IsComplete.Should().BeTrue();
        evt.Emotion.Should().Be("cheerful");
        evt.Text.Should().Be("Hello there!");
    }

    // ========================================================================
    // AudioChunkEvent
    // ========================================================================

    [Fact]
    public void AudioChunkEvent_Construction_SetsRequiredProperties()
    {
        var data = new byte[] { 0xFF, 0x00 };
        var evt = new AudioChunkEvent
        {
            Source = InteractionSource.User,
            AudioData = data,
            Format = "pcm16",
        };

        evt.AudioData.Should().BeEquivalentTo(data);
        evt.Format.Should().Be("pcm16");
    }

    [Fact]
    public void AudioChunkEvent_SampleRate_DefaultsTo16000()
    {
        var evt = new AudioChunkEvent
        {
            Source = InteractionSource.User,
            AudioData = new byte[] { 0 },
            Format = "pcm16",
        };
        evt.SampleRate.Should().Be(16000);
    }

    [Fact]
    public void AudioChunkEvent_Channels_DefaultsToOne()
    {
        var evt = new AudioChunkEvent
        {
            Source = InteractionSource.User,
            AudioData = new byte[] { 0 },
            Format = "pcm16",
        };
        evt.Channels.Should().Be(1);
    }

    [Fact]
    public void AudioChunkEvent_IsFinal_DefaultsToFalse()
    {
        var evt = new AudioChunkEvent
        {
            Source = InteractionSource.User,
            AudioData = new byte[] { 0 },
            Format = "pcm16",
        };
        evt.IsFinal.Should().BeFalse();
    }

    [Fact]
    public void AudioChunkEvent_AllProperties_CanBeSet()
    {
        var evt = new AudioChunkEvent
        {
            Source = InteractionSource.System,
            AudioData = new byte[] { 1, 2, 3, 4 },
            Format = "wav",
            SampleRate = 44100,
            Channels = 2,
            IsFinal = true,
        };

        evt.SampleRate.Should().Be(44100);
        evt.Channels.Should().Be(2);
        evt.IsFinal.Should().BeTrue();
    }

    // ========================================================================
    // AgentResponseEvent
    // ========================================================================

    [Fact]
    public void AgentResponseEvent_Construction_SetsRequiredProperties()
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "Hello!",
        };

        evt.TextChunk.Should().Be("Hello!");
        evt.Source.Should().Be(InteractionSource.Agent);
    }

    [Fact]
    public void AgentResponseEvent_IsComplete_DefaultsToFalse()
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "chunk",
        };
        evt.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void AgentResponseEvent_Type_DefaultsToDirect()
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "chunk",
        };
        evt.Type.Should().Be(ResponseType.Direct);
    }

    [Fact]
    public void AgentResponseEvent_IsSentenceEnd_DefaultsToFalse()
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "chunk",
        };
        evt.IsSentenceEnd.Should().BeFalse();
    }

    [Fact]
    public void AgentResponseEvent_AllProperties_CanBeSet()
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "Let me explain.",
            IsComplete = true,
            Type = ResponseType.Narration,
            IsSentenceEnd = true,
        };

        evt.IsComplete.Should().BeTrue();
        evt.Type.Should().Be(ResponseType.Narration);
        evt.IsSentenceEnd.Should().BeTrue();
    }

    [Theory]
    [InlineData(ResponseType.Direct)]
    [InlineData(ResponseType.Narration)]
    [InlineData(ResponseType.Action)]
    [InlineData(ResponseType.Clarification)]
    [InlineData(ResponseType.InnerThought)]
    public void AgentResponseEvent_AcceptsAllResponseTypes(ResponseType type)
    {
        var evt = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "text",
            Type = type,
        };
        evt.Type.Should().Be(type);
    }

    // ========================================================================
    // AgentThinkingEvent
    // ========================================================================

    [Fact]
    public void AgentThinkingEvent_Construction_SetsRequiredProperties()
    {
        var evt = new AgentThinkingEvent
        {
            Source = InteractionSource.Agent,
            ThoughtChunk = "Analyzing input...",
        };

        evt.ThoughtChunk.Should().Be("Analyzing input...");
        evt.Source.Should().Be(InteractionSource.Agent);
    }

    [Fact]
    public void AgentThinkingEvent_Phase_DefaultsToReasoning()
    {
        var evt = new AgentThinkingEvent
        {
            Source = InteractionSource.Agent,
            ThoughtChunk = "thinking",
        };
        evt.Phase.Should().Be(ThinkingPhase.Reasoning);
    }

    [Fact]
    public void AgentThinkingEvent_IsComplete_DefaultsToFalse()
    {
        var evt = new AgentThinkingEvent
        {
            Source = InteractionSource.Agent,
            ThoughtChunk = "thinking",
        };
        evt.IsComplete.Should().BeFalse();
    }

    [Theory]
    [InlineData(ThinkingPhase.Analyzing)]
    [InlineData(ThinkingPhase.Reasoning)]
    [InlineData(ThinkingPhase.Planning)]
    [InlineData(ThinkingPhase.Reflecting)]
    public void AgentThinkingEvent_AcceptsAllThinkingPhases(ThinkingPhase phase)
    {
        var evt = new AgentThinkingEvent
        {
            Source = InteractionSource.Agent,
            ThoughtChunk = "thought",
            Phase = phase,
        };
        evt.Phase.Should().Be(phase);
    }

    [Fact]
    public void AgentThinkingEvent_AllProperties_CanBeSet()
    {
        var evt = new AgentThinkingEvent
        {
            Source = InteractionSource.Agent,
            ThoughtChunk = "Done planning.",
            Phase = ThinkingPhase.Planning,
            IsComplete = true,
        };

        evt.Phase.Should().Be(ThinkingPhase.Planning);
        evt.IsComplete.Should().BeTrue();
    }

    // ========================================================================
    // ControlEvent
    // ========================================================================

    [Fact]
    public void ControlEvent_Construction_SetsRequiredProperties()
    {
        var evt = new ControlEvent
        {
            Source = InteractionSource.System,
            Action = ControlAction.StartListening,
        };

        evt.Action.Should().Be(ControlAction.StartListening);
        evt.Source.Should().Be(InteractionSource.System);
    }

    [Fact]
    public void ControlEvent_Reason_DefaultsToNull()
    {
        var evt = new ControlEvent
        {
            Source = InteractionSource.System,
            Action = ControlAction.Reset,
        };
        evt.Reason.Should().BeNull();
    }

    [Fact]
    public void ControlEvent_Reason_CanBeSet()
    {
        var evt = new ControlEvent
        {
            Source = InteractionSource.User,
            Action = ControlAction.InterruptSpeech,
            Reason = "User pressed stop",
        };
        evt.Reason.Should().Be("User pressed stop");
    }

    [Theory]
    [InlineData(ControlAction.StartListening)]
    [InlineData(ControlAction.StopListening)]
    [InlineData(ControlAction.InterruptSpeech)]
    [InlineData(ControlAction.CancelGeneration)]
    [InlineData(ControlAction.Reset)]
    [InlineData(ControlAction.Pause)]
    [InlineData(ControlAction.Resume)]
    public void ControlEvent_AcceptsAllControlActions(ControlAction action)
    {
        var evt = new ControlEvent
        {
            Source = InteractionSource.System,
            Action = action,
        };
        evt.Action.Should().Be(action);
    }

    // ========================================================================
    // ErrorEvent
    // ========================================================================

    [Fact]
    public void ErrorEvent_Construction_SetsRequiredProperties()
    {
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "Something went wrong",
        };

        evt.Message.Should().Be("Something went wrong");
        evt.Source.Should().Be(InteractionSource.System);
    }

    [Fact]
    public void ErrorEvent_Exception_DefaultsToNull()
    {
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "error",
        };
        evt.Exception.Should().BeNull();
    }

    [Fact]
    public void ErrorEvent_Category_DefaultsToUnknown()
    {
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "error",
        };
        evt.Category.Should().Be(ErrorCategory.Unknown);
    }

    [Fact]
    public void ErrorEvent_IsRecoverable_DefaultsToTrue()
    {
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "error",
        };
        evt.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public void ErrorEvent_AllProperties_CanBeSet()
    {
        var exception = new InvalidOperationException("test");
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "Audio device lost",
            Exception = exception,
            Category = ErrorCategory.AudioHardware,
            IsRecoverable = false,
        };

        evt.Exception.Should().BeSameAs(exception);
        evt.Category.Should().Be(ErrorCategory.AudioHardware);
        evt.IsRecoverable.Should().BeFalse();
    }

    [Theory]
    [InlineData(ErrorCategory.Unknown)]
    [InlineData(ErrorCategory.SpeechRecognition)]
    [InlineData(ErrorCategory.SpeechSynthesis)]
    [InlineData(ErrorCategory.Generation)]
    [InlineData(ErrorCategory.AudioHardware)]
    [InlineData(ErrorCategory.Network)]
    public void ErrorEvent_AcceptsAllErrorCategories(ErrorCategory category)
    {
        var evt = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "error",
            Category = category,
        };
        evt.Category.Should().Be(category);
    }

    // ========================================================================
    // HeartbeatEvent
    // ========================================================================

    [Fact]
    public void HeartbeatEvent_Construction_SetsSource()
    {
        var evt = new HeartbeatEvent { Source = InteractionSource.System };
        evt.Source.Should().Be(InteractionSource.System);
    }

    [Fact]
    public void HeartbeatEvent_CurrentState_DefaultsToIdle()
    {
        var evt = new HeartbeatEvent { Source = InteractionSource.System };
        evt.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    [Fact]
    public void HeartbeatEvent_TimeSinceLastInteraction_DefaultsToZero()
    {
        var evt = new HeartbeatEvent { Source = InteractionSource.System };
        evt.TimeSinceLastInteraction.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void HeartbeatEvent_AllProperties_CanBeSet()
    {
        var elapsed = TimeSpan.FromMinutes(5);
        var evt = new HeartbeatEvent
        {
            Source = InteractionSource.System,
            CurrentState = AgentPresenceState.Listening,
            TimeSinceLastInteraction = elapsed,
        };

        evt.CurrentState.Should().Be(AgentPresenceState.Listening);
        evt.TimeSinceLastInteraction.Should().Be(elapsed);
    }

    // ========================================================================
    // PresenceStateEvent
    // ========================================================================

    [Fact]
    public void PresenceStateEvent_Construction_SetsRequiredProperties()
    {
        var evt = new PresenceStateEvent
        {
            Source = InteractionSource.System,
            State = AgentPresenceState.Processing,
        };

        evt.State.Should().Be(AgentPresenceState.Processing);
        evt.Source.Should().Be(InteractionSource.System);
    }

    [Fact]
    public void PresenceStateEvent_PreviousState_DefaultsToNull()
    {
        var evt = new PresenceStateEvent
        {
            Source = InteractionSource.System,
            State = AgentPresenceState.Idle,
        };
        evt.PreviousState.Should().BeNull();
    }

    [Fact]
    public void PresenceStateEvent_Reason_DefaultsToNull()
    {
        var evt = new PresenceStateEvent
        {
            Source = InteractionSource.System,
            State = AgentPresenceState.Idle,
        };
        evt.Reason.Should().BeNull();
    }

    [Fact]
    public void PresenceStateEvent_AllProperties_CanBeSet()
    {
        var evt = new PresenceStateEvent
        {
            Source = InteractionSource.System,
            State = AgentPresenceState.Speaking,
            PreviousState = AgentPresenceState.Processing,
            Reason = "Response ready",
        };

        evt.State.Should().Be(AgentPresenceState.Speaking);
        evt.PreviousState.Should().Be(AgentPresenceState.Processing);
        evt.Reason.Should().Be("Response ready");
    }

    [Theory]
    [InlineData(AgentPresenceState.Idle)]
    [InlineData(AgentPresenceState.Listening)]
    [InlineData(AgentPresenceState.Processing)]
    [InlineData(AgentPresenceState.Speaking)]
    [InlineData(AgentPresenceState.Interrupted)]
    [InlineData(AgentPresenceState.Paused)]
    public void PresenceStateEvent_AcceptsAllPresenceStates(AgentPresenceState state)
    {
        var evt = new PresenceStateEvent
        {
            Source = InteractionSource.System,
            State = state,
        };
        evt.State.Should().Be(state);
    }

    // ========================================================================
    // BargeInEventArgs
    // ========================================================================

    [Fact]
    public void BargeInEventArgs_Construction_SetsAllProperties()
    {
        var args = new BargeInEventArgs(
            AgentPresenceState.Speaking,
            "stop",
            BargeInType.SpeechInterrupt);

        args.InterruptedState.Should().Be(AgentPresenceState.Speaking);
        args.UserInput.Should().Be("stop");
        args.Type.Should().Be(BargeInType.SpeechInterrupt);
    }

    [Fact]
    public void BargeInEventArgs_Timestamp_IsRecentUtc()
    {
        var before = DateTimeOffset.UtcNow;
        var args = new BargeInEventArgs(
            AgentPresenceState.Processing,
            null,
            BargeInType.ProcessingCancel);
        var after = DateTimeOffset.UtcNow;

        args.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void BargeInEventArgs_UserInput_CanBeNull()
    {
        var args = new BargeInEventArgs(
            AgentPresenceState.Processing,
            null,
            BargeInType.ProcessingCancel);

        args.UserInput.Should().BeNull();
    }

    [Fact]
    public void BargeInEventArgs_InheritsFromEventArgs()
    {
        var args = new BargeInEventArgs(
            AgentPresenceState.Speaking,
            "input",
            BargeInType.SpeechInterrupt);

        args.Should().BeAssignableTo<EventArgs>();
    }

    [Theory]
    [InlineData(BargeInType.SpeechInterrupt)]
    [InlineData(BargeInType.ProcessingCancel)]
    public void BargeInEventArgs_AcceptsAllBargeInTypes(BargeInType type)
    {
        var args = new BargeInEventArgs(AgentPresenceState.Speaking, "input", type);
        args.Type.Should().Be(type);
    }

    // ========================================================================
    // StateChangeEventArgs
    // ========================================================================

    [Fact]
    public void StateChangeEventArgs_Construction_SetsAllProperties()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle,
            AgentPresenceState.Listening,
            "User started speaking");

        args.PreviousState.Should().Be(AgentPresenceState.Idle);
        args.NewState.Should().Be(AgentPresenceState.Listening);
        args.Reason.Should().Be("User started speaking");
    }

    [Fact]
    public void StateChangeEventArgs_Timestamp_IsRecentUtc()
    {
        var before = DateTimeOffset.UtcNow;
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle,
            AgentPresenceState.Processing,
            null);
        var after = DateTimeOffset.UtcNow;

        args.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void StateChangeEventArgs_Reason_CanBeNull()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Processing,
            AgentPresenceState.Speaking,
            null);

        args.Reason.Should().BeNull();
    }

    [Fact]
    public void StateChangeEventArgs_InheritsFromEventArgs()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle,
            AgentPresenceState.Listening,
            "reason");

        args.Should().BeAssignableTo<EventArgs>();
    }

    [Fact]
    public void StateChangeEventArgs_SameState_IsAllowed()
    {
        var args = new StateChangeEventArgs(
            AgentPresenceState.Idle,
            AgentPresenceState.Idle,
            "no-op");

        args.PreviousState.Should().Be(args.NewState);
    }

    // ========================================================================
    // Record equality and with-expression tests
    // ========================================================================

    [Fact]
    public void AgentResponseEvent_RecordEquality_WorksCorrectly()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var evt1 = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "hello",
            Id = id,
            Timestamp = timestamp,
        };
        var evt2 = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "hello",
            Id = id,
            Timestamp = timestamp,
        };

        evt1.Should().Be(evt2);
    }

    [Fact]
    public void AgentResponseEvent_RecordInequality_DifferentTextChunk()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var evt1 = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "hello",
            Id = id,
            Timestamp = timestamp,
        };
        var evt2 = new AgentResponseEvent
        {
            Source = InteractionSource.Agent,
            TextChunk = "world",
            Id = id,
            Timestamp = timestamp,
        };

        evt1.Should().NotBe(evt2);
    }

    [Fact]
    public void ControlEvent_WithExpression_CreatesModifiedCopy()
    {
        var original = new ControlEvent
        {
            Source = InteractionSource.System,
            Action = ControlAction.Pause,
        };
        var modified = original with { Action = ControlAction.Resume, Reason = "User resumed" };

        modified.Action.Should().Be(ControlAction.Resume);
        modified.Reason.Should().Be("User resumed");
        original.Action.Should().Be(ControlAction.Pause);
        original.Reason.Should().BeNull();
    }

    [Fact]
    public void ErrorEvent_WithExpression_PreservesUnchangedProperties()
    {
        var exception = new InvalidOperationException("test");
        var original = new ErrorEvent
        {
            Source = InteractionSource.System,
            Message = "original error",
            Exception = exception,
            Category = ErrorCategory.Network,
            IsRecoverable = false,
        };
        var modified = original with { Message = "updated error" };

        modified.Message.Should().Be("updated error");
        modified.Exception.Should().BeSameAs(exception);
        modified.Category.Should().Be(ErrorCategory.Network);
        modified.IsRecoverable.Should().BeFalse();
        modified.Id.Should().Be(original.Id);
    }

    [Fact]
    public void HeartbeatEvent_WithExpression_CreatesModifiedCopy()
    {
        var original = new HeartbeatEvent
        {
            Source = InteractionSource.System,
            CurrentState = AgentPresenceState.Idle,
            TimeSinceLastInteraction = TimeSpan.FromSeconds(10),
        };
        var modified = original with
        {
            CurrentState = AgentPresenceState.Processing,
            TimeSinceLastInteraction = TimeSpan.FromSeconds(20),
        };

        modified.CurrentState.Should().Be(AgentPresenceState.Processing);
        modified.TimeSinceLastInteraction.Should().Be(TimeSpan.FromSeconds(20));
        original.CurrentState.Should().Be(AgentPresenceState.Idle);
    }

    // ========================================================================
    // Polymorphism tests - all event types derive from InteractionEvent
    // ========================================================================

    [Fact]
    public void AllEventTypes_DeriveFromInteractionEvent()
    {
        InteractionEvent textInput = new TextInputEvent { Source = InteractionSource.User, Text = "hi" };
        InteractionEvent textOutput = new TextOutputEvent { Source = InteractionSource.Agent, Text = "hello" };
        InteractionEvent voiceInput = new VoiceInputEvent { Source = InteractionSource.User, TranscribedText = "hey" };
        InteractionEvent voiceOutput = new VoiceOutputEvent { Source = InteractionSource.Agent, AudioChunk = new byte[] { 0 }, Format = "pcm16" };
        InteractionEvent audioChunk = new AudioChunkEvent { Source = InteractionSource.User, AudioData = new byte[] { 0 }, Format = "pcm16" };
        InteractionEvent response = new AgentResponseEvent { Source = InteractionSource.Agent, TextChunk = "response" };
        InteractionEvent thinking = new AgentThinkingEvent { Source = InteractionSource.Agent, ThoughtChunk = "hmm" };
        InteractionEvent control = new ControlEvent { Source = InteractionSource.System, Action = ControlAction.Reset };
        InteractionEvent error = new ErrorEvent { Source = InteractionSource.System, Message = "err" };
        InteractionEvent heartbeat = new HeartbeatEvent { Source = InteractionSource.System };
        InteractionEvent presence = new PresenceStateEvent { Source = InteractionSource.System, State = AgentPresenceState.Idle };

        var events = new[] { textInput, textOutput, voiceInput, voiceOutput, audioChunk, response, thinking, control, error, heartbeat, presence };

        events.Should().AllSatisfy(e =>
        {
            e.Id.Should().NotBeEmpty();
            e.Timestamp.Should().BeAfter(DateTimeOffset.MinValue);
        });
    }

    // ========================================================================
    // InteractionSource on each event type
    // ========================================================================

    [Theory]
    [InlineData(InteractionSource.User)]
    [InlineData(InteractionSource.Agent)]
    [InlineData(InteractionSource.System)]
    public void TextInputEvent_AcceptsAllSources(InteractionSource source)
    {
        var evt = new TextInputEvent { Source = source, Text = "text" };
        evt.Source.Should().Be(source);
    }

    [Theory]
    [InlineData(InteractionSource.User)]
    [InlineData(InteractionSource.Agent)]
    [InlineData(InteractionSource.System)]
    public void ErrorEvent_AcceptsAllSources(InteractionSource source)
    {
        var evt = new ErrorEvent { Source = source, Message = "err" };
        evt.Source.Should().Be(source);
    }

    // ========================================================================
    // Enum value coverage (AgentPresenceState, ThinkingPhase)
    // ========================================================================

    [Fact]
    public void AgentPresenceState_HasExpectedValues()
    {
        var values = Enum.GetValues<AgentPresenceState>();
        values.Should().Contain(AgentPresenceState.Idle);
        values.Should().Contain(AgentPresenceState.Listening);
        values.Should().Contain(AgentPresenceState.Processing);
        values.Should().Contain(AgentPresenceState.Speaking);
        values.Should().Contain(AgentPresenceState.Interrupted);
        values.Should().Contain(AgentPresenceState.Paused);
        values.Should().HaveCount(6);
    }

    [Fact]
    public void ThinkingPhase_HasExpectedValues()
    {
        var values = Enum.GetValues<ThinkingPhase>();
        values.Should().Contain(ThinkingPhase.Analyzing);
        values.Should().Contain(ThinkingPhase.Reasoning);
        values.Should().Contain(ThinkingPhase.Planning);
        values.Should().Contain(ThinkingPhase.Reflecting);
        values.Should().HaveCount(4);
    }

    [Fact]
    public void ResponseType_HasExpectedValues()
    {
        var values = Enum.GetValues<ResponseType>();
        values.Should().Contain(ResponseType.Direct);
        values.Should().Contain(ResponseType.Narration);
        values.Should().Contain(ResponseType.Action);
        values.Should().Contain(ResponseType.Clarification);
        values.Should().Contain(ResponseType.InnerThought);
        values.Should().HaveCount(5);
    }

    [Fact]
    public void OutputStyle_HasExpectedValues()
    {
        var values = Enum.GetValues<OutputStyle>();
        values.Should().Contain(OutputStyle.Normal);
        values.Should().Contain(OutputStyle.Thinking);
        values.Should().Contain(OutputStyle.Emphasis);
        values.Should().Contain(OutputStyle.Whisper);
        values.Should().Contain(OutputStyle.System);
        values.Should().Contain(OutputStyle.Error);
        values.Should().Contain(OutputStyle.UserInput);
        values.Should().HaveCount(7);
    }

    [Fact]
    public void ControlAction_HasExpectedValues()
    {
        var values = Enum.GetValues<ControlAction>();
        values.Should().Contain(ControlAction.StartListening);
        values.Should().Contain(ControlAction.StopListening);
        values.Should().Contain(ControlAction.InterruptSpeech);
        values.Should().Contain(ControlAction.CancelGeneration);
        values.Should().Contain(ControlAction.Reset);
        values.Should().Contain(ControlAction.Pause);
        values.Should().Contain(ControlAction.Resume);
        values.Should().HaveCount(7);
    }

    [Fact]
    public void ErrorCategory_HasExpectedValues()
    {
        var values = Enum.GetValues<ErrorCategory>();
        values.Should().Contain(ErrorCategory.Unknown);
        values.Should().Contain(ErrorCategory.SpeechRecognition);
        values.Should().Contain(ErrorCategory.SpeechSynthesis);
        values.Should().Contain(ErrorCategory.Generation);
        values.Should().Contain(ErrorCategory.AudioHardware);
        values.Should().Contain(ErrorCategory.Network);
        values.Should().HaveCount(6);
    }

    [Fact]
    public void BargeInType_HasExpectedValues()
    {
        var values = Enum.GetValues<BargeInType>();
        values.Should().Contain(BargeInType.SpeechInterrupt);
        values.Should().Contain(BargeInType.ProcessingCancel);
        values.Should().HaveCount(2);
    }

    [Fact]
    public void InteractionSource_HasExpectedValues()
    {
        var values = Enum.GetValues<InteractionSource>();
        values.Should().Contain(InteractionSource.User);
        values.Should().Contain(InteractionSource.Agent);
        values.Should().Contain(InteractionSource.System);
        values.Should().HaveCount(3);
    }

    // ========================================================================
    // Sealed record type verification
    // ========================================================================

    [Fact]
    public void InteractionEvent_IsAbstract()
    {
        typeof(InteractionEvent).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void TextInputEvent_IsSealed()
    {
        typeof(TextInputEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void TextOutputEvent_IsSealed()
    {
        typeof(TextOutputEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void VoiceInputEvent_IsSealed()
    {
        typeof(VoiceInputEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void VoiceOutputEvent_IsSealed()
    {
        typeof(VoiceOutputEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void AudioChunkEvent_IsSealed()
    {
        typeof(AudioChunkEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void AgentResponseEvent_IsSealed()
    {
        typeof(AgentResponseEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void AgentThinkingEvent_IsSealed()
    {
        typeof(AgentThinkingEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ControlEvent_IsSealed()
    {
        typeof(ControlEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void ErrorEvent_IsSealed()
    {
        typeof(ErrorEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void HeartbeatEvent_IsSealed()
    {
        typeof(HeartbeatEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void PresenceStateEvent_IsSealed()
    {
        typeof(PresenceStateEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void BargeInEventArgs_IsSealed()
    {
        typeof(BargeInEventArgs).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void StateChangeEventArgs_IsSealed()
    {
        typeof(StateChangeEventArgs).IsSealed.Should().BeTrue();
    }
}
