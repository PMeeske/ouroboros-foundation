using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActuatorDescriptorTests
{
    [Fact]
    public void Voice_CreatesVoiceActuator()
    {
        var desc = ActuatorDescriptor.Voice("v1");
        desc.Id.Should().Be("v1");
        desc.Modality.Should().Be(ActuatorModality.Voice);
        desc.Name.Should().Be("Voice Output");
        desc.IsActive.Should().BeTrue();
        desc.Capabilities.Should().Contain(Capability.Speaking);
    }

    [Fact]
    public void Text_CreatesTextActuator()
    {
        var desc = ActuatorDescriptor.Text("t1", "Custom Text");
        desc.Id.Should().Be("t1");
        desc.Modality.Should().Be(ActuatorModality.Text);
        desc.Name.Should().Be("Custom Text");
        desc.Capabilities.Should().Contain(Capability.Writing);
    }

    [Fact]
    public void Properties_DefaultsToNull()
    {
        var desc = ActuatorDescriptor.Voice("v1");
        desc.Properties.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class ActuatorInfoTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var info = new ActuatorInfo("a1", "Speaker", ActuatorModality.Voice, true,
            EmbodimentCapabilities.AudioOutput, new List<string> { "speak", "beep" });

        info.ActuatorId.Should().Be("a1");
        info.Name.Should().Be("Speaker");
        info.Modality.Should().Be(ActuatorModality.Voice);
        info.IsActive.Should().BeTrue();
        info.Capabilities.Should().Be(EmbodimentCapabilities.AudioOutput);
        info.SupportedActions.Should().HaveCount(2);
        info.Properties.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class ActuatorModalityEnumTests
{
    [Theory]
    [InlineData(ActuatorModality.Voice)]
    [InlineData(ActuatorModality.Text)]
    [InlineData(ActuatorModality.Visual)]
    [InlineData(ActuatorModality.Motor)]
    public void AllValues_AreDefined(ActuatorModality modality)
    {
        Enum.IsDefined(typeof(ActuatorModality), modality).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class AffordanceConstraintsTests
{
    [Fact]
    public void None_HasAllNullValues()
    {
        var none = AffordanceConstraints.None;
        none.MinApproachDistance.Should().BeNull();
        none.MaxApproachDistance.Should().BeNull();
        none.RequiredOrientation.Should().BeNull();
        none.ForceRange.Should().BeNull();
        none.TimeConstraint.Should().BeNull();
        none.CustomConstraints.Should().BeNull();
    }

    [Fact]
    public void Construction_SetsAllProperties()
    {
        var constraints = new AffordanceConstraints(0.5, 2.0, (1.0, 0.0, 0.0), (5.0, 20.0), TimeSpan.FromSeconds(10), null);
        constraints.MinApproachDistance.Should().Be(0.5);
        constraints.MaxApproachDistance.Should().Be(2.0);
        constraints.ForceRange.Should().Be((5.0, 20.0));
        constraints.TimeConstraint.Should().Be(TimeSpan.FromSeconds(10));
    }
}

[Trait("Category", "Unit")]
public class AffordanceTypeEnumTests
{
    [Theory]
    [InlineData(AffordanceType.Traversable)]
    [InlineData(AffordanceType.Graspable)]
    [InlineData(AffordanceType.Activatable)]
    [InlineData(AffordanceType.Hazardous)]
    [InlineData(AffordanceType.Custom)]
    public void AllValues_AreDefined(AffordanceType type)
    {
        Enum.IsDefined(typeof(AffordanceType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class AggregateStatusEnumTests
{
    [Theory]
    [InlineData(AggregateStatus.Inactive)]
    [InlineData(AggregateStatus.Activating)]
    [InlineData(AggregateStatus.Active)]
    [InlineData(AggregateStatus.Deactivating)]
    [InlineData(AggregateStatus.Failed)]
    public void AllValues_AreDefined(AggregateStatus status)
    {
        Enum.IsDefined(typeof(AggregateStatus), status).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class AudioChunkTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var data = new byte[] { 1, 2, 3 };
        var now = DateTime.UtcNow;
        var chunk = new AudioChunk(data, 16000, 1, now, false);

        chunk.Data.Should().BeEquivalentTo(data);
        chunk.SampleRate.Should().Be(16000);
        chunk.Channels.Should().Be(1);
        chunk.Timestamp.Should().Be(now);
        chunk.IsFinal.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
public class AudioPerceptionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var perception = new AudioPerception(id, now, 0.9, "Hello world", "en", null, TimeSpan.FromSeconds(2), true);

        perception.Id.Should().Be(id);
        perception.Modality.Should().Be(SensorModality.Audio);
        perception.Confidence.Should().Be(0.9);
        perception.TranscribedText.Should().Be("Hello world");
        perception.IsFinal.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class AudioSensorConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new AudioSensorConfig();
        config.SampleRate.Should().Be(16000);
        config.Channels.Should().Be(1);
        config.Language.Should().BeNull();
        config.EnableVAD.Should().BeTrue();
        config.SilenceThresholdMs.Should().Be(1500);
        config.MaxRecordingDurationMs.Should().Be(30000);
        config.EnableInterimResults.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class CapabilityEnumTests
{
    [Theory]
    [InlineData(Capability.Hearing)]
    [InlineData(Capability.Speaking)]
    [InlineData(Capability.Reasoning)]
    [InlineData(Capability.ToolUse)]
    [InlineData(Capability.EmotionExpression)]
    public void AllValues_AreDefined(Capability cap)
    {
        Enum.IsDefined(typeof(Capability), cap).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class DetectedFaceTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var face = new DetectedFace("f1", 0.95, (0.1, 0.2, 0.3, 0.4), "happy", 30, true, "person-1");

        face.FaceId.Should().Be("f1");
        face.Confidence.Should().Be(0.95);
        face.BoundingBox.Should().Be((0.1, 0.2, 0.3, 0.4));
        face.Emotion.Should().Be("happy");
        face.Age.Should().Be(30);
        face.IsKnown.Should().BeTrue();
        face.PersonId.Should().Be("person-1");
    }
}

[Trait("Category", "Unit")]
public class DetectedObjectTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var obj = new DetectedObject("car", 0.92, (0.5, 0.5, 0.2, 0.1), null);
        obj.Label.Should().Be("car");
        obj.Confidence.Should().Be(0.92);
        obj.Attributes.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class SpeechRequestTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var request = new SpeechRequest("Hello");
        request.Text.Should().Be("Hello");
        request.Priority.Should().Be(0);
        request.Emotion.Should().BeNull();
        request.Interruptible.Should().BeTrue();
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var request = new SpeechRequest("Warning", Priority: 10, Emotion: "urgent", Interruptible: false);
        request.Priority.Should().Be(10);
        request.Emotion.Should().Be("urgent");
        request.Interruptible.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
public class SynthesizedSpeechTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var data = new byte[] { 0xFF, 0xFE };
        var now = DateTime.UtcNow;
        var speech = new SynthesizedSpeech("Hello", data, "wav", 16000, TimeSpan.FromSeconds(1), now);

        speech.Text.Should().Be("Hello");
        speech.AudioData.Should().BeEquivalentTo(data);
        speech.Format.Should().Be("wav");
        speech.SampleRate.Should().Be(16000);
        speech.Duration.Should().Be(TimeSpan.FromSeconds(1));
    }
}

[Trait("Category", "Unit")]
public class TextPerceptionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var perception = new TextPerception(id, now, 1.0, "user input", "keyboard");

        perception.Modality.Should().Be(SensorModality.Text);
        perception.Text.Should().Be("user input");
        perception.Source.Should().Be("keyboard");
    }
}

[Trait("Category", "Unit")]
public class TranscriptionResultTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var words = new List<WordTiming> { new("hello", TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0.95) };
        var result = new TranscriptionResult("hello world", 0.9, "en", true, TimeSpan.Zero, TimeSpan.FromSeconds(2), words);

        result.Text.Should().Be("hello world");
        result.Confidence.Should().Be(0.9);
        result.Language.Should().Be("en");
        result.IsFinal.Should().BeTrue();
        result.Words.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
public class VideoFrameTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var data = new byte[] { 1, 2, 3 };
        var now = DateTime.UtcNow;
        var frame = new VideoFrame(data, 640, 480, "jpeg", 42, now);

        frame.Width.Should().Be(640);
        frame.Height.Should().Be(480);
        frame.Format.Should().Be("jpeg");
        frame.FrameNumber.Should().Be(42);
    }
}

[Trait("Category", "Unit")]
public class WordTimingTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var timing = new WordTiming("hello", TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500), 0.98);

        timing.Word.Should().Be("hello");
        timing.StartTime.Should().Be(TimeSpan.FromMilliseconds(100));
        timing.EndTime.Should().Be(TimeSpan.FromMilliseconds(500));
        timing.Confidence.Should().Be(0.98);
    }
}

[Trait("Category", "Unit")]
public class VoiceConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new VoiceConfig();
        config.Voice.Should().Be("default");
        config.Speed.Should().Be(1.0);
        config.Pitch.Should().Be(1.0);
        config.Volume.Should().Be(1.0);
        config.Language.Should().Be("en-US");
        config.Style.Should().Be("neutral");
        config.EnableSSML.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
public class VoiceInfoTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var info = new VoiceInfo("v1", "English Female", "en-US", "Female", new List<string> { "neutral", "cheerful" });

        info.Id.Should().Be("v1");
        info.Name.Should().Be("English Female");
        info.Language.Should().Be("en-US");
        info.Gender.Should().Be("Female");
        info.SupportedStyles.Should().HaveCount(2);
    }
}

