// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// Tests for <see cref="UserPersonaNeuron"/> and its Learning partial class.
/// </summary>
[Trait("Category", "Unit")]
public class UserPersonaNeuronTests
{
    private readonly UserPersonaNeuron _sut;

    public UserPersonaNeuronTests()
    {
        _sut = new UserPersonaNeuron();
    }

    // ----------------------------------------------------------------
    // Identity properties
    // ----------------------------------------------------------------

    [Fact]
    public void Id_ReturnsExpectedValue()
    {
        _sut.Id.Should().Be("neuron.user_persona");
    }

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        _sut.Name.Should().Be("Automatic User Persona");
    }

    [Fact]
    public void Type_IsCognitive()
    {
        _sut.Type.Should().Be(NeuronType.Cognitive);
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        _sut.SubscribedTopics.Should().Contain("training.*");
        _sut.SubscribedTopics.Should().Contain("user_persona.*");
        _sut.SubscribedTopics.Should().Contain("response.generated");
        _sut.SubscribedTopics.Should().Contain("system.tick");
    }

    // ----------------------------------------------------------------
    // Config
    // ----------------------------------------------------------------

    [Fact]
    public void Config_DefaultConfig_IsNotNull()
    {
        _sut.Config.Should().NotBeNull();
    }

    [Fact]
    public void Config_DefaultConfig_HasDefaultName()
    {
        _sut.Config.Name.Should().NotBeNullOrWhiteSpace();
    }

    // ----------------------------------------------------------------
    // IsTrainingActive
    // ----------------------------------------------------------------

    [Fact]
    public void IsTrainingActive_Initially_IsFalse()
    {
        _sut.IsTrainingActive.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // GetStats
    // ----------------------------------------------------------------

    [Fact]
    public void GetStats_NoInteractions_ReturnsZeros()
    {
        // Act
        var stats = _sut.GetStats();

        // Assert
        stats.TotalInteractions.Should().Be(0);
        stats.AverageSatisfaction.Should().Be(0);
        stats.SessionMessages.Should().Be(0);
    }

    // ----------------------------------------------------------------
    // RecordInteraction
    // ----------------------------------------------------------------

    [Fact]
    public void RecordInteraction_IncrementsInteractionCount()
    {
        // Act
        _sut.RecordInteraction("Hello", "Hi there!");

        // Assert
        var stats = _sut.GetStats();
        stats.TotalInteractions.Should().Be(1);
    }

    [Fact]
    public void RecordInteraction_MultipleInteractions_TracksAll()
    {
        // Act
        _sut.RecordInteraction("Q1", "A1");
        _sut.RecordInteraction("Q2", "A2");
        _sut.RecordInteraction("Q3", "A3");

        // Assert
        var stats = _sut.GetStats();
        stats.TotalInteractions.Should().Be(3);
    }

    [Fact]
    public void RecordInteraction_InteractionsBounded_At1000()
    {
        // Act - add more than 1000 interactions
        for (int i = 0; i < 1010; i++)
        {
            _sut.RecordInteraction($"Q{i}", $"A{i}");
        }

        // Assert - should be bounded at 1000
        var stats = _sut.GetStats();
        stats.TotalInteractions.Should().BeLessThanOrEqualTo(1000);
    }

    // ----------------------------------------------------------------
    // Delegate properties
    // ----------------------------------------------------------------

    [Fact]
    public void GenerateFunction_DefaultNull()
    {
        _sut.GenerateFunction.Should().BeNull();
    }

    [Fact]
    public void GenerateFunction_CanBeSet()
    {
        // Arrange
        Func<string, CancellationToken, Task<string>> func = (_, _) => Task.FromResult("test");

        // Act
        _sut.GenerateFunction = func;

        // Assert
        _sut.GenerateFunction.Should().BeSameAs(func);
    }

    [Fact]
    public void EvaluateFunction_DefaultNull()
    {
        _sut.EvaluateFunction.Should().BeNull();
    }

    [Fact]
    public void EvaluateFunction_CanBeSet()
    {
        // Arrange
        Func<string, string, CancellationToken, Task<double>> func = (_, _, _) => Task.FromResult(0.8);

        // Act
        _sut.EvaluateFunction = func;

        // Assert
        _sut.EvaluateFunction.Should().BeSameAs(func);
    }

    [Fact]
    public void ResearchFunction_DefaultNull()
    {
        _sut.ResearchFunction.Should().BeNull();
    }

    // ----------------------------------------------------------------
    // OnUserMessage event
    // ----------------------------------------------------------------

    [Fact]
    public void OnUserMessage_CanSubscribe()
    {
        // Arrange & Act
        bool eventFired = false;
        _sut.OnUserMessage += (msg, config) => eventFired = true;

        // Assert
        eventFired.Should().BeFalse(); // Not fired yet, just subscribed
    }

    // ----------------------------------------------------------------
    // StartTrainingDirectAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StartTrainingDirectAsync_SetsConfig()
    {
        // Arrange
        var config = new UserPersonaConfig { Name = "TestUser", MaxSessionMessages = 5 };

        // Act
        _ = Task.Run(async () =>
        {
            try { await _sut.StartTrainingDirectAsync(config); }
            catch (Exception) { /* expected - no generate function */ }
        });

        // Allow time for config to be set
        await Task.Delay(100);

        // Assert
        _sut.Config.Name.Should().Be("TestUser");
    }
}
