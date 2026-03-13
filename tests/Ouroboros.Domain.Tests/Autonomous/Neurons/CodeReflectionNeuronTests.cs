namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class CodeReflectionNeuronTests : IDisposable
{
    private readonly CodeReflectionNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.code");
        _neuron.Name.Should().Be("Code Reflection");
        _neuron.Type.Should().Be(NeuronType.CodeReflection);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("code.*");
        _neuron.SubscribedTopics.Should().Contain("self.modify");
        _neuron.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void CodeScanIntervalSeconds_DefaultIs300()
    {
        _neuron.CodeScanIntervalSeconds.Should().Be(300);
    }

    [Fact]
    public void CodeScanIntervalSeconds_CanBeModified()
    {
        _neuron.CodeScanIntervalSeconds = 600;
        _neuron.CodeScanIntervalSeconds.Should().Be(600);
    }
}
