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
        var modified = original with { Format = "wav", Duration = 3.5 };

        // Assert
        modified.Format.Should().Be("wav");
        modified.Duration.Should().Be(3.5);
        modified.AudioData.Should().Equal(original.AudioData);
    }

    [Fact]
    public void SpeechChunk_AllPropertiesSet()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var chunk = new SpeechChunk(data, "mp3", 1.5, "Hello", true, true);

        // Assert
        chunk.AudioData.Should().HaveCount(3);
        chunk.Format.Should().Be("mp3");
        chunk.DurationSeconds.Should().Be(1.5);
        chunk.Text.Should().Be("Hello");
        chunk.IsSentenceEnd.Should().BeTrue();
        chunk.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void SpeechChunk_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var data = new byte[] { 0x01 };
        var a = new SpeechChunk(data, "mp3", 0.5);
        var b = new SpeechChunk(data, "mp3", 0.5);

        // Assert
        a.Should().Be(b);
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
            Format: "wav",
            Model: "tts-1-hd",
            IsWhisper: true);

        // Assert
        options.Voice.Should().Be(TtsVoice.Nova);
        options.Speed.Should().Be(1.5);
        options.Format.Should().Be("wav");
        options.Model.Should().Be("tts-1-hd");
        options.IsWhisper.Should().BeTrue();
    }

    [Fact]
    public void TextToSpeechOptions_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TextToSpeechOptions(Voice: TtsVoice.Echo, Speed: 1.0);
        var b = new TextToSpeechOptions(Voice: TtsVoice.Echo, Speed: 1.0);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TextToSpeechOptions_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TextToSpeechOptions(Voice: TtsVoice.Alloy);

        // Act
        var modified = original with { Speed = 2.0 };

        // Assert
        modified.Speed.Should().Be(2.0);
        modified.Voice.Should().Be(TtsVoice.Alloy);
    }

    [Fact]
    public void TtsVoice_AllValues_AreDefined()
    {
        // Assert
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Alloy).Should().BeTrue();
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Echo).Should().BeTrue();
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Fable).Should().BeTrue();
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Onyx).Should().BeTrue();
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Nova).Should().BeTrue();
        Enum.IsDefined(typeof(TtsVoice), TtsVoice.Shimmer).Should().BeTrue();
    }

    [Fact]
    public void TtsVoice_Equality_SameValue_AreEqual()
    {
        // Arrange
        var a = TtsVoice.Nova;
        var b = TtsVoice.Nova;

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TtsVoice_DefaultVoiceInOptions_IsAlloy()
    {
        // Act
        var options = new TextToSpeechOptions();

        // Assert
        options.Voice.Should().Be(TtsVoice.Alloy);
    }
}
