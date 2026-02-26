using Ouroboros.Providers.TextToSpeech;

namespace Ouroboros.Abstractions.Tests.Providers.TextToSpeech;

[Trait("Category", "Unit")]
public class TextToSpeechRecordTests
{
    [Fact]
    public void SpeechResult_AllPropertiesSet()
    {
        // Arrange
        var audioData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var result = new SpeechResult(audioData, "mp3", 2.5);

        // Assert
        result.AudioData.Should().BeEquivalentTo(audioData);
        result.Format.Should().Be("mp3");
        result.Duration.Should().Be(2.5);
    }

    [Fact]
    public void SpeechResult_DefaultDuration_IsNull()
    {
        // Act
        var result = new SpeechResult(new byte[] { 0x00 }, "wav");

        // Assert
        result.Duration.Should().BeNull();
    }

    [Fact]
    public void TextToSpeechOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new TextToSpeechOptions();

        // Assert
        options.Voice.Should().Be(TtsVoice.Alloy);
        options.Speed.Should().Be(1.0);
        options.Format.Should().Be("mp3");
        options.Model.Should().BeNull();
        options.IsWhisper.Should().BeFalse();
    }

    [Fact]
    public void TextToSpeechOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new TextToSpeechOptions(
            Voice: TtsVoice.Nova,
            Speed: 1.5,
            Format: "opus",
            Model: "tts-1-hd",
            IsWhisper: true);

        // Assert
        options.Voice.Should().Be(TtsVoice.Nova);
        options.Speed.Should().Be(1.5);
        options.Format.Should().Be("opus");
        options.Model.Should().Be("tts-1-hd");
        options.IsWhisper.Should().BeTrue();
    }

    [Fact]
    public void TtsVoice_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<TtsVoice>();

        // Assert
        values.Should().Contain(TtsVoice.Alloy);
        values.Should().Contain(TtsVoice.Echo);
        values.Should().Contain(TtsVoice.Fable);
        values.Should().Contain(TtsVoice.Onyx);
        values.Should().Contain(TtsVoice.Nova);
        values.Should().Contain(TtsVoice.Shimmer);
    }

    [Fact]
    public void SpeechChunk_AllPropertiesSet()
    {
        // Arrange
        var audioData = new byte[] { 0x01, 0x02 };

        // Act
        var chunk = new SpeechChunk(
            audioData, "pcm16", 0.5, "hello", true, false);

        // Assert
        chunk.AudioData.Should().BeEquivalentTo(audioData);
        chunk.Format.Should().Be("pcm16");
        chunk.DurationSeconds.Should().Be(0.5);
        chunk.Text.Should().Be("hello");
        chunk.IsSentenceEnd.Should().BeTrue();
        chunk.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void SpeechChunk_DefaultValues_AreCorrect()
    {
        // Act
        var chunk = new SpeechChunk(new byte[] { 0x00 }, "mp3", 1.0);

        // Assert
        chunk.Text.Should().BeNull();
        chunk.IsSentenceEnd.Should().BeFalse();
        chunk.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void SpeechChunk_FinalChunk_IsComplete()
    {
        // Act
        var chunk = new SpeechChunk(
            new byte[] { 0x00 }, "wav", 0.1, IsComplete: true);

        // Assert
        chunk.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void TextToSpeechOptions_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TextToSpeechOptions(TtsVoice.Nova, 1.5, "opus");
        var b = new TextToSpeechOptions(TtsVoice.Nova, 1.5, "opus");

        // Assert
        a.Should().Be(b);
    }
}
