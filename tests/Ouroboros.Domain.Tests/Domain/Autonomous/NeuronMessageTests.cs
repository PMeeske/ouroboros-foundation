namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class NeuronMessageTests
{
    [Fact]
    public void Constructor_RequiredProperties_AreSet()
    {
        // Act
        var msg = new NeuronMessage
        {
            SourceNeuron = "CoreNeuron",
            Topic = "self-reflection",
            Payload = "Need to analyze recent performance",
        };

        // Assert
        msg.SourceNeuron.Should().Be("CoreNeuron");
        msg.Topic.Should().Be("self-reflection");
        msg.Payload.Should().Be("Need to analyze recent performance");
    }

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        // Act
        var msg = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "test",
            Payload = "data",
        };

        // Assert
        msg.Id.Should().NotBeEmpty();
        msg.TargetNeuron.Should().BeNull();
        msg.Priority.Should().Be(IntentionPriority.Normal);
        msg.TtlSeconds.Should().Be(0);
        msg.ExpectsResponse.Should().BeFalse();
        msg.CorrelationId.Should().BeNull();
        msg.Embedding.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllOptionalProperties_SetsValues()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var msg = new NeuronMessage
        {
            SourceNeuron = "src",
            TargetNeuron = "tgt",
            Topic = "query",
            Payload = new { question = "test" },
            Priority = IntentionPriority.High,
            TtlSeconds = 60,
            ExpectsResponse = true,
            CorrelationId = correlationId,
            Embedding = embedding,
        };

        // Assert
        msg.TargetNeuron.Should().Be("tgt");
        msg.Priority.Should().Be(IntentionPriority.High);
        msg.TtlSeconds.Should().Be(60);
        msg.ExpectsResponse.Should().BeTrue();
        msg.CorrelationId.Should().Be(correlationId);
        msg.Embedding.Should().HaveCount(3);
    }

    [Fact]
    public void BroadcastMessage_TargetNeuronIsNull()
    {
        // Act
        var msg = new NeuronMessage
        {
            SourceNeuron = "CoreNeuron",
            Topic = "broadcast",
            Payload = "global message",
        };

        // Assert
        msg.TargetNeuron.Should().BeNull();
    }
}
