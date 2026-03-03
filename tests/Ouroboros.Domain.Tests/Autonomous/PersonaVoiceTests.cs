namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class PersonaVoiceTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var voice = new PersonaVoice("TestPersona", "voice-id-123", Rate: 1.2f, Pitch: 0.8f, Volume: 80);

        // Assert
        voice.PersonaName.Should().Be("TestPersona");
        voice.VoiceId.Should().Be("voice-id-123");
        voice.Rate.Should().Be(1.2f);
        voice.Pitch.Should().Be(0.8f);
        voice.Volume.Should().Be(80);
    }

    [Fact]
    public void Constructor_DefaultValues()
    {
        // Act
        var voice = new PersonaVoice("Default", "default-voice");

        // Assert
        voice.Rate.Should().Be(1.0f);
        voice.Pitch.Should().Be(1.0f);
        voice.Volume.Should().Be(100);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var voice1 = new PersonaVoice("A", "v1", 1.0f, 1.0f, 100);
        var voice2 = new PersonaVoice("A", "v1", 1.0f, 1.0f, 100);

        // Assert
        voice1.Should().Be(voice2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var voice1 = new PersonaVoice("A", "v1");
        var voice2 = new PersonaVoice("B", "v2");

        // Assert
        voice1.Should().NotBe(voice2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PersonaVoice("Original", "voice-1");

        // Act
        var modified = original with { Rate = 2.0f };

        // Assert
        modified.PersonaName.Should().Be("Original");
        modified.Rate.Should().Be(2.0f);
        original.Rate.Should().Be(1.0f);
    }
}
