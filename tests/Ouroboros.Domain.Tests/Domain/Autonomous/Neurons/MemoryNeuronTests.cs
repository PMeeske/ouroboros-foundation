using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class MemoryNeuronTests : IDisposable
{
    private readonly MemoryNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.memory");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("Semantic Memory");
    }

    [Fact]
    public void Type_Returns_Memory()
    {
        _sut.Type.Should().Be(NeuronType.Memory);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected()
    {
        _sut.SubscribedTopics.Should().Contain("memory.*");
        _sut.SubscribedTopics.Should().Contain("learning.fact");
        _sut.SubscribedTopics.Should().Contain("experience.store");
        _sut.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void StoreFunction_Initially_Null()
    {
        _sut.StoreFunction.Should().BeNull();
    }

    [Fact]
    public void SearchFunction_Initially_Null()
    {
        _sut.SearchFunction.Should().BeNull();
    }

    [Fact]
    public void EmbedFunction_Initially_Null()
    {
        _sut.EmbedFunction.Should().BeNull();
    }

    [Fact]
    public void StoreFunction_CanBeSet()
    {
        Func<string, string, float[], CancellationToken, Task> fn = (_, _, _, _) => Task.CompletedTask;
        _sut.StoreFunction = fn;
        _sut.StoreFunction.Should().BeSameAs(fn);
    }

    [Fact]
    public void SearchFunction_CanBeSet()
    {
        Func<float[], int, CancellationToken, Task<IReadOnlyList<string>>> fn = (_, _, _) =>
            Task.FromResult<IReadOnlyList<string>>(new List<string>());
        _sut.SearchFunction = fn;
        _sut.SearchFunction.Should().BeSameAs(fn);
    }

    [Fact]
    public void EmbedFunction_CanBeSet()
    {
        Func<string, CancellationToken, Task<float[]>> fn = (_, _) =>
            Task.FromResult(new float[] { 1.0f });
        _sut.EmbedFunction = fn;
        _sut.EmbedFunction.Should().BeSameAs(fn);
    }

    [Fact]
    public void ReceiveMessage_LearningFact_ProcessedWithoutExternalDeps()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "learning.fact",
            Payload = "The sky is blue",
        };

        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReceiveMessage_MemoryRecall_Fallback_WithoutFunctions()
    {
        _sut.Start();

        // First store a fact
        NeuronMessage storeMsg = new()
        {
            SourceNeuron = "test",
            Topic = "learning.fact",
            Payload = "Water is essential for life",
        };
        _sut.ReceiveMessage(storeMsg);
        Thread.Sleep(100);

        // Then recall
        NeuronMessage recallMsg = new()
        {
            SourceNeuron = "test",
            Topic = "memory.recall",
            Payload = "water",
        };
        _sut.ReceiveMessage(recallMsg);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReceiveMessage_MemoryConsolidate_Runs()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "memory.consolidate",
            Payload = "consolidate now",
        };

        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
