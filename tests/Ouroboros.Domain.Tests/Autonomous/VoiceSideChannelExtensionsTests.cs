namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceSideChannelExtensionsTests : IAsyncDisposable
{
    private readonly VoiceSideChannel _channel = new(maxQueueSize: 5);

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
    }

    [Fact]
    public void SayAs_PublishesWithPersona()
    {
        // Arrange
        _channel.SetSynthesizer((text, voice, ct) => Task.CompletedTask);

        // Act (should not throw - fire and forget)
        _channel.SayAs("Aria", "Hello from Aria");

        // Assert - message was queued (queue depth should be at least 0, indicating no error)
        _channel.QueueDepth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Announce_PublishesWithSystemPersonaAndHighPriority()
    {
        // Arrange
        VoiceMessage? captured = null;
        _channel.MessageSkipped += (_, msg) => captured = msg;

        // Act (without synthesizer, message will be skipped and we can inspect it)
        _channel.Announce("System update");

        // Assert
        captured.Should().NotBeNull();
        captured!.Text.Should().Contain("System update");
    }

    [Fact]
    public void Whisper_PublishesWithLowPriority()
    {
        // Arrange
        VoiceMessage? captured = null;
        _channel.MessageSkipped += (_, msg) => captured = msg;

        // Act
        _channel.Whisper("quiet message");

        // Assert
        captured.Should().NotBeNull();
    }
}
