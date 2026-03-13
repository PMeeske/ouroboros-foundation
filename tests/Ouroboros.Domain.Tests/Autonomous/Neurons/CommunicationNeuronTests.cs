namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class CommunicationNeuronTests : IDisposable
{
    private readonly CommunicationNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.communication");
        _neuron.Name.Should().Be("User Communication");
        _neuron.Type.Should().Be(NeuronType.Communication);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("user.*");
        _neuron.SubscribedTopics.Should().Contain("notification.*");
        _neuron.SubscribedTopics.Should().Contain("share.*");
        _neuron.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public async Task ProcessMessage_UserNotify_FiresOnUserMessage()
    {
        string? receivedMessage = null;
        IntentionPriority? receivedPriority = null;

        _neuron.OnUserMessage += (msg, priority) =>
        {
            receivedMessage = msg;
            receivedPriority = priority;
        };

        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "user.notify",
            Payload = "Hello user!",
            Priority = IntentionPriority.High
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        receivedMessage.Should().Be("Hello user!");
        receivedPriority.Should().Be(IntentionPriority.High);
    }

    [Fact]
    public async Task ProcessMessage_NotificationSend_FiresOnUserMessage()
    {
        string? receivedMessage = null;
        _neuron.OnUserMessage += (msg, _) => receivedMessage = msg;

        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "notification.send",
            Payload = "System notification"
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        receivedMessage.Should().Be("System notification");
    }

    [Fact]
    public async Task ProcessMessage_EmptyPayload_DoesNotFireEvent()
    {
        bool eventFired = false;
        _neuron.OnUserMessage += (_, _) => eventFired = true;

        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "user.notify",
            Payload = ""
        };

        _neuron.ReceiveMessage(message);
        await Task.Delay(200);

        eventFired.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessMessage_ShareInsight_ProposesIntention()
    {
        // Use a separate neuron instance to avoid double-dispose with network
        var neuron = new CommunicationNeuron();
        using var bus = new IntentionBus();
        using var network = new OuroborosNeuralNetwork(bus);
        network.RegisterNeuron(neuron);

        neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "share.insight",
            Payload = "Interesting discovery about patterns"
        };

        neuron.ReceiveMessage(message);
        await Task.Delay(200);

        bus.PendingCount.Should().BeGreaterThanOrEqualTo(1);
    }
}
