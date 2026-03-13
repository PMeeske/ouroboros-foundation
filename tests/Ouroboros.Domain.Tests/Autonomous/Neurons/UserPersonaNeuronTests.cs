namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class UserPersonaNeuronTests : IDisposable
{
    private readonly UserPersonaNeuron _neuron = new();

    public void Dispose() => _neuron.Dispose();

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        _neuron.Id.Should().Be("neuron.user_persona");
        _neuron.Name.Should().Be("Automatic User Persona");
        _neuron.Type.Should().Be(NeuronType.Cognitive);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _neuron.SubscribedTopics.Should().Contain("training.*");
        _neuron.SubscribedTopics.Should().Contain("user_persona.*");
        _neuron.SubscribedTopics.Should().Contain("response.generated");
        _neuron.SubscribedTopics.Should().Contain("system.tick");
    }

    [Fact]
    public void Config_HasDefaultValues()
    {
        _neuron.Config.Should().NotBeNull();
        _neuron.Config.Name.Should().Be("AutoUser");
    }

    [Fact]
    public void IsTrainingActive_DefaultIsFalse()
    {
        _neuron.IsTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void DelegateProperties_DefaultToNull()
    {
        _neuron.GenerateFunction.Should().BeNull();
        _neuron.EvaluateFunction.Should().BeNull();
        _neuron.ResearchFunction.Should().BeNull();
    }

    [Fact]
    public void GetStats_InitialState_ReturnsZeros()
    {
        var stats = _neuron.GetStats();

        stats.TotalInteractions.Should().Be(0);
        stats.AverageSatisfaction.Should().Be(0);
        stats.SessionMessages.Should().Be(0);
    }

    [Fact]
    public void RecordInteraction_TracksInteraction()
    {
        _neuron.RecordInteraction("Hello", "Hi there!");

        var stats = _neuron.GetStats();
        stats.TotalInteractions.Should().Be(1);
    }

    [Fact]
    public void RecordInteraction_MultipleInteractions_TracksAll()
    {
        _neuron.RecordInteraction("Q1", "A1");
        _neuron.RecordInteraction("Q2", "A2");
        _neuron.RecordInteraction("Q3", "A3");

        var stats = _neuron.GetStats();
        stats.TotalInteractions.Should().Be(3);
    }

    [Fact]
    public async Task StartTrainingDirectAsync_ActivatesTraining()
    {
        _neuron.GenerateFunction = (_, _) => Task.FromResult("Test question?");

        // Wire OnUserMessage to prevent null warnings
        _neuron.OnUserMessage += (_, _) => { };

        await _neuron.StartTrainingDirectAsync(new UserPersonaConfig
        {
            MaxSessionMessages = 1,
            MessageIntervalSeconds = 60
        });

        _neuron.IsTrainingActive.Should().BeTrue();
        _neuron.Config.MaxSessionMessages.Should().Be(1);
    }

    [Fact]
    public async Task StartTrainingDirectAsync_WithoutGenerateFunction_UsesTemplates()
    {
        string? sentMessage = null;
        _neuron.OnUserMessage += (msg, _) => sentMessage = msg;

        await _neuron.StartTrainingDirectAsync(new UserPersonaConfig
        {
            MaxSessionMessages = 1,
            MessageIntervalSeconds = 60
        });

        // Template-based generation should have produced a message
        await Task.Delay(500);
        sentMessage.Should().NotBeNullOrEmpty();
    }
}
