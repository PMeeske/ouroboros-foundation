namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class VoiceMessageTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var message = new VoiceMessage("Test message", "Ouroboros", VoicePriority.High, true);

        // Assert
        message.Text.Should().Be("Test message");
        message.PersonaName.Should().Be("Ouroboros");
        message.Priority.Should().Be(VoicePriority.High);
        message.Interrupt.Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultValues()
    {
        // Act
        var message = new VoiceMessage("Hello");

        // Assert
        message.Text.Should().Be("Hello");
        message.PersonaName.Should().BeNull();
        message.Priority.Should().Be(VoicePriority.Normal);
        message.Interrupt.Should().BeFalse();
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var msg1 = new VoiceMessage("Text", "Persona", VoicePriority.Normal);
        var msg2 = new VoiceMessage("Text", "Persona", VoicePriority.Normal);

        // Assert
        msg1.Should().Be(msg2);
    }

    [Fact]
    public void Equality_DifferentPriority_AreNotEqual()
    {
        // Arrange
        var msg1 = new VoiceMessage("Text", "Persona", VoicePriority.Normal);
        var msg2 = new VoiceMessage("Text", "Persona", VoicePriority.High);

        // Assert
        msg1.Should().NotBe(msg2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new VoiceMessage("Hello", "Ouroboros");

        // Act
        var modified = original with { Interrupt = true };

        // Assert
        modified.Text.Should().Be("Hello");
        modified.Interrupt.Should().BeTrue();
        original.Interrupt.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
public class SpeechRequestTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();

        // Act
        var request = new SpeechRequest("Speak this", "Agent", tcs);

        // Assert
        request.Text.Should().Be("Speak this");
        request.Persona.Should().Be("Agent");
        ReferenceEquals(request.Completion, tcs).Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultCompletion_IsNull()
    {
        // Act
        var request = new SpeechRequest("Text", "Persona");

        // Assert
        (request.Completion is null).Should().BeTrue();
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var req1 = new SpeechRequest("Text", "P");
        var req2 = new SpeechRequest("Text", "P");

        // Assert
        req1.Should().Be(req2);
    }
}

[Trait("Category", "Unit")]
public class ProactiveMessageEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var args = new ProactiveMessageEventArgs(
            "Test message",
            IntentionPriority.High,
            "TestNeuron",
            DateTime.UtcNow);

        // Assert
        args.Message.Should().Be("Test message");
        args.Priority.Should().Be(IntentionPriority.High);
        args.Source.Should().Be("TestNeuron");
    }
}
