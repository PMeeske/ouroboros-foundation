using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class VoiceOutputEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var audio = new byte[] { 0xFF, 0xFE };

        var evt = new VoiceOutputEvent
        {
            AudioChunk = audio,
            Format = "mp3",
            SampleRate = 24000,
            DurationSeconds = 1.5,
            IsComplete = true,
            Emotion = "happy",
            Text = "Hello there",
            Source = InteractionSource.Agent,
        };

        evt.AudioChunk.Should().BeEquivalentTo(audio);
        evt.Format.Should().Be("mp3");
        evt.SampleRate.Should().Be(24000);
        evt.DurationSeconds.Should().Be(1.5);
        evt.IsComplete.Should().BeTrue();
        evt.Emotion.Should().Be("happy");
        evt.Text.Should().Be("Hello there");
    }

    [Fact]
    public void Defaults_ShouldHave24kHzAndNoEmotion()
    {
        var evt = new VoiceOutputEvent
        {
            AudioChunk = Array.Empty<byte>(),
            Format = "wav",
            Source = InteractionSource.Agent,
        };

        evt.SampleRate.Should().Be(24000);
        evt.DurationSeconds.Should().Be(0);
        evt.IsComplete.Should().BeFalse();
        evt.Emotion.Should().BeNull();
        evt.Text.Should().BeNull();
    }
}
