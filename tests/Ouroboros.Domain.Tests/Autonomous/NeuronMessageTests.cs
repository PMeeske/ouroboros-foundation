namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class NeuronMessageTests
{
    [Fact]
    public void Constructor_RequiredProperties_AreSet()
    {
        var message = new NeuronMessage
        {
            SourceNeuron = "neuron.executive",
            Topic = "goal.add",
            Payload = "test payload"
        };

        message.SourceNeuron.Should().Be("neuron.executive");
        message.Topic.Should().Be("goal.add");
        message.Payload.Should().Be("test payload");
    }

    [Fact]
    public void Constructor_DefaultValues_AreExpected()
    {
        var message = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "topic",
            Payload = "data"
        };

        message.Id.Should().NotBe(Guid.Empty);
        message.TargetNeuron.Should().BeNull();
        message.Priority.Should().Be(IntentionPriority.Normal);
        message.TtlSeconds.Should().Be(0);
        message.ExpectsResponse.Should().BeFalse();
        message.CorrelationId.Should().BeNull();
        message.Embedding.Should().BeNull();
    }

    [Fact]
    public void Id_IsUniquePerInstance()
    {
        var m1 = new NeuronMessage { SourceNeuron = "s", Topic = "t", Payload = "p" };
        var m2 = new NeuronMessage { SourceNeuron = "s", Topic = "t", Payload = "p" };

        m1.Id.Should().NotBe(m2.Id);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "topic",
            Payload = "data",
            Priority = IntentionPriority.Normal
        };

        var modified = original with { Priority = IntentionPriority.High };

        modified.Priority.Should().Be(IntentionPriority.High);
        original.Priority.Should().Be(IntentionPriority.Normal);
        modified.SourceNeuron.Should().Be("src");
    }

    [Fact]
    public void TargetedMessage_SetsTargetNeuron()
    {
        var message = new NeuronMessage
        {
            SourceNeuron = "neuron.executive",
            TargetNeuron = "neuron.memory",
            Topic = "memory.recall",
            Payload = "query"
        };

        message.TargetNeuron.Should().Be("neuron.memory");
    }

    [Fact]
    public void CorrelationId_CanBeSetForRequestResponse()
    {
        var requestId = Guid.NewGuid();
        var response = new NeuronMessage
        {
            SourceNeuron = "neuron.memory",
            Topic = "memory.recall.response",
            Payload = "result",
            CorrelationId = requestId
        };

        response.CorrelationId.Should().Be(requestId);
    }

    [Fact]
    public void CreatedAt_IsCloseToNow()
    {
        var before = DateTime.UtcNow;
        var message = new NeuronMessage { SourceNeuron = "s", Topic = "t", Payload = "p" };
        var after = DateTime.UtcNow;

        message.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
