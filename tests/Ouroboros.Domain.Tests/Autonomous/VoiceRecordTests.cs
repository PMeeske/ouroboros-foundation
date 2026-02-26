namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceRecordTests
{
    [Fact]
    public void PersonaVoice_Constructor_SetsAllProperties()
    {
        // Act
        var voice = new PersonaVoice("Ouroboros", "onyx", 1.0f, 1.0f, 100);

        // Assert
        voice.PersonaName.Should().Be("Ouroboros");
        voice.VoiceId.Should().Be("onyx");
        voice.Rate.Should().Be(1.0f);
        voice.Pitch.Should().Be(1.0f);
        voice.Volume.Should().Be(100);
    }

    [Fact]
    public void PersonaVoice_DefaultRate_IsOne()
    {
        // Act
        var voice = new PersonaVoice("Test", "alloy");

        // Assert
        voice.Rate.Should().Be(1.0f);
        voice.Pitch.Should().Be(1.0f);
        voice.Volume.Should().Be(100);
    }

    [Fact]
    public void VoiceMessage_Constructor_SetsAllProperties()
    {
        // Act
        var msg = new VoiceMessage("Hello world", "Ouroboros", VoicePriority.High, true);

        // Assert
        msg.Text.Should().Be("Hello world");
        msg.PersonaName.Should().Be("Ouroboros");
        msg.Priority.Should().Be(VoicePriority.High);
        msg.Interrupt.Should().BeTrue();
    }

    [Fact]
    public void VoiceMessage_Defaults_AreReasonable()
    {
        // Act
        var msg = new VoiceMessage("Hello");

        // Assert
        msg.PersonaName.Should().BeNull();
        msg.Priority.Should().Be(VoicePriority.Normal);
        msg.Interrupt.Should().BeFalse();
    }

    [Fact]
    public void SpeechRequest_Constructor_SetsAllProperties()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();

        // Act
        var req = new SpeechRequest("Say this", "Aria", tcs);

        // Assert
        req.Text.Should().Be("Say this");
        req.Persona.Should().Be("Aria");
        Assert.Same(tcs, req.Completion);
    }

    [Fact]
    public void SpeechRequest_DefaultCompletion_IsNull()
    {
        // Act
        var req = new SpeechRequest("Text", "Persona");

        // Assert
        Assert.Null(req.Completion);
    }
}
