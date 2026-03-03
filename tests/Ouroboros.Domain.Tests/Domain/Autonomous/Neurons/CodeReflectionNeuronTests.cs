using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class CodeReflectionNeuronTests : IDisposable
{
    private readonly CodeReflectionNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.code");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("Code Reflection");
    }

    [Fact]
    public void Type_Returns_CodeReflection()
    {
        _sut.Type.Should().Be(NeuronType.CodeReflection);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected()
    {
        _sut.SubscribedTopics.Should().Contain("code.*");
        _sut.SubscribedTopics.Should().Contain("self.modify");
        _sut.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void CodeScanIntervalSeconds_Default_300()
    {
        _sut.CodeScanIntervalSeconds.Should().Be(300);
    }

    [Fact]
    public void CodeScanIntervalSeconds_CanBeSet()
    {
        _sut.CodeScanIntervalSeconds = 600;
        _sut.CodeScanIntervalSeconds.Should().Be(600);
    }

    [Fact]
    public void Start_Activates_Neuron()
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
    public void ReceiveMessage_CodeAnalyze_EmptyPath_Processed()
    {
        _sut.Start();

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "code.analyze",
            Payload = "",
        };
        _sut.ReceiveMessage(message);
        Thread.Sleep(300);

        _sut.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
