namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceSynthesizerTests
{
    [Fact]
    public void VoiceSynthesizer_IsDelegate()
    {
        // VoiceSynthesizer is a delegate type: Task VoiceSynthesizer(string text, PersonaVoice voice, CancellationToken ct)
        VoiceSynthesizer synth = (text, voice, ct) => Task.CompletedTask;
        synth.Should().NotBeNull();
    }

    [Fact]
    public async Task VoiceSynthesizer_CanBeInvokedWithParameters()
    {
        string? capturedText = null;
        PersonaVoice? capturedVoice = null;

        VoiceSynthesizer synth = (text, voice, ct) =>
        {
            capturedText = text;
            capturedVoice = voice;
            return Task.CompletedTask;
        };

        var voice = new PersonaVoice("Test", "alloy", 1.0f, 1.0f, 100);
        await synth("Hello", voice, CancellationToken.None);

        capturedText.Should().Be("Hello");
        capturedVoice.Should().Be(voice);
    }

    [Fact]
    public async Task VoiceSynthesizer_SupportsCancellation()
    {
        using var cts = new CancellationTokenSource();
        bool wasCancelled = false;

        VoiceSynthesizer synth = (text, voice, ct) =>
        {
            wasCancelled = ct.IsCancellationRequested;
            return Task.CompletedTask;
        };

        cts.Cancel();
        var voice = new PersonaVoice("Test", "alloy");
        await synth("Hello", voice, cts.Token);

        wasCancelled.Should().BeTrue();
    }

    [Fact]
    public void PersonaVoice_ConstructsWithAllParameters()
    {
        var voice = new PersonaVoice("Aria", "nova", 1.3f, 1.1f, 95);

        voice.PersonaName.Should().Be("Aria");
        voice.VoiceId.Should().Be("nova");
        voice.Rate.Should().Be(1.3f);
        voice.Pitch.Should().Be(1.1f);
        voice.Volume.Should().Be(95);
    }

    [Fact]
    public void PersonaVoice_DefaultValues()
    {
        var voice = new PersonaVoice("Test", "alloy");

        voice.Rate.Should().Be(1.0f);
        voice.Pitch.Should().Be(1.0f);
        voice.Volume.Should().Be(100);
    }
}
