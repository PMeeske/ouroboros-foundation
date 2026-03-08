using System.Text.Json;
using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public sealed class AffectNeuronTests : IDisposable
{
    private readonly AffectNeuron _sut = new();

    [Fact]
    public void Id_Returns_Expected()
    {
        _sut.Id.Should().Be("neuron.affect");
    }

    [Fact]
    public void Name_Returns_Expected()
    {
        _sut.Name.Should().Be("Affective State");
    }

    [Fact]
    public void Type_Returns_Affect()
    {
        _sut.Type.Should().Be(NeuronType.Affect);
    }

    [Fact]
    public void SubscribedTopics_Contains_Expected_Topics()
    {
        _sut.SubscribedTopics.Should().Contain("emotion.*");
        _sut.SubscribedTopics.Should().Contain("affect.*");
        _sut.SubscribedTopics.Should().Contain("mood.*");
        _sut.SubscribedTopics.Should().Contain("reflection.request");
    }

    [Fact]
    public void Initial_Arousal_Is_Zero()
    {
        _sut.Arousal.Should().Be(0.0);
    }

    [Fact]
    public void Initial_Valence_Is_Zero()
    {
        _sut.Valence.Should().Be(0.0);
    }

    [Fact]
    public void Initial_DominantEmotion_Is_Neutral()
    {
        _sut.DominantEmotion.Should().Be("neutral");
    }

    [Fact]
    public void ReceiveMessage_AffectPositive_Increases_Valence()
    {
        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "affect.positive",
            Payload = "boost",
        };

        _sut.ReceiveMessage(message);
        _sut.Start();
        Thread.Sleep(200);

        _sut.Valence.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void ReceiveMessage_AffectNegative_Decreases_Valence()
    {
        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "affect.negative",
            Payload = "drop",
        };

        _sut.ReceiveMessage(message);
        _sut.Start();
        Thread.Sleep(200);

        _sut.Valence.Should().BeLessThan(0.0);
    }

    [Fact]
    public void ReceiveMessage_EmotionUpdate_Updates_From_JsonPayload()
    {
        JsonElement payload = JsonSerializer.Deserialize<JsonElement>(
            """{"arousal": 0.8, "valence": -0.5}""");

        NeuronMessage message = new()
        {
            SourceNeuron = "test",
            Topic = "emotion.update",
            Payload = payload,
        };

        _sut.ReceiveMessage(message);
        _sut.Start();
        Thread.Sleep(200);

        _sut.Arousal.Should().BeApproximately(0.8, 0.05);
        _sut.Valence.Should().BeApproximately(-0.5, 0.05);
    }

    [Fact]
    public void IsActive_False_Before_Start()
    {
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_True_After_Start()
    {
        _sut.Start();
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_Sets_IsActive_False()
    {
        _sut.Start();
        await _sut.StopAsync();
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Start_Multiple_Times_Is_Idempotent()
    {
        _sut.Start();
        _sut.Start();
        _sut.IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
