namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class TrainingInteractionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var feedback = new List<string> { "good response", "could be shorter" };
        var metrics = new Dictionary<string, object>
        {
            ["latency_ms"] = 250,
            ["tokens"] = 150,
        };

        // Act
        var interaction = new TrainingInteraction(
            id, "What is AI?", "AI is...", timestamp, 0.85, feedback, metrics);

        // Assert
        interaction.Id.Should().Be(id);
        interaction.UserMessage.Should().Be("What is AI?");
        interaction.SystemResponse.Should().Be("AI is...");
        interaction.Timestamp.Should().Be(timestamp);
        interaction.UserSatisfaction.Should().Be(0.85);
        interaction.Feedback.Should().HaveCount(2);
        interaction.Metrics.Should().ContainKey("latency_ms");
    }

    [Fact]
    public void Constructor_NullOptionalProperties_AreAllowed()
    {
        // Act
        var interaction = new TrainingInteraction(
            Guid.NewGuid(), "msg", "response", DateTime.UtcNow, null, null, null);

        // Assert
        interaction.UserSatisfaction.Should().BeNull();
        interaction.Feedback.Should().BeNull();
        interaction.Metrics.Should().BeNull();
    }
}
