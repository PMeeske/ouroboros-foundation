using Ouroboros.Providers.SpeechToText;

namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

/// <summary>
/// Additional edge case and equality tests for SpeechToText types.
/// </summary>
[Trait("Category", "Unit")]
public class SpeechToTextEdgeCaseTests
{
    [Fact]
    public void AudioChunk_RecordEquality_SameData_AreEqual()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02 };
        var a = new AudioChunk(data, "pcm16", 16000, 1, false);
        var b = new AudioChunk(data, "pcm16", 16000, 1, false);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AudioChunk_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AudioChunk(new byte[] { 0x01 }, "pcm16", 16000, 1, false);

        // Act
        var modified = original with { IsLastChunk = true };

        // Assert
        modified.IsLastChunk.Should().BeTrue();
        modified.SampleRate.Should().Be(16000);
    }

    [Fact]
    public void AudioChunk_EmptyData_IsValid()
    {
        // Act
        var chunk = new AudioChunk(Array.Empty<byte>(), "pcm16", 44100, 2, true);

        // Assert
        chunk.Data.Should().BeEmpty();
        chunk.IsLastChunk.Should().BeTrue();
    }

    [Fact]
    public void TranscriptionResult_AllPropertiesSet()
    {
        // Arrange
        var segments = new List<TranscriptionSegment>
        {
            new TranscriptionSegment("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1), 0.95, null)
        };

        // Act
        var result = new TranscriptionResult(
            "Hello world", "en", TimeSpan.FromSeconds(5),
            segments, new Dictionary<string, object> { ["model"] = "whisper" });

        // Assert
        result.Text.Should().Be("Hello world");
        result.Language.Should().Be("en");
        result.Duration.Should().Be(TimeSpan.FromSeconds(5));
        result.Segments.Should().HaveCount(1);
        result.Metadata.Should().ContainKey("model");
    }

    [Fact]
    public void TranscriptionResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var segments = new List<TranscriptionSegment>();
        var metadata = new Dictionary<string, object>();

        var a = new TranscriptionResult("text", "en", TimeSpan.FromSeconds(1), segments, metadata);
        var b = new TranscriptionResult("text", "en", TimeSpan.FromSeconds(1), segments, metadata);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TranscriptionSegment_WithSpeakerId()
    {
        // Act
        var segment = new TranscriptionSegment(
            "Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1),
            0.98, "speaker-1");

        // Assert
        segment.Text.Should().Be("Hello");
        segment.SpeakerId.Should().Be("speaker-1");
        segment.Confidence.Should().Be(0.98);
    }

    [Fact]
    public void TranscriptionSegment_DefaultSpeakerId_IsNull()
    {
        // Act
        var segment = new TranscriptionSegment(
            "text", TimeSpan.Zero, TimeSpan.FromSeconds(1), 0.9);

        // Assert
        segment.SpeakerId.Should().BeNull();
    }

    [Fact]
    public void TranscriptionEvent_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TranscriptionEvent("hello", true, 0.9, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        var b = new TranscriptionEvent("hello", true, 0.9, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TranscriptionOptions_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TranscriptionOptions(Language: "en", Temperature: 0.5);
        var b = new TranscriptionOptions(Language: "en", Temperature: 0.5);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void StreamingTranscriptionOptions_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new StreamingTranscriptionOptions();
        var b = new StreamingTranscriptionOptions();

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void VoiceActivityEvent_RecordEquality_DifferentActivities_AreNotEqual()
    {
        // Arrange
        var ts = DateTimeOffset.UtcNow;
        var a = new VoiceActivityEvent(VoiceActivity.SpeechStart, ts, 0.9);
        var b = new VoiceActivityEvent(VoiceActivity.SpeechEnd, ts, 0.9);

        // Assert
        a.Should().NotBe(b);
    }
}
