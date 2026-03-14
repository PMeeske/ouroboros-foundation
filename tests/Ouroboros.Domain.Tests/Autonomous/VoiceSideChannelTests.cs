namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceSideChannelTests : IAsyncDisposable
{
    private readonly VoiceSideChannel _channel = new(maxQueueSize: 5);

    public async ValueTask DisposeAsync() => await _channel.DisposeAsync();

    // ═══════════════════════════════════════════════════════════════
    // Construction
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_DefaultState()
    {
        // IsEnabled is false because no synthesizer is set
        _channel.IsEnabled.Should().BeFalse();
        _channel.QueueDepth.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // SetSynthesizer
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetSynthesizer_NullSynthesizer_ThrowsArgumentNull()
    {
        var act = () => _channel.SetSynthesizer(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetSynthesizer_ValidSynthesizer_EnablesChannel()
    {
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        _channel.SetSynthesizer(synth);

        _channel.IsEnabled.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // SetLlmSanitizer
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetLlmSanitizer_NullSanitizer_ThrowsArgumentNull()
    {
        var act = () => _channel.SetLlmSanitizer(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetLlmSanitizer_ValidSanitizer_DoesNotThrow()
    {
        var act = () => _channel.SetLlmSanitizer((text, ct) => Task.FromResult(text));
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════
    // SetEnabled / SetDefaultPersona
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetEnabled_False_DisablesChannel()
    {
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        _channel.SetSynthesizer(synth);

        _channel.SetEnabled(false);
        _channel.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void SetDefaultPersona_ChangesDefault()
    {
        _channel.SetDefaultPersona("Aria");
        var voice = _channel.GetVoice(null);
        voice.PersonaName.Should().Be("Aria");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetVoice
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetVoice_KnownPersona_ReturnsConfiguredVoice()
    {
        var voice = _channel.GetVoice("Ouroboros");
        voice.PersonaName.Should().Be("Ouroboros");
        voice.VoiceId.Should().Be("onyx");
    }

    [Fact]
    public void GetVoice_UnknownPersona_ReturnsFallbackDefault()
    {
        var voice = _channel.GetVoice("UnknownPersona");
        voice.Should().NotBeNull();
        voice.PersonaName.Should().Be("Ouroboros");
    }

    [Fact]
    public void GetVoice_Null_ReturnsDefaultPersona()
    {
        var voice = _channel.GetVoice(null);
        voice.PersonaName.Should().Be("Ouroboros");
    }

    // ═══════════════════════════════════════════════════════════════
    // RegisterVoice
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RegisterVoice_AddsNewVoice()
    {
        var customVoice = new PersonaVoice("Custom", "alloy", 1.0f, 1.0f, 100);
        _channel.RegisterVoice(customVoice);

        var retrieved = _channel.GetVoice("Custom");
        retrieved.Should().Be(customVoice);
    }

    [Fact]
    public void RegisterVoice_OverridesExisting()
    {
        var newVoice = new PersonaVoice("Ouroboros", "echo", 0.5f, 0.5f, 80);
        _channel.RegisterVoice(newVoice);

        var retrieved = _channel.GetVoice("Ouroboros");
        retrieved.VoiceId.Should().Be("echo");
    }

    // ═══════════════════════════════════════════════════════════════
    // Say
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Say_EmptyText_DoesNotEnqueue()
    {
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        _channel.SetSynthesizer(synth);

        _channel.Say("");
        _channel.QueueDepth.Should().Be(0);
    }

    [Fact]
    public void Say_WhitespaceText_DoesNotEnqueue()
    {
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        _channel.SetSynthesizer(synth);

        _channel.Say("   ");
        _channel.QueueDepth.Should().Be(0);
    }

    [Fact]
    public void Say_WhenDisabled_FiresMessageSkipped()
    {
        _channel.SetEnabled(false);
        VoiceMessage? skipped = null;
        _channel.MessageSkipped += (_, msg) => skipped = msg;

        _channel.Say("Hello");

        skipped.Should().NotBeNull();
    }

    [Fact]
    public void Say_WhenNoSynthesizer_FiresMessageSkipped()
    {
        VoiceMessage? skipped = null;
        _channel.MessageSkipped += (_, msg) => skipped = msg;

        _channel.Say("Hello");

        skipped.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Interrupt
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Interrupt_EmptyText_DoesNotEnqueue()
    {
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        _channel.SetSynthesizer(synth);

        _channel.Interrupt("");
        _channel.QueueDepth.Should().Be(0);
    }

    [Fact]
    public void Interrupt_WhenDisabled_FiresMessageSkipped()
    {
        _channel.SetEnabled(false);
        VoiceMessage? skipped = null;
        _channel.MessageSkipped += (_, msg) => skipped = msg;

        _channel.Interrupt("Alert!");

        skipped.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // SetUseLlmSanitization
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetUseLlmSanitization_DoesNotThrow()
    {
        var act = () => _channel.SetUseLlmSanitization(false);
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════
    // GlobalSpeechLock
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GlobalSpeechLock_IsSharedInstance()
    {
        var lock1 = VoiceSideChannel.GlobalSpeechLock;
        var lock2 = VoiceSideChannel.GlobalSpeechLock;
        lock1.Should().BeSameAs(lock2);
    }
}
