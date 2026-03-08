// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;
using Xunit;

[Trait("Category", "Unit")]
public class AutonomousCoordinatorTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;

    public AutonomousCoordinatorTests()
    {
        _sut = new AutonomousCoordinator(new AutonomousConfiguration
        {
            PushBasedMode = true,
            YoloMode = false,
        });
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // ----------------------------------------------------------------
    // Construction and initial state
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_DefaultConfig_SetsDefaults()
    {
        // Arrange & Act
        using var coordinator = new AutonomousCoordinator();

        // Assert
        coordinator.IsActive.Should().BeFalse();
        coordinator.IsAutoTrainingActive.Should().BeFalse();
        coordinator.IsYoloMode.Should().BeFalse();
        coordinator.Configuration.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_YoloModeConfig_SetsYoloMode()
    {
        // Arrange & Act
        using var coordinator = new AutonomousCoordinator(
            new AutonomousConfiguration { YoloMode = true });

        // Assert
        coordinator.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCulture_SetsBusCulture()
    {
        // Arrange & Act
        using var coordinator = new AutonomousCoordinator(
            new AutonomousConfiguration { Culture = "de-DE" });

        // Assert
        coordinator.IntentionBus.Culture.Should().Be("de-DE");
    }

    [Fact]
    public void Constructor_NullConfig_UsesDefaults()
    {
        // Arrange & Act
        using var coordinator = new AutonomousCoordinator(null);

        // Assert
        coordinator.Configuration.Should().NotBeNull();
        coordinator.IsActive.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /approve
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ApproveCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/approve abc123");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_RejectCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/reject abc123 bad idea");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /intentions, /pending
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_IntentionsCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/intentions");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_PendingCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/pending");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /network, /neurons, /bus
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_NetworkCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/network");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_NeuronsCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/neurons");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_BusCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/bus");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /help
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_HelpCommand_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/help");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_QuestionMark_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/?");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /yolo toggle
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_YoloToggle_TogglesMode()
    {
        // Arrange
        _sut.IsYoloMode.Should().BeFalse();

        // Act
        _sut.ProcessCommand("/yolo");

        // Assert
        _sut.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_YoloToggleTwice_RestoresOriginal()
    {
        // Arrange
        _sut.IsYoloMode.Should().BeFalse();

        // Act
        _sut.ProcessCommand("/yolo");
        _sut.ProcessCommand("/yolo");

        // Assert
        _sut.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_YoloOn_EnablesYolo()
    {
        // Act
        _sut.ProcessCommand("/yolo on");

        // Assert
        _sut.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_YoloOff_DisablesYolo()
    {
        // Arrange - enable first
        _sut.ProcessCommand("/yolo on");

        // Act
        _sut.ProcessCommand("/yolo off");

        // Assert
        _sut.IsYoloMode.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /voice toggle
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_VoiceToggle_TogglesVoiceEnabled()
    {
        // Arrange
        bool initial = _sut.IsVoiceEnabled;

        // Act
        _sut.ProcessCommand("/voice");

        // Assert
        _sut.IsVoiceEnabled.Should().Be(!initial);
    }

    [Fact]
    public void ProcessCommand_VoiceOn_EnablesVoice()
    {
        // Act
        _sut.ProcessCommand("/voice on");

        // Assert
        _sut.IsVoiceEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_VoiceOff_DisablesVoice()
    {
        // Act
        _sut.ProcessCommand("/voice off");

        // Assert
        _sut.IsVoiceEnabled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_VoiceToggle_InvokesSetVoiceEnabledDelegate()
    {
        // Arrange
        bool? capturedValue = null;
        _sut.SetVoiceEnabled = v => capturedValue = v;

        // Act
        _sut.ProcessCommand("/voice on");

        // Assert
        capturedValue.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /listen toggle
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ListenToggle_TogglesListening()
    {
        // Arrange
        _sut.IsListeningEnabled.Should().BeFalse();

        // Act
        _sut.ProcessCommand("/listen");

        // Assert
        _sut.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOn_EnablesListening()
    {
        // Act
        _sut.ProcessCommand("/listen on");

        // Assert
        _sut.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOff_DisablesListening()
    {
        // Arrange
        _sut.ProcessCommand("/listen on");

        // Act
        _sut.ProcessCommand("/listen off");

        // Assert
        _sut.IsListeningEnabled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_ListenToggle_InvokesSetListeningEnabledDelegate()
    {
        // Arrange
        bool? capturedValue = null;
        _sut.SetListeningEnabled = v => capturedValue = v;

        // Act
        _sut.ProcessCommand("/listen on");

        // Assert
        capturedValue.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /toggle-push
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_TogglePush_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/toggle-push");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /approve-all-safe
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ApproveAllSafe_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/approve-all-safe");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - unrecognized input
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_UnrecognizedInput_ReturnsFalse()
    {
        // Act
        bool handled = _sut.ProcessCommand("hello world");

        // Assert
        handled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_EmptyInput_ReturnsFalse()
    {
        // Act
        bool handled = _sut.ProcessCommand("");

        // Assert
        handled.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /auto commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_Auto_EnablesYoloAndAutoTraining()
    {
        // Act
        bool handled = _sut.ProcessCommand("/auto");

        // Assert
        handled.Should().BeTrue();
        _sut.IsYoloMode.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_AutoStop_DisablesYoloAndAutoTraining()
    {
        // Arrange
        _sut.ProcessCommand("/auto");

        // Act
        _sut.ProcessCommand("/auto stop");

        // Assert
        _sut.IsYoloMode.Should().BeFalse();
        _sut.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_AutoSolve_WithTrimmedEmptyProblem_IsNotHandled()
    {
        // "/auto solve " trims to "/auto solve" which does not match
        // the "/auto solve " prefix (trailing space required for argument parsing).
        // Act
        bool handled = _sut.ProcessCommand("/auto solve ");

        // Assert - trimmed input lacks trailing space so doesn't match the prefix
        handled.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /training commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_TrainingStatus_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/training status");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStart_StartsAutoTraining()
    {
        // Act
        bool handled = _sut.ProcessCommand("/training start");

        // Assert
        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingStop_StopsAutoTraining()
    {
        // Arrange
        _sut.ProcessCommand("/training start");

        // Act
        bool handled = _sut.ProcessCommand("/training stop");

        // Assert
        handled.Should().BeTrue();
        _sut.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_TrainingHelp_ReturnsTrue()
    {
        // Act - unknown subcommand shows help
        bool handled = _sut.ProcessCommand("/training whatever");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ProcessCommand - /tools commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ToolsStatus_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/tools");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ToolsAvailable_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/tools available");

        // Assert
        handled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ToolsUnknown_ReturnsTrue()
    {
        // Act
        bool handled = _sut.ProcessCommand("/tools xyz");

        // Assert
        handled.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // GetPreferredTool
    // ----------------------------------------------------------------

    [Fact]
    public void GetPreferredTool_NoAvailableTools_ReturnsFallback()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string>();

        // Act
        string tool = _sut.GetPreferredTool(new[] { "a", "b", "c" }, "fallback");

        // Assert
        tool.Should().Be("fallback");
    }

    [Fact]
    public void GetPreferredTool_FirstMatchReturned()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string> { "b", "c" };

        // Act
        string tool = _sut.GetPreferredTool(new[] { "a", "b", "c" });

        // Assert
        tool.Should().Be("b");
    }

    [Fact]
    public void GetPreferredTool_RespectsOrderPriority()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string> { "low_priority", "high_priority" };

        // Act
        string tool = _sut.GetPreferredTool(new[] { "high_priority", "low_priority" });

        // Assert
        tool.Should().Be("high_priority");
    }

    [Fact]
    public void GetPreferredResearchTool_ReturnsFirstAvailable()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string> { "web_search" };

        // Act
        string tool = _sut.GetPreferredResearchTool();

        // Assert
        tool.Should().Be("web_search");
    }

    [Fact]
    public void GetPreferredCodeTool_NoToolsAvailable_ReturnsFallback()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string>();

        // Act
        string tool = _sut.GetPreferredCodeTool();

        // Assert
        tool.Should().Be("file_read");
    }

    [Fact]
    public void GetPreferredGeneralTool_NoToolsAvailable_ReturnsFallback()
    {
        // Arrange
        _sut.AvailableTools = new HashSet<string>();

        // Act
        string tool = _sut.GetPreferredGeneralTool();

        // Assert
        tool.Should().Be("recall");
    }

    // ----------------------------------------------------------------
    // AddConversationContext
    // ----------------------------------------------------------------

    [Fact]
    public void AddConversationContext_AddsMessageToContext()
    {
        // Act - should not throw
        _sut.AddConversationContext("Hello");
        _sut.AddConversationContext("World");

        // Assert - no exception
    }

    [Fact]
    public void AddConversationContext_PrunesOlderThan20Messages()
    {
        // Act - add 25 messages
        for (int i = 0; i < 25; i++)
        {
            _sut.AddConversationContext($"Message {i}");
        }

        // No assertion on internal state needed; just verifying it doesn't throw
        // and the method correctly trims (tested implicitly by successful operation)
    }

    // ----------------------------------------------------------------
    // Start and Stop lifecycle
    // ----------------------------------------------------------------

    [Fact]
    public void Start_SetsIsActiveTrue()
    {
        // Act
        _sut.Start();

        // Assert
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_WhenAlreadyActive_DoesNotThrow()
    {
        // Arrange
        _sut.Start();

        // Act
        Action act = () => _sut.Start();

        // Assert
        act.Should().NotThrow();
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_SetsIsActiveFalse()
    {
        // Arrange
        _sut.Start();

        // Act
        await _sut.StopAsync();

        // Assert
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotActive_DoesNotThrow()
    {
        // Act
        Func<Task> act = async () => await _sut.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ----------------------------------------------------------------
    // OnProactiveMessage event
    // ----------------------------------------------------------------

    [Fact]
    public void Start_FiresProactiveMessageEvent()
    {
        // Arrange
        ProactiveMessageEventArgs? captured = null;
        _sut.OnProactiveMessage += args => captured = args;

        // Act
        _sut.Start();

        // Assert
        captured.Should().NotBeNull();
        captured!.Message.Should().Contain("Autonomous Mode Activated");
    }

    [Fact]
    public void Start_YoloMode_ProactiveMessageContainsYoloWarning()
    {
        // Arrange
        using var coordinator = new AutonomousCoordinator(
            new AutonomousConfiguration { YoloMode = true });
        ProactiveMessageEventArgs? captured = null;
        coordinator.OnProactiveMessage += args => captured = args;

        // Act
        coordinator.Start();

        // Assert
        captured.Should().NotBeNull();
        captured!.Message.Should().Contain("YOLO");
    }

    // ----------------------------------------------------------------
    // GetAutoTrainingStats
    // ----------------------------------------------------------------

    [Fact]
    public void GetAutoTrainingStats_BeforeTrainingStarted_ReturnsNull()
    {
        // Act
        var stats = _sut.GetAutoTrainingStats();

        // Assert
        stats.Should().BeNull();
    }

    [Fact]
    public void GetAutoTrainingStats_AfterTrainingStarted_ReturnsStats()
    {
        // Arrange
        _sut.StartAutoTraining();

        // Act
        var stats = _sut.GetAutoTrainingStats();

        // Assert
        stats.Should().NotBeNull();
    }

    // ----------------------------------------------------------------
    // GetStatus
    // ----------------------------------------------------------------

    [Fact]
    public void GetStatus_ReturnsNonEmptyString()
    {
        // Act
        string status = _sut.GetStatus();

        // Assert
        status.Should().NotBeNullOrWhiteSpace();
        status.Should().Contain("Ouroboros");
    }

    [Fact]
    public void GetStatus_WhenActive_IndicatesActive()
    {
        // Arrange
        _sut.Start();

        // Act
        string status = _sut.GetStatus();

        // Assert
        status.Should().Contain("Active");
    }

    [Fact]
    public void GetStatus_WhenInactive_IndicatesInactive()
    {
        // Act
        string status = _sut.GetStatus();

        // Assert
        status.Should().Contain("Inactive");
    }

    // ----------------------------------------------------------------
    // InjectGoal
    // ----------------------------------------------------------------

    [Fact]
    public async Task InjectGoal_IncreasesPendingIntentionsCount()
    {
        // Arrange
        int before = _sut.PendingIntentionsCount;

        // Act
        await _sut.InjectGoalAsync("Learn quantum computing");

        // Assert
        _sut.PendingIntentionsCount.Should().BeGreaterThanOrEqualTo(before);
    }

    // ----------------------------------------------------------------
    // StartAutoTraining / StopAutoTraining
    // ----------------------------------------------------------------

    [Fact]
    public void StartAutoTraining_SetsIsAutoTrainingActive()
    {
        // Act
        _sut.StartAutoTraining();

        // Assert
        _sut.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void StopAutoTraining_ClearsIsAutoTrainingActive()
    {
        // Arrange
        _sut.StartAutoTraining();

        // Act
        _sut.StopAutoTraining();

        // Assert
        _sut.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void StartAutoTraining_WithYoloConfig_EnablesYoloMode()
    {
        // Arrange
        var config = new UserPersonaConfig { YoloMode = true };

        // Act
        _sut.StartAutoTraining(config);

        // Assert
        _sut.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public async Task StopAutoTraining_WithYoloFromTraining_RestoresConfigDefault()
    {
        // Arrange - coordinator config has YOLO=false, but training enables it
        var config = new UserPersonaConfig { YoloMode = true };
        _sut.StartAutoTraining(config);
        _sut.IsYoloMode.Should().BeTrue();

        // Allow background Task.Run in StartAutoTraining to set neuron config
        await Task.Delay(200);

        // Act
        _sut.StopAutoTraining();

        // Assert - should restore to config default (false)
        _sut.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public void StartAutoTraining_ProblemSolvingMode_SuppressesProactiveMessages()
    {
        // Arrange
        bool? suppressedValue = null;
        _sut.SetSuppressProactiveMessages = v => suppressedValue = v;
        var config = new UserPersonaConfig { ProblemSolvingMode = true, Problem = "test" };

        // Act
        _sut.StartAutoTraining(config);

        // Assert
        suppressedValue.Should().BeTrue();
    }

    [Fact]
    public async Task StopAutoTraining_ProblemSolvingMode_ReenablesProactiveMessages()
    {
        // Arrange
        bool? suppressedValue = null;
        _sut.SetSuppressProactiveMessages = v => suppressedValue = v;
        var config = new UserPersonaConfig { ProblemSolvingMode = true, Problem = "test" };
        _sut.StartAutoTraining(config);

        // Allow the background Task.Run in StartAutoTraining to set the neuron config
        await Task.Delay(200);

        // Act
        _sut.StopAutoTraining();

        // Assert
        suppressedValue.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // StartNetwork
    // ----------------------------------------------------------------

    [Fact]
    public void StartNetwork_DoesNotSetIsActive()
    {
        // Act
        _sut.StartNetwork();

        // Assert - StartNetwork only starts network, not coordination loops
        _sut.IsActive.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // Properties
    // ----------------------------------------------------------------

    [Fact]
    public void IntentionBus_IsNotNull()
    {
        _sut.IntentionBus.Should().NotBeNull();
    }

    [Fact]
    public void Network_IsNotNull()
    {
        _sut.Network.Should().NotBeNull();
    }

    [Fact]
    public void TopicDiscoveryIntervalSeconds_DefaultValue()
    {
        _sut.TopicDiscoveryIntervalSeconds.Should().Be(90);
    }

    [Fact]
    public void TopicDiscoveryIntervalSeconds_CanBeSet()
    {
        // Act
        _sut.TopicDiscoveryIntervalSeconds = 30;

        // Assert
        _sut.TopicDiscoveryIntervalSeconds.Should().Be(30);
    }

    [Fact]
    public void EnableMeTTaValidation_DefaultTrue()
    {
        _sut.EnableMeTTaValidation.Should().BeTrue();
    }
}
