// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;
using Xunit;

/// <summary>
/// Tests for AutonomousCoordinator.Training.cs — auto-training lifecycle,
/// user persona management, and InferDeliverableType.
/// </summary>
[Trait("Category", "Unit")]
public class AutonomousCoordinatorTrainingTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;
    private readonly List<ProactiveMessageEventArgs> _capturedMessages = new();

    public AutonomousCoordinatorTrainingTests()
    {
        _sut = new AutonomousCoordinator(new AutonomousConfiguration
        {
            PushBasedMode = true,
            YoloMode = false,
        });
        _sut.OnProactiveMessage += args => _capturedMessages.Add(args);
    }

    public void Dispose() => _sut.Dispose();

    // ----------------------------------------------------------------
    // StartAutoTraining
    // ----------------------------------------------------------------

    [Fact]
    public void StartAutoTraining_DefaultConfig_SetsActive()
    {
        _sut.StartAutoTraining();

        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void StartAutoTraining_NullConfig_UsesDefaults()
    {
        _sut.StartAutoTraining(null);

        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void StartAutoTraining_WithYoloConfig_EnablesYoloMode()
    {
        _sut.StartAutoTraining(new UserPersonaConfig { YoloMode = true });

        _sut.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void StartAutoTraining_ProblemSolvingMode_SuppressesMessages()
    {
        bool? suppressed = null;
        _sut.SetSuppressProactiveMessages = v => suppressed = v;

        _sut.StartAutoTraining(new UserPersonaConfig
        {
            ProblemSolvingMode = true,
            Problem = "Test problem"
        });

        suppressed.Should().BeTrue();
    }

    [Fact]
    public void StartAutoTraining_ProblemSolvingMode_RaisesProblemSolvingMessage()
    {
        _sut.StartAutoTraining(new UserPersonaConfig
        {
            ProblemSolvingMode = true,
            Problem = "Build a REST API",
            DeliverableType = "code",
            UseTools = true,
        });

        _capturedMessages.Should().Contain(m => m.Message.Contains("Problem-Solving Mode"));
        _capturedMessages.Should().Contain(m => m.Message.Contains("Build a REST API"));
    }

    [Fact]
    public void StartAutoTraining_SelfDialogueMode_RaisesSelfDialogueMessage()
    {
        _sut.StartAutoTraining(new UserPersonaConfig
        {
            SelfDialogueMode = true,
            Name = "Ouroboros-B",
        });

        _capturedMessages.Should().Contain(m => m.Message.Contains("Self-Dialogue Mode"));
    }

    [Fact]
    public void StartAutoTraining_NormalMode_RaisesAutoTrainingMessage()
    {
        _sut.StartAutoTraining(new UserPersonaConfig
        {
            Name = "TestUser",
        });

        _capturedMessages.Should().Contain(m => m.Message.Contains("Auto-Training Mode"));
    }

    [Fact]
    public void StartAutoTraining_CalledTwice_DoesNotDuplicate()
    {
        _sut.StartAutoTraining();
        _sut.StartAutoTraining();

        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // StopAutoTraining
    // ----------------------------------------------------------------

    [Fact]
    public void StopAutoTraining_ClearsActiveFlag()
    {
        _sut.StartAutoTraining();
        _sut.StopAutoTraining();

        _sut.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void StopAutoTraining_RaisesStoppedMessage()
    {
        _sut.StartAutoTraining();
        _capturedMessages.Clear();

        _sut.StopAutoTraining();

        _capturedMessages.Should().Contain(m => m.Message.Contains("Stopped"));
    }

    [Fact]
    public async Task StopAutoTraining_WithYoloFromTraining_RestoresConfigDefault()
    {
        _sut.StartAutoTraining(new UserPersonaConfig { YoloMode = true });
        _sut.IsYoloMode.Should().BeTrue();

        // Allow background tasks to configure neuron
        await Task.Delay(200);

        _sut.StopAutoTraining();

        _sut.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public async Task StopAutoTraining_ProblemSolving_ReenablesProactiveMessages()
    {
        bool? suppressed = null;
        _sut.SetSuppressProactiveMessages = v => suppressed = v;

        _sut.StartAutoTraining(new UserPersonaConfig
        {
            ProblemSolvingMode = true,
            Problem = "test"
        });

        await Task.Delay(200);
        _sut.StopAutoTraining();

        suppressed.Should().BeFalse();
    }

    [Fact]
    public void StopAutoTraining_WhenNotStarted_DoesNotThrow()
    {
        Action act = () => _sut.StopAutoTraining();

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // GetAutoTrainingStats
    // ----------------------------------------------------------------

    [Fact]
    public void GetAutoTrainingStats_BeforeStart_ReturnsNull()
    {
        _sut.GetAutoTrainingStats().Should().BeNull();
    }

    [Fact]
    public void GetAutoTrainingStats_AfterStart_ReturnsStats()
    {
        _sut.StartAutoTraining();

        var stats = _sut.GetAutoTrainingStats();

        stats.Should().NotBeNull();
        stats!.Value.TotalInteractions.Should().Be(0);
        stats.Value.AverageSatisfaction.Should().Be(0);
        stats.Value.SessionMessages.Should().Be(0);
    }

    // ----------------------------------------------------------------
    // ConfigureAutoTraining
    // ----------------------------------------------------------------

    [Fact]
    public void ConfigureAutoTraining_DoesNotThrow()
    {
        Action act = () => _sut.ConfigureAutoTraining(new UserPersonaConfig { Name = "TestBot" });

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // InferDeliverableTypeFallback — keyword-based inference
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("Design a system architecture", "design")]
    [InlineData("Plan the deployment strategy", "plan")]
    [InlineData("Analyze the performance bottleneck", "analysis")]
    [InlineData("Document the API usage", "document")]
    [InlineData("Build a web scraper", "code")]
    [InlineData("Fix the login bug", "code")]
    public void InferDeliverableTypeFallback_CorrectlyClassifies(string problem, string expected)
    {
        MethodInfo? method = typeof(AutonomousCoordinator)
            .GetMethod("InferDeliverableTypeFallback", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        string result = (string)method!.Invoke(null, new object[] { problem })!;
        result.Should().Be(expected);
    }
}
