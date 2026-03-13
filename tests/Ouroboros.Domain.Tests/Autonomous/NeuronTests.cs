namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class NeuronTests : IDisposable
{
    private readonly TestNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    /// <summary>
    /// Concrete test implementation of abstract Neuron.
    /// </summary>
    private sealed class TestNeuron : Neuron
    {
        public override string Id => "neuron.test";
        public override string Name => "Test Neuron";
        public override NeuronType Type => NeuronType.Custom;
        public override IReadOnlySet<string> SubscribedTopics => new HashSet<string> { "test.*", "ping" };

        public List<NeuronMessage> ReceivedMessages { get; } = [];
        public bool OnStartedCalled { get; private set; }
        public bool OnStoppedCalled { get; private set; }
        public int TickCount { get; private set; }

        protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
        {
            ReceivedMessages.Add(message);
            return Task.CompletedTask;
        }

        protected override void OnStarted() => OnStartedCalled = true;
        protected override void OnStopped() => OnStoppedCalled = true;

        protected override Task OnTickAsync(CancellationToken ct)
        {
            TickCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void AbstractProperties_ReturnExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.test");
        _neuron.Name.Should().Be("Test Neuron");
        _neuron.Type.Should().Be(NeuronType.Custom);
        _neuron.SubscribedTopics.Should().Contain("test.*");
        _neuron.SubscribedTopics.Should().Contain("ping");
    }

    [Fact]
    public void InitialState_IsNotActive()
    {
        _neuron.IsActive.Should().BeFalse();
        _neuron.Network.Should().BeNull();
        _neuron.IntentionBus.Should().BeNull();
    }

    [Fact]
    public void Start_SetsIsActive_CallsOnStarted()
    {
        _neuron.Start();

        _neuron.IsActive.Should().BeTrue();
        _neuron.OnStartedCalled.Should().BeTrue();
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        _neuron.Start();
        var act = () => _neuron.Start();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_SetsInactive_CallsOnStopped()
    {
        _neuron.Start();
        await _neuron.StopAsync();

        _neuron.IsActive.Should().BeFalse();
        _neuron.OnStoppedCalled.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WhenNotActive_DoesNotThrow()
    {
        await _neuron.StopAsync();
        _neuron.OnStoppedCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ReceiveMessage_WhenStarted_ProcessesMessage()
    {
        _neuron.Start();

        var message = new NeuronMessage
        {
            SourceNeuron = "other",
            Topic = "test.hello",
            Payload = "data"
        };

        _neuron.ReceiveMessage(message);

        // Allow time for message processing
        await Task.Delay(200);

        _neuron.ReceivedMessages.Should().ContainSingle();
        _neuron.ReceivedMessages[0].Topic.Should().Be("test.hello");
    }

    [Fact]
    public void RegisterNeuron_SetsNetworkAndBusReferences()
    {
        using var bus = new IntentionBus();
        using var network = new OuroborosNeuralNetwork(bus);

        network.RegisterNeuron(_neuron);

        _neuron.Network.Should().BeSameAs(network);
        _neuron.IntentionBus.Should().BeSameAs(bus);
    }

    [Fact]
    public void Dispose_SetsInactive()
    {
        _neuron.Start();
        _neuron.Dispose();

        _neuron.IsActive.Should().BeFalse();
    }
}
