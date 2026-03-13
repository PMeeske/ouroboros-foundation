using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class InteractionEventTests
{
    [Fact]
    public void AudioChunkEvent_AsInteractionEvent_ShouldHaveBaseProperties()
    {
        var evt = new AudioChunkEvent
        {
            AudioData = new byte[] { 1, 2, 3 },
            Format = "pcm16",
            Source = InteractionSource.User,
        };

        evt.Id.Should().NotBeEmpty();
        evt.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        evt.Source.Should().Be(InteractionSource.User);
        evt.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void InteractionEvent_WithCorrelationId_ShouldSetCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        var evt = new AudioChunkEvent
        {
            AudioData = Array.Empty<byte>(),
            Format = "wav",
            Source = InteractionSource.Agent,
            CorrelationId = correlationId,
        };

        evt.CorrelationId.Should().Be(correlationId);
    }
}
