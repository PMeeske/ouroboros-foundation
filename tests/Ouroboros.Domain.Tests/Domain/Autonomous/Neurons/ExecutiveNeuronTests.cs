using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class ExecutiveNeuronTests : IDisposable
{
    private readonly ExecutiveNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.executive");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("Executive Controller");
    }

    [Fact]
    public void Type_Returns_Executive()
    {
        _sut.Type.Should().Be(NeuronType.Executive);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected_Topics()
    {
        _sut.SubscribedTopics.Should().Contain("goal.*");
        _sut.SubscribedTopics.Should().Contain("task.*");
        _sut.SubscribedTopics.Should().Contain("decision.*");
        _sut.SubscribedTopics.Should().Contain("reflection.*");
        _sut.SubscribedTopics.Should().Contain("system.status");
    }

    [Fact]
    public void ReflectionIntervalSeconds_Default_Is_60()
    {
        _sut.ReflectionIntervalSeconds.Should().Be(60);
    }

    [Fact]
    public void ReflectionIntervalSeconds_CanBeSet()
    {
        _sut.ReflectionIntervalSeconds = 120;
        _sut.ReflectionIntervalSeconds.Should().Be(120);
    }

    [Fact]
    public void Start_Makes_Neuron_Active()
    {
        _sut.Start();
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_Deactivates_Neuron()
    {
        _sut.Start();
        await _sut.StopAsync();
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ReceiveMessage_GoalAdd_Accepted()
    {
        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "goal.add",
            Payload = "Learn new skill",
        };

        _sut.Start();
        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        // No exception means success; ExecutiveNeuron tracks goals internally
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ReceiveMessage_TaskQueue_Accepted()
    {
        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "task.queue",
            Payload = "Process data batch",
        };

        _sut.Start();
        _sut.ReceiveMessage(message);
        Thread.Sleep(200);

        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Network_Initially_Null()
    {
        _sut.Network.Should().BeNull();
    }

    [Fact]
    public void IntentionBus_Initially_Null()
    {
        _sut.IntentionBus.Should().BeNull();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
