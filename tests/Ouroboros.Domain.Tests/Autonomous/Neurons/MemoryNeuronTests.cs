namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class MemoryNeuronTests : IDisposable
{
    private readonly MemoryNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.memory");
        _neuron.Name.Should().Be("Semantic Memory");
        _neuron.Type.Should().Be(NeuronType.Memory);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("memory.*");
        _neuron.SubscribedTopics.Should().Contain("learning.fact");
        _neuron.SubscribedTopics.Should().Contain("experience.store");
        _neuron.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void DelegateProperties_DefaultToNull()
    {
        _neuron.StoreFunction.Should().BeNull();
        _neuron.SearchFunction.Should().BeNull();
        _neuron.EmbedFunction.Should().BeNull();
    }

    [Fact]
    public async Task ProcessMessage_LearningFact_StoresFact()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "learning.fact",
            Payload = "The sky is blue"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        // Fact stored internally (no exception)
    }

    [Fact]
    public async Task ProcessMessage_LearningFact_WithEmbedAndStore_CallsDelegates()
    {
        bool embedCalled = false;
        bool storeCalled = false;

        _neuron.EmbedFunction = (text, ct) =>
        {
            embedCalled = true;
            return Task.FromResult(new float[] { 0.1f, 0.2f });
        };

        _neuron.StoreFunction = (category, content, embedding, ct) =>
        {
            storeCalled = true;
            return Task.CompletedTask;
        };

        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "learning.fact",
            Payload = "Test fact"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(300);

        embedCalled.Should().BeTrue();
        storeCalled.Should().BeTrue();
    }
}
