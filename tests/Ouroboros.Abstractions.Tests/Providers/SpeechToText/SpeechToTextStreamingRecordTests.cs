using Ouroboros.Providers.SpeechToText;

namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

[Trait("Category", "Unit")]
public class SpeechToTextStreamingRecordTests
{
    [Fact]
    public void TranscriptionOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new TranscriptionOptions();

        // Assert
        options.Language.Should().BeNull();
        options.ResponseFormat.Should().Be("json");
        options.Temperature.Should().BeNull();
        options.TimestampGranularity.Should().BeNull();
        options.Prompt.Should().BeNull();
    }

    [Fact]
    public void TranscriptionOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new TranscriptionOptions(
            Language: "en",
            ResponseFormat: "verbose_json",
            Temperature: 0.3,
            TimestampGranularity: "word",
            Prompt: "Technical discussion");

        // Assert
        options.Language.Should().Be("en");
        options.ResponseFormat.Should().Be("verbose_json");
        options.Temperature.Should().Be(0.3);
        options.TimestampGranularity.Should().Be("word");
        options.Prompt.Should().Be("Technical discussion");
    }

    [Fact]
    public void StreamingTranscriptionOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new StreamingTranscriptionOptions();

        // Assert
        options.Language.Should().BeNull();
        options.EnableInterimResults.Should().BeTrue();
        options.PunctuationMode.Should().Be("auto");
        options.ProfanityFilter.Should().BeFalse();
        options.SpeakerDiarization.Should().BeFalse();
        options.MaxSpeakers.Should().Be(2);
        options.ModelSize.Should().Be("base");
    }

    [Fact]
    public void StreamingTranscriptionOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new StreamingTranscriptionOptions(
            Language: "de",
            EnableInterimResults: false,
            PunctuationMode: "none",
            ProfanityFilter: true,
            SpeakerDiarization: true,
            MaxSpeakers: 5,
            ModelSize: "large");

        // Assert
        options.Language.Should().Be("de");
        options.EnableInterimResults.Should().BeFalse();
        options.PunctuationMode.Should().Be("none");
        options.ProfanityFilter.Should().BeTrue();
        options.SpeakerDiarization.Should().BeTrue();
        options.MaxSpeakers.Should().Be(5);
        options.ModelSize.Should().Be("large");
    }

    [Fact]
    public void TranscriptionEvent_AllPropertiesSet()
    {
        // Act
        var evt = new TranscriptionEvent(
            "Hello world", true, 0.95,
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), "en");

        // Assert
        evt.Text.Should().Be("Hello world");
        evt.IsFinal.Should().BeTrue();
        evt.Confidence.Should().Be(0.95);
        evt.Offset.Should().Be(TimeSpan.FromSeconds(1));
        evt.Duration.Should().Be(TimeSpan.FromSeconds(2));
        evt.Language.Should().Be("en");
    }

    [Fact]
    public void TranscriptionEvent_DefaultLanguage_IsNull()
    {
        // Act
        var evt = new TranscriptionEvent(
            "text", false, 0.5, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

        // Assert
        evt.Language.Should().BeNull();
    }

    [Fact]
    public void VoiceActivity_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<VoiceActivity>();

        // Assert
        values.Should().Contain(VoiceActivity.SpeechStart);
        values.Should().Contain(VoiceActivity.SpeechEnd);
        values.Should().Contain(VoiceActivity.Silence);
        values.Should().Contain(VoiceActivity.Noise);
    }

    [Fact]
    public void VoiceActivityEvent_AllPropertiesSet()
    {
        // Arrange
        var ts = DateTimeOffset.UtcNow;

        // Act
        var evt = new VoiceActivityEvent(VoiceActivity.SpeechStart, ts, 0.92);

        // Assert
        evt.Activity.Should().Be(VoiceActivity.SpeechStart);
        evt.Timestamp.Should().Be(ts);
        evt.Confidence.Should().Be(0.92);
    }

    [Fact]
    public void VoiceActivityEvent_DefaultConfidence_IsOne()
    {
        // Act
        var evt = new VoiceActivityEvent(VoiceActivity.Silence, DateTimeOffset.UtcNow);

        // Assert
        evt.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void VoiceActivityEvent_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = DateTimeOffset.UtcNow;
        var a = new VoiceActivityEvent(VoiceActivity.SpeechEnd, ts, 0.8);
        var b = new VoiceActivityEvent(VoiceActivity.SpeechEnd, ts, 0.8);

        // Assert
        a.Should().Be(b);
    }
}
