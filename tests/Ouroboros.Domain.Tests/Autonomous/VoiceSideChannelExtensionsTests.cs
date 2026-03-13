namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceSideChannelExtensionsTests : IAsyncDisposable
{
    private readonly VoiceSideChannel _channel;
    private readonly List<VoiceMessage> _skippedMessages = [];

    public VoiceSideChannelExtensionsTests()
    {
        _channel = new VoiceSideChannel();
        // Channel has no synthesizer, so all messages will be skipped - we capture them
        _channel.MessageSkipped += (_, msg) => _skippedMessages.Add(msg);
    }

    public async ValueTask DisposeAsync() => await _channel.DisposeAsync().ConfigureAwait(false);

    [Fact]
    public void SayAs_DelegatesToSayWithPersona()
    {
        _channel.SayAs("Aria", "Hello from Aria");

        _skippedMessages.Should().ContainSingle();
        _skippedMessages[0].PersonaName.Should().Be("Aria");
    }

    [Fact]
    public void Announce_UsesSystemPersonaAndHighPriority()
    {
        _channel.Announce("System alert!");

        _skippedMessages.Should().ContainSingle();
        _skippedMessages[0].PersonaName.Should().Be("System");
        _skippedMessages[0].Priority.Should().Be(VoicePriority.High);
    }

    [Fact]
    public void Whisper_UsesLowPriority()
    {
        _channel.Whisper("Quiet message");

        _skippedMessages.Should().ContainSingle();
        _skippedMessages[0].Priority.Should().Be(VoicePriority.Low);
    }

    [Fact]
    public void Whisper_WithPersona_SetsPersona()
    {
        _channel.Whisper("Quiet message", "Echo");

        _skippedMessages.Should().ContainSingle();
        _skippedMessages[0].PersonaName.Should().Be("Echo");
    }

    [Fact]
    public void Whisper_WithoutPersona_UsesDefault()
    {
        _channel.Whisper("Quiet message");

        _skippedMessages.Should().ContainSingle();
        // Default persona is null in Say, which the channel resolves to "Ouroboros"
    }
}
