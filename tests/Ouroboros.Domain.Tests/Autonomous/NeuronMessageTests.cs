using Ouroboros.Domain.Autonomous;

namespace Ouroboros.Tests.Autonomous;

[Trait("Category", "Unit")]
public class NeuronMessageTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        // Arrange & Act
        var message = new NeuronMessage
        {
            SourceNeuron = "Memory",
            Topic = "recall",
            Payload = "some data"
        };

        // Assert
        message.Id.Should().NotBe(Guid.Empty);
        message.SourceNeuron.Should().Be("Memory");
        message.TargetNeuron.Should().BeNull();
        message.Topic.Should().Be("recall");
        message.Payload.Should().Be("some data");
        message.Priority.Should().Be(IntentionPriority.Normal);
        message.TtlSeconds.Should().Be(0);
        message.ExpectsResponse.Should().BeFalse();
        message.CorrelationId.Should().BeNull();
        message.Embedding.Should().BeNull();
    }

    [Fact]
    public void CreatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var message = new NeuronMessage
        {
            SourceNeuron = "Exec",
            Topic = "decision",
            Payload = new { action = "approve" }
        };

        // Assert
        message.CreatedAt.Should().BeOnOrAfter(before);
        message.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new NeuronMessage
        {
            SourceNeuron = "Safety",
            Topic = "alert",
            Payload = "warning"
        };

        // Act
        var modified = original with
        {
            TargetNeuron = "Executive",
            Priority = IntentionPriority.High,
            ExpectsResponse = true
        };

        // Assert
        modified.SourceNeuron.Should().Be("Safety");
        modified.TargetNeuron.Should().Be("Executive");
        modified.Priority.Should().Be(IntentionPriority.High);
        modified.ExpectsResponse.Should().BeTrue();
        original.TargetNeuron.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_CanBeSetForRequestResponse()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        // Act
        var request = new NeuronMessage
        {
            SourceNeuron = "A",
            Topic = "query",
            Payload = "?",
            ExpectsResponse = true,
            CorrelationId = correlationId
        };

        var response = new NeuronMessage
        {
            SourceNeuron = "B",
            TargetNeuron = "A",
            Topic = "response",
            Payload = "!",
            CorrelationId = correlationId
        };

        // Assert
        request.CorrelationId.Should().Be(response.CorrelationId);
    }

    [Fact]
    public void EachInstance_GetsUniqueId()
    {
        // Act
        var a = new NeuronMessage { SourceNeuron = "A", Topic = "t", Payload = "p" };
        var b = new NeuronMessage { SourceNeuron = "A", Topic = "t", Payload = "p" };

        // Assert
        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Embedding_CanBeSet()
    {
        // Arrange
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var message = new NeuronMessage
        {
            SourceNeuron = "Embed",
            Topic = "vec",
            Payload = "data",
            Embedding = embedding
        };

        // Assert
        message.Embedding.Should().BeEquivalentTo(embedding);
    }
}
