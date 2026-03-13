using Ouroboros.Domain.Autonomous;

namespace Ouroboros.Tests.Autonomous;

[Trait("Category", "Unit")]
public class PersonaVoiceTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
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
    public void Constructor_DefaultValues()
    {
        // Act
        var voice = new PersonaVoice("Test", "voice1");

        // Assert
        voice.Rate.Should().Be(1.0f);
        voice.Pitch.Should().Be(1.0f);
        voice.Volume.Should().Be(100);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new PersonaVoice("Echo", "echo", 0.7f, 0.9f, 100);
        var b = new PersonaVoice("Echo", "echo", 0.7f, 0.9f, 100);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var a = new PersonaVoice("Echo", "echo", 0.7f, 0.9f, 100);
        var b = new PersonaVoice("Aria", "nova", 1.3f, 1.1f, 100);

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PersonaVoice("Sage", "onyx", 0.8f, 0.95f, 95);

        // Act
        var modified = original with { Volume = 80 };

        // Assert
        modified.PersonaName.Should().Be("Sage");
        modified.Volume.Should().Be(80);
        original.Volume.Should().Be(95);
    }
}
