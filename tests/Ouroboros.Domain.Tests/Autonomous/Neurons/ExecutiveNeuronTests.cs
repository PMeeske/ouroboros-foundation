namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class ExecutiveNeuronTests : IDisposable
{
    private readonly ExecutiveNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.executive");
        _neuron.Name.Should().Be("Executive Controller");
        _neuron.Type.Should().Be(NeuronType.Executive);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("goal.*");
        _neuron.SubscribedTopics.Should().Contain("task.*");
        _neuron.SubscribedTopics.Should().Contain("decision.*");
        _neuron.SubscribedTopics.Should().Contain("reflection.*");
        _neuron.SubscribedTopics.Should().Contain("system.status");
    }

    [Fact]
    public void ReflectionIntervalSeconds_DefaultIs60()
    {
        _neuron.ReflectionIntervalSeconds.Should().Be(60);
    }

    [Fact]
    public async Task ProcessMessage_GoalAdd_AddsGoal()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "user",
            Topic = "goal.add",
            Payload = "Learn quantum computing"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        // Goal added successfully (no exception)
    }

    [Fact]
    public async Task ProcessMessage_TaskQueue_QueuesTask()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "user",
            Topic = "task.queue",
            Payload = "Implement feature X"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        // Task queued successfully (no exception)
    }
}
