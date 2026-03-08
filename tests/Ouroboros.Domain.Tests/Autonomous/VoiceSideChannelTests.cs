namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceSideChannelTests : IAsyncDisposable
{
    private readonly VoiceSideChannel _channel = new(maxQueueSize: 5);

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
    }

    [Fact]
    public void IsEnabled_WithoutSynthesizer_ReturnsFalse()
    {
        // Assert
        _channel.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_WithSynthesizer_ReturnsTrue()
    {
        // Arrange
        _channel.SetSynthesizer((text, voice, ct) => Task.CompletedTask);

        // Assert
        _channel.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsEnabled_Disabled_ReturnsFalse()
    {
        // Arrange
        _channel.SetSynthesizer((text, voice, ct) => Task.CompletedTask);
        _channel.SetEnabled(false);

        // Assert
        _channel.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void SetSynthesizer_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _channel.SetSynthesizer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetLlmSanitizer_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _channel.SetLlmSanitizer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetVoice_KnownPersona_ReturnsCorrectVoice()
    {
        // Act
        var voice = _channel.GetVoice("Ouroboros");

        // Assert
        voice.PersonaName.Should().Be("Ouroboros");
        voice.VoiceId.Should().Be("onyx");
    }

    [Fact]
    public void GetVoice_UnknownPersona_ReturnsDefault()
    {
        // Act
        var voice = _channel.GetVoice("UnknownPersona");

        // Assert
        voice.PersonaName.Should().Be("Ouroboros");
    }

    [Fact]
    public void GetVoice_Null_ReturnsDefault()
    {
        // Act
        var voice = _channel.GetVoice(null);

        // Assert
        voice.PersonaName.Should().Be("Ouroboros");
    }

    [Fact]
    public void RegisterVoice_NewPersona_CanBeRetrieved()
    {
        // Arrange
        var customVoice = new PersonaVoice("CustomBot", "shimmer", 1.2f, 1.1f, 90);

        // Act
        _channel.RegisterVoice(customVoice);
        var retrieved = _channel.GetVoice("CustomBot");

        // Assert
        retrieved.Should().Be(customVoice);
    }

    [Fact]
    public void SetDefaultPersona_ChangesDefault()
    {
        // Arrange
        _channel.SetDefaultPersona("Aria");

        // Act
        var voice = _channel.GetVoice(null);

        // Assert
        voice.PersonaName.Should().Be("Aria");
    }

    [Fact]
    public void Say_EmptyText_DoesNotQueueMessage()
    {
        // Arrange
        _channel.SetSynthesizer((text, voice, ct) => Task.CompletedTask);

        // Act
        _channel.Say("");
        _channel.Say("   ");

        // Assert
        _channel.QueueDepth.Should().Be(0);
    }

    [Fact]
    public void Say_WithoutSynthesizer_FiresMessageSkipped()
    {
        // Arrange
        VoiceMessage? skipped = null;
        _channel.MessageSkipped += (_, msg) => skipped = msg;

        // Act
        _channel.Say("Hello");

        // Assert
        skipped.Should().NotBeNull();
    }

    [Fact]
    public void Interrupt_ClearsQueueFirst()
    {
        // Arrange
        _channel.SetSynthesizer(async (text, voice, ct) =>
        {
            await Task.Delay(5000, ct); // long delay
        });

        // Add several messages first
        _channel.Say("msg1");
        _channel.Say("msg2");
        _channel.Say("msg3");

        var skippedCount = 0;
        _channel.MessageSkipped += (_, _) => Interlocked.Increment(ref skippedCount);

        // Act
        _channel.Interrupt("URGENT!");

        // Assert - queue should have been cleared (skipped) before the interrupt message
        // The exact count depends on timing, but at least some should be skipped
        _channel.QueueDepth.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Interrupt_EmptyText_DoesNothing()
    {
        // Arrange
        _channel.SetSynthesizer((text, voice, ct) => Task.CompletedTask);

        // Act (should not throw)
        _channel.Interrupt("");
        _channel.Interrupt("   ");

        // Assert
        _channel.QueueDepth.Should().Be(0);
    }

    [Fact]
    public void GlobalSpeechLock_IsNotNull()
    {
        // Assert
        VoiceSideChannel.GlobalSpeechLock.Should().NotBeNull();
    }
}
