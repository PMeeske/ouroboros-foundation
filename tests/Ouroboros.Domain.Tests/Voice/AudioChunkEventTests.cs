using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class AudioChunkEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };

        var evt = new AudioChunkEvent
        {
            AudioData = data,
            Format = "pcm16",
            SampleRate = 44100,
            Channels = 2,
            IsFinal = true,
            Source = InteractionSource.User,
        };

        evt.AudioData.Should().BeEquivalentTo(data);
        evt.Format.Should().Be("pcm16");
        evt.SampleRate.Should().Be(44100);
        evt.Channels.Should().Be(2);
        evt.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void Defaults_ShouldHave16kHzMonoNonFinal()
    {
        var evt = new AudioChunkEvent
        {
            AudioData = Array.Empty<byte>(),
            Format = "wav",
            Source = InteractionSource.User,
        };

        evt.SampleRate.Should().Be(16000);
        evt.Channels.Should().Be(1);
        evt.IsFinal.Should().BeFalse();
    }
}