[Trait("Category", "Unit")]
public class VoiceActivityEnumTests
{
    [Theory]
    [InlineData(VoiceActivity.Silence)]
    [InlineData(VoiceActivity.SpeechStart)]
    [InlineData(VoiceActivity.Speaking)]
    [InlineData(VoiceActivity.SpeechEnd)]
    public void AllValues_AreDefined(VoiceActivity activity)
    {
        Enum.IsDefined(typeof(VoiceActivity), activity).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class SensorModalityEnumTests
{
    [Theory]
    [InlineData(SensorModality.Audio)]
    [InlineData(SensorModality.Visual)]
    [InlineData(SensorModality.Text)]
    [InlineData(SensorModality.Haptic)]
    [InlineData(SensorModality.Proprioceptive)]
    public void AllValues_AreDefined(SensorModality modality)
    {
        Enum.IsDefined(typeof(SensorModality), modality).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class SensorDescriptorTests
{
    [Fact]
    public void Audio_CreatesMicDescriptor()
    {
        var desc = SensorDescriptor.Audio("mic1");
        desc.Id.Should().Be("mic1");
        desc.Modality.Should().Be(SensorModality.Audio);
        desc.Name.Should().Be("Microphone");
        desc.IsActive.Should().BeTrue();
        desc.Capabilities.Should().Contain(Capability.Hearing);
    }

    [Fact]
    public void Visual_CreatesCameraDescriptor()
    {
        var desc = SensorDescriptor.Visual("cam1", "Front Camera");
        desc.Modality.Should().Be(SensorModality.Visual);
        desc.Name.Should().Be("Front Camera");
        desc.Capabilities.Should().Contain(Capability.Seeing);
    }

    [Fact]
    public void Text_CreatesTextDescriptor()
    {
        var desc = SensorDescriptor.Text("text1");
        desc.Modality.Should().Be(SensorModality.Text);
        desc.Capabilities.Should().Contain(Capability.Reading);
    }
}

[Trait("Category", "Unit")]
public class SensorInfoTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var info = new SensorInfo("s1", "Camera", SensorModality.Visual, true,
            EmbodimentCapabilities.VideoCapture | EmbodimentCapabilities.VisionAnalysis);

        info.SensorId.Should().Be("s1");
        info.Modality.Should().Be(SensorModality.Visual);
        info.IsActive.Should().BeTrue();
        info.Capabilities.Should().HaveFlag(EmbodimentCapabilities.VideoCapture);
        info.Properties.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class EmbodimentCapabilitiesTests
{
    [Fact]
    public void None_HasNoFlags()
    {
        var caps = EmbodimentCapabilities.None;
        caps.Should().Be(0);
    }

    [Fact]
    public void FlagsCombination_Works()
    {
        var caps = EmbodimentCapabilities.VideoCapture | EmbodimentCapabilities.AudioCapture;
        caps.HasFlag(EmbodimentCapabilities.VideoCapture).Should().BeTrue();
        caps.HasFlag(EmbodimentCapabilities.AudioCapture).Should().BeTrue();
        caps.HasFlag(EmbodimentCapabilities.VisionAnalysis).Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
public class EmbodimentStateEnumTests
{
    [Theory]
    [InlineData(EmbodimentState.Dormant)]
    [InlineData(EmbodimentState.Awake)]
    [InlineData(EmbodimentState.Listening)]
    [InlineData(EmbodimentState.Speaking)]
    [InlineData(EmbodimentState.FullyEngaged)]
    public void AllValues_AreDefined(EmbodimentState state)
    {
        Enum.IsDefined(typeof(EmbodimentState), state).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class EmbodimentDomainEventTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var evt = new EmbodimentDomainEvent(EmbodimentDomainEventType.SensorActivated, now);

        evt.EventType.Should().Be(EmbodimentDomainEventType.SensorActivated);
        evt.Timestamp.Should().Be(now);
        evt.Details.Should().BeNull();
    }

    [Fact]
    public void Construction_WithDetails()
    {
        var details = new Dictionary<string, object> { { "sensorId", "mic1" } };
        var evt = new EmbodimentDomainEvent(EmbodimentDomainEventType.PerceptionReceived, DateTime.UtcNow, details);
        evt.Details.Should().ContainKey("sensorId");
    }
}

[Trait("Category", "Unit")]
public class EmbodimentProviderEventTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var evt = new EmbodimentProviderEvent(EmbodimentProviderEventType.Connected, DateTime.UtcNow);
        evt.EventType.Should().Be(EmbodimentProviderEventType.Connected);
        evt.Details.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class FusedPerceptionTests
{
    [Fact]
    public void HasAudio_WhenAudioPresent_ReturnsTrue()
    {
        var audio = new AudioPerception(Guid.NewGuid(), DateTime.UtcNow, 0.9, "test", null, null, TimeSpan.FromSeconds(1), true);
        var fused = new FusedPerception(Guid.NewGuid(), DateTime.UtcNow,
            new List<AudioPerception> { audio },
            new List<VisualPerception>(),
            new List<TextPerception>(),
            "Audio detected", 0.9);

        fused.HasAudio.Should().BeTrue();
        fused.HasVisual.Should().BeFalse();
    }

    [Fact]
    public void CombinedTranscript_ConcatenatesFinalAudio()
    {
        var a1 = new AudioPerception(Guid.NewGuid(), DateTime.UtcNow, 0.9, "Hello", null, null, TimeSpan.FromSeconds(1), true);
        var a2 = new AudioPerception(Guid.NewGuid(), DateTime.UtcNow, 0.8, "World", null, null, TimeSpan.FromSeconds(1), true);
        var a3 = new AudioPerception(Guid.NewGuid(), DateTime.UtcNow, 0.5, "interim", null, null, TimeSpan.FromSeconds(1), false);

        var fused = new FusedPerception(Guid.NewGuid(), DateTime.UtcNow,
            new List<AudioPerception> { a1, a2, a3 },
            new List<VisualPerception>(),
            new List<TextPerception>(),
            "Combined", 0.85);

        fused.CombinedTranscript.Should().Be("Hello World");
    }

    [Fact]
    public void DominantModality_ReturnsMostCommon()
    {
        var text1 = new TextPerception(Guid.NewGuid(), DateTime.UtcNow, 1.0, "a", null);
        var text2 = new TextPerception(Guid.NewGuid(), DateTime.UtcNow, 1.0, "b", null);

        var fused = new FusedPerception(Guid.NewGuid(), DateTime.UtcNow,
            new List<AudioPerception>(),
            new List<VisualPerception>(),
            new List<TextPerception> { text1, text2 },
            "Text dominant", 0.9);

        fused.DominantModality.Should().Be(SensorModality.Text);
    }
}

[Trait("Category", "Unit")]
public class GripRequirementTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var grip = new GripRequirement(0.1, 0.5, (5.0, 20.0));
        grip.MinApproachDistance.Should().Be(0.1);
        grip.MaxApproachDistance.Should().Be(0.5);
        grip.ForceRange.Should().Be((5.0, 20.0));
    }
}

[Trait("Category", "Unit")]
public class LimitationTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var limitation = new Limitation(LimitationType.MemoryBounded, "Limited context window", 0.7);
        limitation.Type.Should().Be(LimitationType.MemoryBounded);
        limitation.Description.Should().Be("Limited context window");
        limitation.Severity.Should().Be(0.7);
    }

    [Fact]
    public void DefaultSeverity_IsHalf()
    {
        var limitation = new Limitation(LimitationType.KnowledgeGap, "Missing info");
        limitation.Severity.Should().Be(0.5);
    }
}

[Trait("Category", "Unit")]
public class LimitationTypeEnumTests
{
    [Theory]
    [InlineData(LimitationType.PerceptualBlind)]
    [InlineData(LimitationType.ActionRestricted)]
    [InlineData(LimitationType.MemoryBounded)]
    [InlineData(LimitationType.EthicalConstraint)]
    public void AllValues_AreDefined(LimitationType type)
    {
        Enum.IsDefined(typeof(LimitationType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class PerceptionDataTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var data = new PerceptionData("s1", SensorModality.Audio, DateTime.UtcNow, "raw audio data");
        data.SensorId.Should().Be("s1");
        data.Modality.Should().Be(SensorModality.Audio);
        data.Metadata.Should().BeNull();
    }

    [Fact]
    public void GetDataAs_CorrectType_ReturnsData()
    {
        var data = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, "hello");
        data.GetDataAs<string>().Should().Be("hello");
    }

    [Fact]
    public void GetDataAs_WrongType_ReturnsNull()
    {
        var data = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, "hello");
        data.GetDataAs<List<int>>().Should().BeNull();
    }

    [Fact]
    public void GetBytes_WithByteArray_ReturnsBytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var data = new PerceptionData("s1", SensorModality.Audio, DateTime.UtcNow, bytes);
        data.GetBytes().Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void GetBytes_WithNonBytes_ReturnsNull()
    {
        var data = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, "not bytes");
        data.GetBytes().Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class UnifiedPerceptionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var textPerception = new TextPerception(Guid.NewGuid(), DateTime.UtcNow, 1.0, "hello", "keyboard");
        var unified = new UnifiedPerception("text1", SensorModality.Text, textPerception, DateTime.UtcNow);

        unified.Source.Should().Be("text1");
        unified.Modality.Should().Be(SensorModality.Text);
        unified.Perception.Should().Be(textPerception);
    }
}

[Trait("Category", "Unit")]
public class VisionAnalysisOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new VisionAnalysisOptions();
        options.IncludeDescription.Should().BeTrue();
        options.DetectObjects.Should().BeTrue();
        options.DetectFaces.Should().BeTrue();
        options.ClassifyScene.Should().BeTrue();
        options.ExtractText.Should().BeFalse();
        options.AnalyzeColors.Should().BeFalse();
        options.MaxObjects.Should().Be(20);
        options.ConfidenceThreshold.Should().Be(0.5);
    }
}

[Trait("Category", "Unit")]
public class VisionAnalysisResultModelTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var result = new VisionAnalysisResult(
            "A room with furniture", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", new List<string> { "brown", "white" }, null, 0.85, 150);

        result.Description.Should().Be("A room with furniture");
        result.SceneType.Should().Be("indoor");
        result.DominantColors.Should().HaveCount(2);
        result.ProcessingTimeMs.Should().Be(150);
    }
}

[Trait("Category", "Unit")]
public class EmbodimentAggregateStateTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var state = new EmbodimentAggregateState("agg-1", "Main", AggregateStatus.Active,
            EmbodimentCapabilities.AudioCapture | EmbodimentCapabilities.VideoCapture, now);

        state.AggregateId.Should().Be("agg-1");
        state.Name.Should().Be("Main");
        state.Status.Should().Be(AggregateStatus.Active);
        state.Capabilities.HasFlag(EmbodimentCapabilities.AudioCapture).Should().BeTrue();
        state.LastUpdatedAt.Should().Be(now);
    }
}
