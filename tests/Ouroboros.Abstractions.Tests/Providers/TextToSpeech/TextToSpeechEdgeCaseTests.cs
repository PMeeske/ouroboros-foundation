using Ouroboros.Providers.TextToSpeech;

namespace Ouroboros.Abstractions.Tests.Providers.TextToSpeech;

/// <summary>
/// Additional edge case and equality tests for TextToSpeech types.
/// </summary>
[Trait("Category", "Unit")]
public class TextToSpeechEdgeCaseTests
{
    [Fact]
    public void SpeechResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02 };
        var a = new SpeechResult(data, "mp3", 2.5);
        var b = new SpeechResult(data, "mp3", 2.5);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void SpeechResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new SpeechResult(new byte[] { 0x01 }, "mp3", 1.0);

        // Act
        var modified = original with { Format = "wav", DurationSeconds = 3.5 };

        // Assert
        modified.Format.Should().Be("wav");
        modified.DurationSeconds.Should().Be(3.5);
        modified.AudioData.Should().Equal(original.AudioData);
    }

    [Fact]
    public void SpeechChunk_AllPropertiesSet()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var chunk = new SpeechChunk(data, 0, true);

        // Assert
        chunk.AudioData.Should().HaveCount(3);
        chunk.SequenceNumber.Should().Be(0);
        chunk.IsLastChunk.Should().BeTrue();
    }

    [Fact]
    public void SpeechChunk_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var data = new byte[] { 0x01 };
        var a = new SpeechChunk(data, 5, false);
        var b = new SpeechChunk(data, 5, false);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TextToSpeechOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new TextToSpeechOptions();

        // Assert
        options.Voice.Should().BeNull();
        options.Speed.Should().Be(1.0);
        options.Pitch.Should().Be(1.0);
        options.Format.Should().Be("mp3");
        options.SampleRate.Should().BeNull();
    }

    [Fact]
    public void TextToSpeechOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new TextToSpeechOptions(
            Voice: "en-US-Neural",
            Speed: 1.5,
            Pitch: 0.8,
            Format: "wav",
            SampleRate: 44100);

        // Assert
        options.Voice.Should().Be("en-US-Neural");
        options.Speed.Should().Be(1.5);
        options.Pitch.Should().Be(0.8);
        options.Format.Should().Be("wav");
        options.SampleRate.Should().Be(44100);
    }

    [Fact]
    public void TextToSpeechOptions_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TextToSpeechOptions(Voice: "voice1", Speed: 1.0);
        var b = new TextToSpeechOptions(Voice: "voice1", Speed: 1.0);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TextToSpeechOptions_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TextToSpeechOptions(Voice: "default");

        // Act
        var modified = original with { Speed = 2.0 };

        // Assert
        modified.Speed.Should().Be(2.0);
        modified.Voice.Should().Be("default");
    }

    [Fact]
    public void TtsVoice_AllPropertiesSet()
    {
        // Act
        var voice = new TtsVoice(
            "voice-1", "Neural Voice", "en-US", "Female",
            new List<string> { "conversational", "narration" });

        // Assert
        voice.Id.Should().Be("voice-1");
        voice.Name.Should().Be("Neural Voice");
        voice.Language.Should().Be("en-US");
        voice.Gender.Should().Be("Female");
        voice.SupportedStyles.Should().HaveCount(2);
    }

    [Fact]
    public void TtsVoice_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var styles = new List<string> { "style1" };
        var a = new TtsVoice("id", "name", "en", "Male", styles);
        var b = new TtsVoice("id", "name", "en", "Male", styles);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TtsVoice_DefaultStyles_IsNull()
    {
        // Act
        var voice = new TtsVoice("id", "name", "en", "Female");

        // Assert
        voice.SupportedStyles.Should().BeNull();
    }
}
