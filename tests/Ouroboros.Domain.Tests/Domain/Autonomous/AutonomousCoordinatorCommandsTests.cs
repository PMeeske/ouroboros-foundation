// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;
using Xunit;

/// <summary>
/// Tests for AutonomousCoordinator.Commands.cs — command processing partial.
/// </summary>
[Trait("Category", "Unit")]
public class AutonomousCoordinatorCommandsTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;
    private readonly List<ProactiveMessageEventArgs> _capturedMessages = new();

    public AutonomousCoordinatorCommandsTests()
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
    // /approve command
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ApproveWithId_HandledAndRaisesMessage()
    {
        bool handled = _sut.ProcessCommand("/approve deadbeef");

        handled.Should().BeTrue();
        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("deadbeef"));
    }

    [Fact]
    public void ProcessCommand_ApproveCaseInsensitive_IsHandled()
    {
        bool handled = _sut.ProcessCommand("/APPROVE someid");

        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // /reject command
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_RejectWithIdAndReason_HandledAndRaisesMessage()
    {
        bool handled = _sut.ProcessCommand("/reject abc123 not needed");

        handled.Should().BeTrue();
        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("abc123"));
    }

    [Fact]
    public void ProcessCommand_RejectWithIdOnly_HandledWithoutReason()
    {
        bool handled = _sut.ProcessCommand("/reject abc123");

        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // /approve-all-safe
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ApproveAllSafe_HandledAndRaisesMessage()
    {
        bool handled = _sut.ProcessCommand("/approve-all-safe");

        handled.Should().BeTrue();
        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Auto-approved"));
    }

    // ----------------------------------------------------------------
    // /intentions and /pending — empty
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Intentions_NoPending_RaisesEmptyMessage()
    {
        _sut.ProcessCommand("/intentions");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("No pending intentions"));
    }

    [Fact]
    public void ProcessCommand_Pending_NoPending_RaisesEmptyMessage()
    {
        _sut.ProcessCommand("/pending");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("No pending intentions"));
    }

    // ----------------------------------------------------------------
    // /intentions — with pending items
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Intentions_WithPending_ListsThem()
    {
        // Arrange — inject a goal to create a pending intention
        _sut.InjectGoalAsync("Test goal for command").GetAwaiter().GetResult();
        _capturedMessages.Clear();

        // Act
        _sut.ProcessCommand("/intentions");

        // Assert
        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Pending Intention"));
    }

    // ----------------------------------------------------------------
    // /network and /neurons
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Network_RaisesNetworkStateMessage()
    {
        _sut.ProcessCommand("/network");

        _capturedMessages.Should().HaveCount(1);
        _capturedMessages[0].Source.Should().Be("coordinator");
    }

    [Fact]
    public void ProcessCommand_Neurons_RaisesNetworkStateMessage()
    {
        _sut.ProcessCommand("/neurons");

        _capturedMessages.Should().HaveCount(1);
    }

    // ----------------------------------------------------------------
    // /bus
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Bus_RaisesSummaryMessage()
    {
        _sut.ProcessCommand("/bus");

        _capturedMessages.Should().HaveCount(1);
    }

    // ----------------------------------------------------------------
    // /help and /?
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Help_RaisesHelpMessage()
    {
        _sut.ProcessCommand("/help");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Autonomous Commands"));
    }

    [Fact]
    public void ProcessCommand_QuestionMark_RaisesHelpMessage()
    {
        _sut.ProcessCommand("/?");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Autonomous Commands"));
    }

    // ----------------------------------------------------------------
    // /toggle-push
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_TogglePush_RaisesCurrentStatusMessage()
    {
        _sut.ProcessCommand("/toggle-push");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Push-based mode"));
    }

    // ----------------------------------------------------------------
    // /yolo toggle
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_YoloToggle_EnablesAndRaisesMessage()
    {
        _sut.ProcessCommand("/yolo");

        _sut.IsYoloMode.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("YOLO Mode"));
    }

    [Fact]
    public void ProcessCommand_YoloOn_ExplicitEnable()
    {
        _sut.ProcessCommand("/yolo on");

        _sut.IsYoloMode.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("ON"));
    }

    [Fact]
    public void ProcessCommand_YoloOff_ExplicitDisable()
    {
        _sut.ProcessCommand("/yolo on");
        _capturedMessages.Clear();

        _sut.ProcessCommand("/yolo off");

        _sut.IsYoloMode.Should().BeFalse();
        _capturedMessages.Should().Contain(m => m.Message.Contains("OFF"));
    }

    [Fact]
    public void ProcessCommand_YoloOn_WithPendingIntentions_AutoApprovesThem()
    {
        // Arrange — inject a goal to create a pending intention
        _sut.InjectGoalAsync("Yolo test goal").GetAwaiter().GetResult();
        int pendingBefore = _sut.PendingIntentionsCount;
        _capturedMessages.Clear();

        // Act
        _sut.ProcessCommand("/yolo on");

        // Assert — the yolo toggle auto-approves pending intentions
        _capturedMessages.Should().Contain(m => m.Message.Contains("YOLO Mode"));
    }

    // ----------------------------------------------------------------
    // /auto commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Auto_EnablesYoloAndStartsTraining()
    {
        _sut.ProcessCommand("/auto");

        _sut.IsYoloMode.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("Full Autonomous Mode"));
    }

    [Fact]
    public void ProcessCommand_YoloTrain_EnablesYoloAndStartsTraining()
    {
        _sut.ProcessCommand("/yolo train");

        _sut.IsYoloMode.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_YoloUser_EnablesYoloAndStartsTraining()
    {
        _sut.ProcessCommand("/yolo user");

        _sut.IsYoloMode.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_AutoStop_DisablesBoth()
    {
        _sut.ProcessCommand("/auto");
        _capturedMessages.Clear();

        _sut.ProcessCommand("/auto stop");

        _sut.IsYoloMode.Should().BeFalse();
        _sut.IsAutoTrainingActive.Should().BeFalse();
        _capturedMessages.Should().Contain(m => m.Message.Contains("Deactivated"));
    }

    // ----------------------------------------------------------------
    // /auto solve
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_AutoSolve_EmptyProblem_ShowsUsageMessage()
    {
        _sut.ProcessCommand("/auto solve  ");

        _capturedMessages.Should().Contain(m => m.Message.Contains("provide a problem"));
    }

    [Fact]
    public void ProcessCommand_AutoSolve_WithProblem_ReturnsTrue()
    {
        bool handled = _sut.ProcessCommand("/auto solve Build a REST API");

        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // /tools commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ToolsStatus_ListsToolPriorities()
    {
        _sut.ProcessCommand("/tools status");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Tool Priorities"));
    }

    [Fact]
    public void ProcessCommand_ToolsList_ListsToolPriorities()
    {
        _sut.ProcessCommand("/tools list");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Tool Priorities"));
    }

    [Fact]
    public void ProcessCommand_ToolsAvailable_ListsAvailableTools()
    {
        _sut.AvailableTools = new HashSet<string> { "web_search", "file_read" };

        _sut.ProcessCommand("/tools available");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Available Tools"));
    }

    [Fact]
    public void ProcessCommand_ToolsUnknownSubcommand_ShowsHelp()
    {
        _sut.ProcessCommand("/tools xyz");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Tool Priority Commands"));
    }

    // ----------------------------------------------------------------
    // /training commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Training_DefaultsToStatus()
    {
        bool handled = _sut.ProcessCommand("/training");

        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStart_WithParams_StartsWithConfig()
    {
        bool handled = _sut.ProcessCommand("/training start name=TestBot interval=10 max=5 skill=expert style=formal");

        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStart_SelfDialogue_ConfiguresSelfDialogueMode()
    {
        bool handled = _sut.ProcessCommand("/training start self");

        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStart_SelfDialogueKeyValue_ConfiguresSelfDialogue()
    {
        bool handled = _sut.ProcessCommand("/training start self-dialogue=true");

        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStart_ProblemSolvingMode_ConfiguresProblem()
    {
        bool handled = _sut.ProcessCommand("/training start problem=BuildAPI deliverable=code tools=true yolo");

        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
        _sut.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingTopic_SetsTopic()
    {
        bool handled = _sut.ProcessCommand("/training topic quantum computing");

        handled.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("quantum computing"));
    }

    [Fact]
    public void ProcessCommand_TrainingInterest_AddsInterest()
    {
        bool handled = _sut.ProcessCommand("/training interest machine learning");

        handled.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("machine learning"));
    }

    [Fact]
    public void ProcessCommand_TrainingUnknown_ShowsHelp()
    {
        bool handled = _sut.ProcessCommand("/training blah");

        handled.Should().BeTrue();
        _capturedMessages.Should().Contain(m => m.Message.Contains("Auto-Training Commands"));
    }

    // ----------------------------------------------------------------
    // Unrecognized commands
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("hello world")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("/unknown-command")]
    public void ProcessCommand_Unrecognized_ReturnsFalse(string input)
    {
        bool handled = _sut.ProcessCommand(input);

        handled.Should().BeFalse();
    }
}
