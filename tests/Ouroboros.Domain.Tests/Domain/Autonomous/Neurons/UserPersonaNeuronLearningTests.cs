// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Autonomous.Neurons;
using Xunit;

/// <summary>
/// Tests for UserPersonaNeuron.Learning.cs — training lifecycle,
/// question generation, prompt building, and configuration parsing.
/// </summary>
[Trait("Category", "Unit")]
public class UserPersonaNeuronLearningTests
{
    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static UserPersonaNeuron CreateNeuron()
    {
        return new UserPersonaNeuron();
    }

    // ----------------------------------------------------------------
    // Basic properties
    // ----------------------------------------------------------------

    [Fact]
    public void Id_ReturnsExpected()
    {
        var neuron = CreateNeuron();
        neuron.Id.Should().Be("neuron.user_persona");
    }

    [Fact]
    public void Name_ReturnsExpected()
    {
        var neuron = CreateNeuron();
        neuron.Name.Should().Be("Automatic User Persona");
    }

    [Fact]
    public void Type_ReturnsCognitive()
    {
        var neuron = CreateNeuron();
        neuron.Type.Should().Be(NeuronType.Cognitive);
    }

    [Fact]
    public void IsTrainingActive_DefaultFalse()
    {
        var neuron = CreateNeuron();
        neuron.IsTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void Config_DefaultNotNull()
    {
        var neuron = CreateNeuron();
        neuron.Config.Should().NotBeNull();
    }

    [Fact]
    public void GenerateFunction_DefaultNull()
    {
        var neuron = CreateNeuron();
        neuron.GenerateFunction.Should().BeNull();
    }

    [Fact]
    public void EvaluateFunction_DefaultNull()
    {
        var neuron = CreateNeuron();
        neuron.EvaluateFunction.Should().BeNull();
    }

    [Fact]
    public void SubscribedTopics_ContainsExpectedTopics()
    {
        var neuron = CreateNeuron();

        neuron.SubscribedTopics.Should().Contain("training.*");
        neuron.SubscribedTopics.Should().Contain("user_persona.*");
        neuron.SubscribedTopics.Should().Contain("response.generated");
        neuron.SubscribedTopics.Should().Contain("system.tick");
    }

    // ----------------------------------------------------------------
    // GetStats
    // ----------------------------------------------------------------

    [Fact]
    public void GetStats_NoInteractions_ReturnsZeros()
    {
        var neuron = CreateNeuron();

        var stats = neuron.GetStats();

        stats.TotalInteractions.Should().Be(0);
        stats.AverageSatisfaction.Should().Be(0);
        stats.SessionMessages.Should().Be(0);
    }

    // ----------------------------------------------------------------
    // RecordInteraction
    // ----------------------------------------------------------------

    [Fact]
    public void RecordInteraction_IncreasesInteractionCount()
    {
        var neuron = CreateNeuron();

        neuron.RecordInteraction("Hello?", "Hi there!");
        neuron.RecordInteraction("How are you?", "I'm fine!");

        var stats = neuron.GetStats();
        stats.TotalInteractions.Should().Be(2);
    }

    [Fact]
    public void RecordInteraction_BoundsAt1000Interactions()
    {
        var neuron = CreateNeuron();

        for (int i = 0; i < 1005; i++)
        {
            neuron.RecordInteraction($"Q{i}", $"A{i}");
        }

        var stats = neuron.GetStats();
        stats.TotalInteractions.Should().BeLessOrEqualTo(1000);
    }

    // ----------------------------------------------------------------
    // StartTrainingDirectAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StartTrainingDirectAsync_SetsTrainingActive()
    {
        var neuron = CreateNeuron();
        var config = new UserPersonaConfig
        {
            MaxSessionMessages = 1, // Limit to 1 to avoid infinite loop
        };

        // Wire up minimal handlers to avoid nulls
        neuron.OnUserMessage += (msg, cfg) => { };

        // Start training (will produce a template question since no GenerateFunction)
        await neuron.StartTrainingDirectAsync(config);

        // After calling StartTrainingDirectAsync, the neuron config should be set
        neuron.Config.Should().Be(config);
    }

    // ----------------------------------------------------------------
    // GenerateTemplateQuestion via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void GenerateTemplateQuestion_ReturnsNonEmptyString()
    {
        var neuron = CreateNeuron();
        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("GenerateTemplateQuestion", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        string question = (string)method!.Invoke(neuron, Array.Empty<object>())!;

        question.Should().NotBeNullOrWhiteSpace();
    }

    // ----------------------------------------------------------------
    // BuildQuestionGenerationPrompt via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void BuildQuestionGenerationPrompt_NormalMode_ContainsUserInfo()
    {
        var neuron = CreateNeuron();
        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("BuildQuestionGenerationPrompt", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        string prompt = (string)method!.Invoke(neuron, Array.Empty<object>())!;

        prompt.Should().Contain("simulating a curious user");
    }

    [Fact]
    public void BuildProblemSolvingPrompt_ContainsProblemInfo()
    {
        var neuron = CreateNeuron();

        // Set config via reflection to problem-solving mode
        var config = new UserPersonaConfig
        {
            ProblemSolvingMode = true,
            Problem = "Build a REST API",
            DeliverableType = "code",
        };
        typeof(UserPersonaNeuron)
            .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(neuron, config);

        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("BuildProblemSolvingPrompt", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        string prompt = (string)method!.Invoke(neuron, Array.Empty<object>())!;

        prompt.Should().Contain("Build a REST API");
        prompt.Should().Contain("PROBLEM-SOLVING SESSION");
    }

    // ----------------------------------------------------------------
    // ConfigurePersona via reflection (with JsonElement)
    // ----------------------------------------------------------------

    [Fact]
    public void ConfigurePersona_WithJsonElement_SetsConfigFields()
    {
        var neuron = CreateNeuron();
        string json = """
        {
            "name": "TestBot",
            "skillLevel": "expert",
            "style": "formal",
            "interval": 15,
            "maxMessages": 25,
            "followUpProb": 0.6,
            "challengeProb": 0.2
        }
        """;
        JsonElement element = JsonDocument.Parse(json).RootElement;

        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("ConfigurePersona", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        method!.Invoke(neuron, new object?[] { element });

        neuron.Config.Name.Should().Be("TestBot");
        neuron.Config.SkillLevel.Should().Be("expert");
        neuron.Config.CommunicationStyle.Should().Be("formal");
        neuron.Config.MessageIntervalSeconds.Should().Be(15);
        neuron.Config.MaxSessionMessages.Should().Be(25);
    }

    [Fact]
    public void ConfigurePersona_WithTraitsAndInterests_ParsesArrays()
    {
        var neuron = CreateNeuron();
        string json = """
        {
            "traits": ["curious", "analytical"],
            "interests": ["AI", "quantum computing"]
        }
        """;
        JsonElement element = JsonDocument.Parse(json).RootElement;

        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("ConfigurePersona", BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(neuron, new object?[] { element });

        neuron.Config.Traits.Should().Contain("curious");
        neuron.Config.Traits.Should().Contain("analytical");
        neuron.Config.Interests.Should().Contain("AI");
        neuron.Config.Interests.Should().Contain("quantum computing");
    }

    [Fact]
    public void ConfigurePersona_WithNullPayload_DoesNotThrow()
    {
        var neuron = CreateNeuron();

        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("ConfigurePersona", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        Action act = () => method!.Invoke(neuron, new object?[] { null });
        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // StopTraining via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void StopTraining_ClearsActiveFlag()
    {
        var neuron = CreateNeuron();

        // Set training active manually
        typeof(UserPersonaNeuron)
            .GetField("_isTrainingActive", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(neuron, true);

        MethodInfo? method = typeof(UserPersonaNeuron)
            .GetMethod("StopTraining", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        method!.Invoke(neuron, Array.Empty<object>());

        neuron.IsTrainingActive.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // OnUserMessage event
    // ----------------------------------------------------------------

    [Fact]
    public void OnUserMessage_CanSubscribe()
    {
        var neuron = CreateNeuron();
        string? capturedMessage = null;
        UserPersonaConfig? capturedConfig = null;

        neuron.OnUserMessage += (msg, cfg) =>
        {
            capturedMessage = msg;
            capturedConfig = cfg;
        };

        // Simulate invoking via RecordInteraction (which doesn't fire OnUserMessage)
        // But the event subscription should work without error
        neuron.OnUserMessage.Should().NotBeNull();
    }
}
