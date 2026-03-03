using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class CommunicationNeuronTests : IDisposable
{
    private readonly CommunicationNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.communication");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("User Communication");
    }

    [Fact]
    public void Type_Returns_Communication()
    {
        _sut.Type.Should().Be(NeuronType.Communication);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected()
    {
        _sut.SubscribedTopics.Should().Contain("user.*");
        _sut.SubscribedTopics.Should().Contain("notification.*");
        _sut.SubscribedTopics.Should().Contain("share.*");
        _sut.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void OnUserMessage_Event_Fires_On_UserNotify()
    {
        string? receivedMessage = null;
        IntentionPriority? receivedPriority = null;

        _sut.OnUserMessage += (msg, priority) =>
        {
            receivedMessage = msg;
            receivedPriority = priority;
        };

        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "user.notify",
            Payload = "Hello user!",
            Priority = IntentionPriority.High,
        };
        _sut.ReceiveMessage(message);
        Thread.Sleep(300);

        receivedMessage.Should().Be("Hello user!");
        receivedPriority.Should().Be(IntentionPriority.High);
    }

    [Fact]
    public void OnUserMessage_Event_Fires_On_NotificationSend()
    {
        string? receivedMessage = null;

        _sut.OnUserMessage += (msg, _) => receivedMessage = msg;
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "notification.send",
            Payload = "Alert!",
        };
        _sut.ReceiveMessage(message);
        Thread.Sleep(300);

        receivedMessage.Should().Be("Alert!");
    }

    [Fact]
    public void OnUserMessage_Not_Fired_For_Empty_Payload()
    {
        bool eventFired = false;
        _sut.OnUserMessage += (_, _) => eventFired = true;
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "user.notify",
            Payload = "",
        };
        _sut.ReceiveMessage(message);
        Thread.Sleep(300);

        eventFired.Should().BeFalse();
    }

    [Fact]
    public void ShareInsight_Processes_Without_Error()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "share.insight",
            Payload = "Interesting pattern detected",
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
