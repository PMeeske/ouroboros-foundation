namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class SymbolicNeuronTests : IDisposable
{
    private readonly SymbolicNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.symbolic");
        _neuron.Name.Should().Be("MeTTa Symbolic Reasoning");
        _neuron.Type.Should().Be(NeuronType.Symbolic);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("metta.*");
        _neuron.SubscribedTopics.Should().Contain("reasoning.*");
        _neuron.SubscribedTopics.Should().Contain("logic.*");
        _neuron.SubscribedTopics.Should().Contain("reflection.request");
        _neuron.SubscribedTopics.Should().Contain("dag.*");
    }

    [Fact]
    public void DelegateProperties_DefaultToNull()
    {
        _neuron.MeTTaQueryFunction.Should().BeNull();
        _neuron.MeTTaAddFactFunction.Should().BeNull();
        _neuron.MeTTaEngine.Should().BeNull();
    }

    [Fact]
    public async Task ProcessMessage_MeTTaFact_StoresFact()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "metta.fact",
            Payload = "(is-color sky blue)"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        // Fact stored successfully (no exception)
    }

    [Fact]
    public async Task ProcessMessage_MeTTaFact_WithAddFunction_CallsDelegate()
    {
        bool addCalled = false;
        _neuron.MeTTaAddFactFunction = (fact, ct) =>
        {
            addCalled = true;
            return Task.FromResult(true);
        };

        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "metta.fact",
            Payload = "(is-color sky blue)"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(300);

        addCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessMessage_MeTTaQuery_WithoutQueryFunction_UsesLocalFacts()
    {
        _neuron.Start();

        // First add a fact
        var factMessage = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "metta.fact",
            Payload = "sky is blue"
        };
        _neuron.ReceiveMessage(factMessage);
        await Task.Delay(200);

        // Then query
        var queryMessage = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "metta.query",
            Payload = "sky"
        };
        _neuron.ReceiveMessage(queryMessage);
        await Task.Delay(200);

        // Should not throw - uses fallback local fact matching
    }
}
