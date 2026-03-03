using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class SymbolicNeuronTests : IDisposable
{
    private readonly SymbolicNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.symbolic");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("MeTTa Symbolic Reasoning");
    }

    [Fact]
    public void Type_Returns_Symbolic()
    {
        _sut.Type.Should().Be(NeuronType.Symbolic);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected()
    {
        _sut.SubscribedTopics.Should().Contain("metta.*");
        _sut.SubscribedTopics.Should().Contain("reasoning.*");
        _sut.SubscribedTopics.Should().Contain("logic.*");
        _sut.SubscribedTopics.Should().Contain("reflection.request");
        _sut.SubscribedTopics.Should().Contain("dag.*");
    }

    [Fact]
    public void MeTTaEngine_Initially_Null()
    {
        _sut.MeTTaEngine.Should().BeNull();
    }

    [Fact]
    public void MeTTaQueryFunction_Initially_Null()
    {
        _sut.MeTTaQueryFunction.Should().BeNull();
    }

    [Fact]
    public void MeTTaAddFactFunction_Initially_Null()
    {
        _sut.MeTTaAddFactFunction.Should().BeNull();
    }

    [Fact]
    public void MeTTaQueryFunction_CanBeSet()
    {
        Func<string, CancellationToken, Task<string>> fn = (_, _) => Task.FromResult("result");
        _sut.MeTTaQueryFunction = fn;
        _sut.MeTTaQueryFunction.Should().BeSameAs(fn);
    }

    [Fact]
    public void ReceiveMessage_MettaFact_ProcessesWithoutError()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "metta.fact",
            Payload = "(is-a dog animal)",
        };

        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReceiveMessage_MettaRule_ProcessesWithoutError()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "metta.rule",
            Payload = "(= (is-mammal $x) (and (is-animal $x) (has-fur $x)))",
        };

        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReceiveMessage_MettaQuery_Fallback_Without_Engine()
    {
        _sut.Start();

        // First add a fact
        NeuronMessage factMsg = new()
        {
            SourceNeuron = "test",
            Topic = "metta.fact",
            Payload = "dog is a pet",
        };
        _sut.ReceiveMessage(factMsg);
        Thread.Sleep(100);

        // Then query
        NeuronMessage queryMsg = new()
        {
            SourceNeuron = "test",
            Topic = "metta.query",
            Payload = "dog",
        };
        _sut.ReceiveMessage(queryMsg);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
