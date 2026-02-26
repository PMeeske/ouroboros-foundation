using Ouroboros.Providers.SpeechToText;

namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

[Trait("Category", "Unit")]
public class SpeechToTextRecordTests
{
    [Fact]
    public void AudioChunk_AllPropertiesSet()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var chunk = new AudioChunk(data, "pcm16", 44100, 2, true);

        // Assert
        chunk.Data.Should().BeEquivalentTo(data);
        chunk.Format.Should().Be("pcm16");
        chunk.SampleRate.Should().Be(44100);
        chunk.Channels.Should().Be(2);
        chunk.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void AudioChunk_DefaultValues_AreCorrect()
    {
        // Act
        var chunk = new AudioChunk(new byte[] { 0x00 }, "wav", 16000);

        // Assert
        chunk.Channels.Should().Be(1);
        chunk.IsFinal.Should().BeFalse();
    }

    [Fact]
    public void TranscriptionResult_AllPropertiesSet()
    {
        // Arrange
        var segments = new List<TranscriptionSegment>
        {
            new TranscriptionSegment("hello", 0.0, 0.5, 0.95),
            new TranscriptionSegment("world", 0.5, 1.0, 0.90)
        };

        // Act
        var result = new TranscriptionResult(
            "hello world", "en", 1.0, segments);

        // Assert
        result.Text.Should().Be("hello world");
        result.Language.Should().Be("en");
        result.Duration.Should().Be(1.0);
        result.Segments.Should().HaveCount(2);
    }

    [Fact]
    public void TranscriptionResult_DefaultValues_AreNull()
    {
        // Act
        var result = new TranscriptionResult("some text");

        // Assert
        result.Language.Should().BeNull();
        result.Duration.Should().BeNull();
        result.Segments.Should().BeNull();
    }

    [Fact]
    public void TranscriptionSegment_AllPropertiesSet()
    {
        // Act
        var segment = new TranscriptionSegment("word", 0.5, 1.0, 0.98);

        // Assert
        segment.Text.Should().Be("word");
        segment.Start.Should().Be(0.5);
        segment.End.Should().Be(1.0);
        segment.Confidence.Should().Be(0.98);
    }

    [Fact]
    public void TranscriptionSegment_DefaultConfidence_IsNull()
    {
        // Act
        var segment = new TranscriptionSegment("word", 0.0, 0.5);

        // Assert
        segment.Confidence.Should().BeNull();
    }

    [Fact]
    public void TranscriptionSegment_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TranscriptionSegment("word", 0.0, 0.5, 0.9);
        var b = new TranscriptionSegment("word", 0.0, 0.5, 0.9);

        // Assert
        a.Should().Be(b);
    }
}
