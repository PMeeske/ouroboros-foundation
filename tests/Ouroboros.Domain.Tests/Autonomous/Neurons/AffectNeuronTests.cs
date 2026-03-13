namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class AffectNeuronTests : IDisposable
{
    private readonly AffectNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.affect");
        _neuron.Name.Should().Be("Affective State");
        _neuron.Type.Should().Be(NeuronType.Affect);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("emotion.*");
        _neuron.SubscribedTopics.Should().Contain("affect.*");
        _neuron.SubscribedTopics.Should().Contain("mood.*");
        _neuron.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void InitialState_IsNeutral()
    {
        _neuron.Arousal.Should().Be(0.0);
        _neuron.Valence.Should().Be(0.0);
        _neuron.DominantEmotion.Should().Be("neutral");
    }

    [Fact]
    public async Task ProcessMessage_AffectPositive_IncreasesValence()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "affect.positive",
            Payload = "good"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        _neuron.Valence.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessMessage_AffectNegative_DecreasesValence()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "affect.negative",
            Payload = "bad"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        _neuron.Valence.Should().BeLessThan(0);
    }
}
