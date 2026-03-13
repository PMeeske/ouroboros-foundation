namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class OuroborosNeuralNetworkTests : IDisposable
{
    private readonly IntentionBus _bus;
    private readonly OuroborosNeuralNetwork _network;

    public OuroborosNeuralNetworkTests()
    {
        _bus = new IntentionBus();
        _network = new OuroborosNeuralNetwork(_bus);
    }

    public void Dispose()
    {
        _network.Dispose();
    }

    private sealed class StubNeuron : Neuron
    {
        public StubNeuron(string id, IReadOnlySet<string>? topics = null)
        {
            _id = id;
            _topics = topics ?? new HashSet<string> { "test.*" };
        }

        private readonly string _id;
        private readonly IReadOnlySet<string> _topics;

        public override string Id => _id;
        public override string Name => $"Stub {_id}";
        public override NeuronType Type => NeuronType.Custom;
        public override IReadOnlySet<string> SubscribedTopics => _topics;

        public List<NeuronMessage> ReceivedMessages { get; } = [];

        protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
        {
            ReceivedMessages.Add(message);
            return Task.CompletedTask;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // Construction
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_InitializesWithEmptyState()
    {
        _network.Neurons.Should().BeEmpty();
        _network.IsActive.Should().BeFalse();
        _network.IntentionBus.Should().BeSameAs(_bus);
    }

    [Fact]
    public void Constructor_WithTopology_SetsTopology()
    {
        var topology = new ConnectionTopology();
        using var bus = new IntentionBus();
        using var net = new OuroborosNeuralNetwork(bus, topology: topology);

        net.Topology.Should().BeSameAs(topology);
    }

    // ═══════════════════════════════════════════════════════════════
    // RegisterNeuron
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RegisterNeuron_AddsNeuron()
    {
        using var neuron = new StubNeuron("n1");
        _network.RegisterNeuron(neuron);

        _network.Neurons.Should().HaveCount(1);
        _network.Neurons.Should().ContainKey("n1");
    }

    [Fact]
    public void RegisterNeuron_SetsNetworkAndBusReferences()
    {
        using var neuron = new StubNeuron("n1");
        _network.RegisterNeuron(neuron);

        neuron.Network.Should().BeSameAs(_network);
        neuron.IntentionBus.Should().BeSameAs(_bus);
    }

    [Fact]
    public void RegisterNeuron_WithTopology_CreatesConnectionsForSharedTopics()
    {
        var topology = new ConnectionTopology();
        using var bus = new IntentionBus();
        using var net = new OuroborosNeuralNetwork(bus, topology: topology);

        var sharedTopics = new HashSet<string> { "shared.topic" };
        using var n1 = new StubNeuron("n1", sharedTopics);
        using var n2 = new StubNeuron("n2", sharedTopics);

        net.RegisterNeuron(n1);
        net.RegisterNeuron(n2);

        topology.GetConnection("n1", "n2").Should().NotBeNull();
        topology.GetConnection("n2", "n1").Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // UnregisterNeuronAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task UnregisterNeuronAsync_RemovesNeuron()
    {
        using var neuron = new StubNeuron("n1");
        _network.RegisterNeuron(neuron);

        await _network.UnregisterNeuronAsync("n1");

        _network.Neurons.Should().BeEmpty();
    }

    [Fact]
    public async Task UnregisterNeuronAsync_NonExistent_DoesNotThrow()
    {
        var act = () => _network.UnregisterNeuronAsync("nonexistent");
        await act.Should().NotThrowAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // Start / Stop
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Start_SetsIsActive()
    {
        _network.Start();
        _network.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_ActivatesAllNeurons()
    {
        using var neuron = new StubNeuron("n1");
        _network.RegisterNeuron(neuron);

        _network.Start();

        neuron.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        _network.Start();
        var act = () => _network.Start();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_SetsInactive()
    {
        _network.Start();
        await _network.StopAsync();

        _network.IsActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // RouteMessageAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RouteMessageAsync_TargetedMessage_RoutesToTarget()
    {
        using var target = new StubNeuron("n1");
        _network.RegisterNeuron(target);
        target.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "external",
            TargetNeuron = "n1",
            Topic = "test.msg",
            Payload = "data"
        };

        await _network.RouteMessageAsync(message);
        await Task.Delay(100);

        target.ReceivedMessages.Should().ContainSingle();
    }

    [Fact]
    public async Task RouteMessageAsync_TopicMessage_RoutesToSubscribers()
    {
        using var subscriber = new StubNeuron("sub", new HashSet<string> { "events.new" });
        _network.RegisterNeuron(subscriber);
        subscriber.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "external",
            Topic = "events.new",
            Payload = "event data"
        };

        await _network.RouteMessageAsync(message);
        await Task.Delay(100);

        subscriber.ReceivedMessages.Should().ContainSingle();
    }

    [Fact]
    public async Task RouteMessageAsync_NullMessage_ThrowsArgumentNull()
    {
        var act = () => _network.RouteMessageAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RouteMessageAsync_SameSourceNeuron_DoesNotRouteBack()
    {
        using var neuron = new StubNeuron("n1", new HashSet<string> { "test.msg" });
        _network.RegisterNeuron(neuron);
        neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "n1",
            Topic = "test.msg",
            Payload = "data"
        };

        await _network.RouteMessageAsync(message);
        await Task.Delay(100);

        neuron.ReceivedMessages.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // BroadcastAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task BroadcastAsync_RoutesToAllExceptSource()
    {
        using var n1 = new StubNeuron("n1");
        using var n2 = new StubNeuron("n2");
        _network.RegisterNeuron(n1);
        _network.RegisterNeuron(n2);
        n1.Start();
        n2.Start();

        await _network.BroadcastAsync("system.tick", new { Time = DateTime.UtcNow }, "n1");
        await Task.Delay(100);

        n2.ReceivedMessages.Should().ContainSingle();
        n1.ReceivedMessages.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // Message Filters
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task SetMessageFilters_BlockingFilter_PreventsRouting()
    {
        var mockFilter = new Mock<IMessageFilter>();
        mockFilter.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _network.SetMessageFilters(new List<IMessageFilter> { mockFilter.Object });

        using var subscriber = new StubNeuron("n1", new HashSet<string> { "blocked.topic" });
        _network.RegisterNeuron(subscriber);
        subscriber.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "external",
            Topic = "blocked.topic",
            Payload = "data"
        };

        await _network.RouteMessageAsync(message);
        await Task.Delay(100);

        subscriber.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task SetMessageFilters_AllowingFilter_PermitsRouting()
    {
        var mockFilter = new Mock<IMessageFilter>();
        mockFilter.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _network.SetMessageFilters(new List<IMessageFilter> { mockFilter.Object });

        using var subscriber = new StubNeuron("n1", new HashSet<string> { "allowed.topic" });
        _network.RegisterNeuron(subscriber);
        subscriber.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "external",
            Topic = "allowed.topic",
            Payload = "data"
        };

        await _network.RouteMessageAsync(message);
        await Task.Delay(100);

        subscriber.ReceivedMessages.Should().ContainSingle();
    }

    [Fact]
    public void SetMessageFilters_NullOrEmpty_ClearsFilters()
    {
        _network.SetMessageFilters(null);
        // Should not throw
        _network.SetMessageFilters(new List<IMessageFilter>());
    }

    // ═══════════════════════════════════════════════════════════════
    // Queries
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetNeuron_ExistingId_ReturnsNeuron()
    {
        using var neuron = new StubNeuron("n1");
        _network.RegisterNeuron(neuron);

        _network.GetNeuron("n1").Should().BeSameAs(neuron);
    }

    [Fact]
    public void GetNeuron_NonExistentId_ReturnsNull()
    {
        _network.GetNeuron("nonexistent").Should().BeNull();
    }

    [Fact]
    public void GetNeuronsByType_ReturnsMatchingNeurons()
    {
        using var sn1 = new StubNeuron("n1");
        using var sn2 = new StubNeuron("n2");
        _network.RegisterNeuron(sn1);
        _network.RegisterNeuron(sn2);

        _network.GetNeuronsByType(NeuronType.Custom).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentMessages_ReturnsHistory()
    {
        var message = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "topic",
            Payload = "data"
        };

        await _network.RouteMessageAsync(message);

        var recent = _network.GetRecentMessages(10);
        recent.Should().HaveCount(1);
    }

    [Fact]
    public void GetNetworkState_ContainsStatusInfo()
    {
        using var sn = new StubNeuron("n1");
        _network.RegisterNeuron(sn);

        string state = _network.GetNetworkState();
        state.Should().Contain("Ouroboros Neural Network");
        state.Should().Contain("Neurons:");
    }

    // ═══════════════════════════════════════════════════════════════
    // Weighted Routing
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RouteMessageAsync_WithTopology_StrongInhibition_SuppressesMessage()
    {
        var topology = new ConnectionTopology();
        using var bus = new IntentionBus();
        using var net = new OuroborosNeuralNetwork(bus, topology: topology);

        using var subscriber = new StubNeuron("sub", new HashSet<string> { "test.topic" });
        net.RegisterNeuron(subscriber);
        subscriber.Start();

        topology.SetConnection("src", "sub", -0.9); // Strong inhibition

        var message = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "test.topic",
            Payload = "data"
        };

        await net.RouteMessageAsync(message);
        await Task.Delay(100);

        subscriber.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task RouteMessageAsync_WithTopology_HighWeight_BoostsPriority()
    {
        var topology = new ConnectionTopology();
        using var bus = new IntentionBus();
        using var net = new OuroborosNeuralNetwork(bus, topology: topology);

        using var subscriber = new StubNeuron("sub", new HashSet<string> { "test.topic" });
        net.RegisterNeuron(subscriber);
        subscriber.Start();

        topology.SetConnection("src", "sub", 0.9); // High excitatory

        var message = new NeuronMessage
        {
            SourceNeuron = "src",
            Topic = "test.topic",
            Payload = "data",
            Priority = IntentionPriority.Normal
        };

        await net.RouteMessageAsync(message);
        await Task.Delay(100);

        subscriber.ReceivedMessages.Should().ContainSingle();
        subscriber.ReceivedMessages[0].Priority.Should().Be(IntentionPriority.High);
    }
}
