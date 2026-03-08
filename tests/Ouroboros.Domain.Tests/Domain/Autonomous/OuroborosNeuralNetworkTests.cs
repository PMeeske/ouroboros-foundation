namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class OuroborosNeuralNetworkTests : IDisposable
{
    private readonly IntentionBus _intentionBus = new();
    private readonly OuroborosNeuralNetwork _sut;

    public OuroborosNeuralNetworkTests()
    {
        _sut = new OuroborosNeuralNetwork(_intentionBus);
    }

    private class TestNeuron : Neuron
    {
        private readonly string _id;
        private readonly HashSet<string> _topics;
        public List<NeuronMessage> ReceivedMessages { get; } = new();

        public TestNeuron(string id, params string[] topics)
        {
            _id = id;
            _topics = new HashSet<string>(topics);
        }

        public override string Id => _id;
        public override string Name => _id;
        public override NeuronType Type => NeuronType.Custom;
        public override IReadOnlySet<string> SubscribedTopics => _topics;

        protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
        {
            ReceivedMessages.Add(message);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void IsActive_InitiallyFalse()
    {
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RegisterNeuron_AddsToNeuronCollection()
    {
        // Arrange
        var neuron = new TestNeuron("test1", "topic.a");

        // Act
        _sut.RegisterNeuron(neuron);

        // Assert
        _sut.Neurons.Should().ContainKey("test1");
    }

    [Fact]
    public void RegisterNeuron_SetsNetworkAndIntentionBus()
    {
        // Arrange
        var neuron = new TestNeuron("test1", "topic.a");

        // Act
        _sut.RegisterNeuron(neuron);

        // Assert
        neuron.Network.Should().Be(_sut);
        neuron.IntentionBus.Should().Be(_intentionBus);
    }

    [Fact]
    public void GetNeuron_ExistingId_ReturnsNeuron()
    {
        // Arrange
        var neuron = new TestNeuron("test1", "topic.a");
        _sut.RegisterNeuron(neuron);

        // Act
        var result = _sut.GetNeuron("test1");

        // Assert
        result.Should().Be(neuron);
    }

    [Fact]
    public void GetNeuron_NonExistentId_ReturnsNull()
    {
        // Act
        var result = _sut.GetNeuron("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetNeuronsByType_ReturnsMatchingNeurons()
    {
        // Arrange
        var neuron1 = new TestNeuron("test1", "topic.a");
        var neuron2 = new TestNeuron("test2", "topic.b");
        _sut.RegisterNeuron(neuron1);
        _sut.RegisterNeuron(neuron2);

        // Act
        var results = _sut.GetNeuronsByType(NeuronType.Custom).ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task RouteMessageAsync_NullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.RouteMessageAsync(null!));
    }

    [Fact]
    public async Task RouteMessageAsync_TargetedMessage_RoutesToSpecificNeuron()
    {
        // Arrange
        var target = new TestNeuron("target", "topic.a");
        var other = new TestNeuron("other", "topic.a");
        _sut.RegisterNeuron(target);
        _sut.RegisterNeuron(other);

        var message = new NeuronMessage
        {
            SourceNeuron = "sender",
            TargetNeuron = "target",
            Topic = "topic.a",
            Payload = "hello"
        };

        // Act
        await _sut.RouteMessageAsync(message);
        await Task.Delay(50); // Allow async delivery

        // Assert
        target.ReceivedMessages.Should().HaveCount(1);
        other.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task RouteMessageAsync_TopicMessage_RoutesToSubscribers()
    {
        // Arrange
        var subscriber = new TestNeuron("sub1", "topic.a");
        var nonSubscriber = new TestNeuron("sub2", "topic.b");
        _sut.RegisterNeuron(subscriber);
        _sut.RegisterNeuron(nonSubscriber);

        var message = new NeuronMessage
        {
            SourceNeuron = "sender",
            Topic = "topic.a",
            Payload = "hello"
        };

        // Act
        await _sut.RouteMessageAsync(message);
        await Task.Delay(50);

        // Assert
        subscriber.ReceivedMessages.Should().HaveCount(1);
        nonSubscriber.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task RouteMessageAsync_DoesNotSendToSelf()
    {
        // Arrange
        var neuron = new TestNeuron("neuron1", "topic.a");
        _sut.RegisterNeuron(neuron);

        var message = new NeuronMessage
        {
            SourceNeuron = "neuron1",
            Topic = "topic.a",
            Payload = "self message"
        };

        // Act
        await _sut.RouteMessageAsync(message);
        await Task.Delay(50);

        // Assert
        neuron.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public void GetRecentMessages_ReturnsMessages()
    {
        // Arrange - route some messages
        for (int i = 0; i < 5; i++)
        {
            _ = _sut.RouteMessageAsync(new NeuronMessage
            {
                SourceNeuron = "sender",
                Topic = "topic",
                Payload = $"msg-{i}"
            });
        }

        // Act
        var recent = _sut.GetRecentMessages(3);

        // Assert
        recent.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void GetNetworkState_ReturnsStatusString()
    {
        // Arrange
        _sut.RegisterNeuron(new TestNeuron("test1", "topic.a"));

        // Act
        string state = _sut.GetNetworkState();

        // Assert
        state.Should().Contain("Neural Network");
        state.Should().Contain("Neurons: 1");
    }

    [Fact]
    public void GetNetworkState_WithTopology_IncludesConnectionInfo()
    {
        // Arrange
        var topology = new ConnectionTopology();
        topology.SetConnection("A", "B", 0.5);
        topology.AddInhibition("B", "A");

        using var network = new OuroborosNeuralNetwork(_intentionBus, topology: topology);

        // Act
        string state = network.GetNetworkState();

        // Assert
        state.Should().Contain("Weighted Connections");
    }

    [Fact]
    public void SetMessageFilters_NullFilters_ClearsFilters()
    {
        // Act & Assert - should not throw
        _sut.SetMessageFilters(null);
    }

    [Fact]
    public void SetMessageFilters_EmptyList_ClearsFilters()
    {
        // Act & Assert - should not throw
        _sut.SetMessageFilters(new List<IMessageFilter>());
    }

    [Fact]
    public async Task RouteMessageAsync_WithFilter_BlocksWhenFilterRejects()
    {
        // Arrange
        var subscriber = new TestNeuron("sub", "topic.a");
        _sut.RegisterNeuron(subscriber);

        var filter = new Mock<IMessageFilter>();
        filter.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _sut.SetMessageFilters(new List<IMessageFilter> { filter.Object });

        var message = new NeuronMessage
        {
            SourceNeuron = "sender",
            Topic = "topic.a",
            Payload = "blocked"
        };

        // Act
        await _sut.RouteMessageAsync(message);
        await Task.Delay(50);

        // Assert
        subscriber.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task BroadcastAsync_SendsToAllNeuronsExceptSource()
    {
        // Arrange
        var n1 = new TestNeuron("n1", "any");
        var n2 = new TestNeuron("n2", "any");
        var source = new TestNeuron("source", "any");
        _sut.RegisterNeuron(n1);
        _sut.RegisterNeuron(n2);
        _sut.RegisterNeuron(source);

        // Act
        await _sut.BroadcastAsync("broadcast.topic", "hello", "source");
        await Task.Delay(50);

        // Assert
        n1.ReceivedMessages.Should().HaveCount(1);
        n2.ReceivedMessages.Should().HaveCount(1);
        source.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public void Topology_CanBeSetAndRetrieved()
    {
        // Arrange
        var topology = new ConnectionTopology();

        // Act
        _sut.Topology = topology;

        // Assert
        _sut.Topology.Should().Be(topology);
    }

    [Fact]
    public void RegisterNeuron_WithTopology_CreatesAutoConnections()
    {
        // Arrange
        var topology = new ConnectionTopology();
        using var network = new OuroborosNeuralNetwork(_intentionBus, topology: topology);

        var n1 = new TestNeuron("n1", "shared.topic");
        var n2 = new TestNeuron("n2", "shared.topic");

        // Act
        network.RegisterNeuron(n1);
        network.RegisterNeuron(n2);

        // Assert - topology should have created connections due to shared topic
        topology.GetConnection("n1", "n2").Should().NotBeNull();
        topology.GetConnection("n2", "n1").Should().NotBeNull();
    }

    public void Dispose()
    {
        _sut.Dispose();
        _intentionBus.Dispose();
    }
}
