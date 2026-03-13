namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class AutonomousCoordinatorTests : IDisposable
{
    private readonly AutonomousCoordinator _coordinator;

    public AutonomousCoordinatorTests()
    {
        _coordinator = new AutonomousCoordinator(new AutonomousConfiguration
        {
            PushBasedMode = true,
            TickIntervalSeconds = 60,
            YoloMode = false,
        });
    }

    public void Dispose() => _coordinator.Dispose();

    // ═══════════════════════════════════════════════════════════════
    // Construction
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_Default_CreatesWithDefaultConfig()
    {
        using var coord = new AutonomousCoordinator();
        coord.Configuration.Should().NotBeNull();
        coord.IntentionBus.Should().NotBeNull();
        coord.Network.Should().NotBeNull();
        coord.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithConfig_AppliesConfig()
    {
        using var coord = new AutonomousCoordinator(new AutonomousConfiguration { YoloMode = true });
        coord.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void Constructor_InitializesCoreNeurons()
    {
        _coordinator.Network.Neurons.Should().HaveCountGreaterThanOrEqualTo(7);
        _coordinator.Network.GetNeuron("neuron.executive").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.memory").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.code").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.symbolic").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.safety").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.communication").Should().NotBeNull();
        _coordinator.Network.GetNeuron("neuron.affect").Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCulture_SetsCultureOnBus()
    {
        using var coord = new AutonomousCoordinator(new AutonomousConfiguration { Culture = "de-DE" });
        coord.IntentionBus.Culture.Should().Be("de-DE");
    }

    // ═══════════════════════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Start_SetsIsActive()
    {
        _coordinator.Start();
        _coordinator.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_FiresProactiveMessage()
    {
        ProactiveMessageEventArgs? received = null;
        _coordinator.OnProactiveMessage += args => received = args;

        _coordinator.Start();

        received.Should().NotBeNull();
        received!.Message.Should().Contain("Autonomous Mode Activated");
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        _coordinator.Start();
        var act = () => _coordinator.Start();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_SetsInactive()
    {
        _coordinator.Start();
        await _coordinator.StopAsync();
        _coordinator.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotActive_DoesNotThrow()
    {
        var act = () => _coordinator.StopAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void StartNetwork_DoesNotSetIsActive()
    {
        _coordinator.StartNetwork();
        _coordinator.IsActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void PendingIntentionsCount_ReflectsBusCount()
    {
        _coordinator.IntentionBus.ProposeIntention("Test", "Desc", "Reason",
            IntentionCategory.Learning, "test");

        _coordinator.PendingIntentionsCount.Should().Be(1);
    }

    [Fact]
    public void DelegateProperties_DefaultToNull()
    {
        _coordinator.ExecuteToolFunction.Should().BeNull();
        _coordinator.EmbedFunction.Should().BeNull();
        _coordinator.StoreToQdrantFunction.Should().BeNull();
        _coordinator.SearchQdrantFunction.Should().BeNull();
        _coordinator.ThinkFunction.Should().BeNull();
        _coordinator.MeTTaQueryFunction.Should().BeNull();
        _coordinator.MeTTaAddFactFunction.Should().BeNull();
        _coordinator.ProcessChatFunction.Should().BeNull();
    }

    [Fact]
    public void TopicDiscoveryIntervalSeconds_DefaultIs90()
    {
        _coordinator.TopicDiscoveryIntervalSeconds.Should().Be(90);
    }

    [Fact]
    public void EnableMeTTaValidation_DefaultIsTrue()
    {
        _coordinator.EnableMeTTaValidation.Should().BeTrue();
    }

    [Fact]
    public void AvailableTools_DefaultIsEmpty()
    {
        _coordinator.AvailableTools.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetPreferredTool
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetPreferredTool_NoAvailableTools_ReturnsFallback()
    {
        string result = _coordinator.GetPreferredTool(new[] { "tool_a", "tool_b" }, "fallback");
        result.Should().Be("fallback");
    }

    [Fact]
    public void GetPreferredTool_FirstAvailable_ReturnsFirst()
    {
        _coordinator.AvailableTools = new HashSet<string> { "tool_b", "tool_c" };
        string result = _coordinator.GetPreferredTool(new[] { "tool_a", "tool_b", "tool_c" });
        result.Should().Be("tool_b");
    }

    [Fact]
    public void GetPreferredResearchTool_UsesConfigPriority()
    {
        _coordinator.AvailableTools = new HashSet<string> { "web_search" };
        string result = _coordinator.GetPreferredResearchTool();
        result.Should().Be("web_search");
    }

    [Fact]
    public void GetPreferredCodeTool_UsesConfigPriority()
    {
        _coordinator.AvailableTools = new HashSet<string> { "file_read" };
        string result = _coordinator.GetPreferredCodeTool();
        result.Should().Be("file_read");
    }

    // ═══════════════════════════════════════════════════════════════
    // AddConversationContext
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void AddConversationContext_AddsToContext()
    {
        _coordinator.AddConversationContext("Hello");
        // No direct accessor, but should not throw
    }

    [Fact]
    public void AddConversationContext_BoundsAt20()
    {
        for (int i = 0; i < 25; i++)
        {
            _coordinator.AddConversationContext($"Message {i}");
        }
        // Should not throw; internal list bounded to 20
    }

    // ═══════════════════════════════════════════════════════════════
    // Commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProcessCommand_NonCommand_ReturnsFalse()
    {
        bool result = _coordinator.ProcessCommand("Hello, how are you?");
        result.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_Help_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/help");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_QuestionMark_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/?");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Intentions_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/intentions");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Pending_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/pending");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Network_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/network");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Neurons_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/neurons");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Bus_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/bus");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TogglePush_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/toggle-push");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ApproveAllSafe_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/approve-all-safe");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Approve_WithValidId_ReturnsTrue()
    {
        var intention = _coordinator.IntentionBus.ProposeIntention(
            "Test", "Desc", "Reason", IntentionCategory.Learning, "test");
        string partial = intention.Id.ToString()[..8];

        bool result = _coordinator.ProcessCommand($"/approve {partial}");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Reject_WithId_ReturnsTrue()
    {
        var intention = _coordinator.IntentionBus.ProposeIntention(
            "Test", "Desc", "Reason", IntentionCategory.Learning, "test");
        string partial = intention.Id.ToString()[..8];

        bool result = _coordinator.ProcessCommand($"/reject {partial} Not needed");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_YoloToggle_TogglesMode()
    {
        _coordinator.IsYoloMode.Should().BeFalse();

        _coordinator.ProcessCommand("/yolo");
        _coordinator.IsYoloMode.Should().BeTrue();

        _coordinator.ProcessCommand("/yolo");
        _coordinator.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_YoloOn_EnablesYolo()
    {
        _coordinator.ProcessCommand("/yolo on");
        _coordinator.IsYoloMode.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_YoloOff_DisablesYolo()
    {
        _coordinator.ProcessCommand("/yolo on");
        _coordinator.ProcessCommand("/yolo off");
        _coordinator.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_YoloOn_ApprovesAllPending()
    {
        _coordinator.IntentionBus.ProposeIntention(
            "Test", "Desc", "Reason", IntentionCategory.Learning, "test");

        _coordinator.ProcessCommand("/yolo on");

        _coordinator.IntentionBus.PendingCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // Voice Commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProcessCommand_VoiceToggle_TogglesVoice()
    {
        bool initial = _coordinator.IsVoiceEnabled;
        _coordinator.ProcessCommand("/voice");
        _coordinator.IsVoiceEnabled.Should().Be(!initial);
    }

    [Fact]
    public void ProcessCommand_VoiceOn_EnablesVoice()
    {
        _coordinator.IsVoiceEnabled = false;
        _coordinator.ProcessCommand("/voice on");
        _coordinator.IsVoiceEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_VoiceOff_DisablesVoice()
    {
        _coordinator.ProcessCommand("/voice off");
        _coordinator.IsVoiceEnabled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_ListenToggle_TogglesListening()
    {
        _coordinator.IsListeningEnabled.Should().BeFalse();
        _coordinator.ProcessCommand("/listen");
        _coordinator.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOn_EnablesListening()
    {
        _coordinator.ProcessCommand("/listen on");
        _coordinator.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOff_DisablesListening()
    {
        _coordinator.ProcessCommand("/listen on");
        _coordinator.ProcessCommand("/listen off");
        _coordinator.IsListeningEnabled.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Tools Commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProcessCommand_ToolsStatus_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/tools");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ToolsAvailable_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/tools available");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ToolsHelp_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/tools help");
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Training Commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProcessCommand_TrainingStatus_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/training status");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_TrainingHelp_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/training help");
        result.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_Training_ReturnsTrue()
    {
        bool result = _coordinator.ProcessCommand("/training");
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetStatus
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetStatus_ContainsExpectedInfo()
    {
        string status = _coordinator.GetStatus();

        status.Should().Contain("Ouroboros Autonomous Status");
        status.Should().Contain("Mode:");
        status.Should().Contain("Status:");
        status.Should().Contain("Neurons:");
    }

    // ═══════════════════════════════════════════════════════════════
    // InjectGoalAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task InjectGoalAsync_CreatesIntention()
    {
        await _coordinator.InjectGoalAsync("Learn quantum computing");

        _coordinator.IntentionBus.PendingCount.Should().BeGreaterThanOrEqualTo(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // SendToNeuronAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task SendToNeuronAsync_DoesNotThrow()
    {
        var act = () => _coordinator.SendToNeuronAsync("neuron.memory", "memory.recall", "test query");
        await act.Should().NotThrowAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // Auto Training
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void StartAutoTraining_SetsIsAutoTrainingActive()
    {
        _coordinator.StartAutoTraining();
        _coordinator.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void StopAutoTraining_SetsIsAutoTrainingInactive()
    {
        _coordinator.StartAutoTraining();
        _coordinator.StopAutoTraining();
        _coordinator.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void GetAutoTrainingStats_WhenNotStarted_ReturnsNull()
    {
        _coordinator.GetAutoTrainingStats().Should().BeNull();
    }

    [Fact]
    public void GetAutoTrainingStats_AfterStart_ReturnsStats()
    {
        _coordinator.StartAutoTraining();

        var stats = _coordinator.GetAutoTrainingStats();
        stats.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // Auto / Full Autonomous Commands
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProcessCommand_Auto_EnablesYoloAndTraining()
    {
        _coordinator.ProcessCommand("/auto");

        _coordinator.IsYoloMode.Should().BeTrue();
        _coordinator.IsAutoTrainingActive.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_AutoStop_DisablesYoloAndTraining()
    {
        _coordinator.ProcessCommand("/auto");
        _coordinator.ProcessCommand("/auto stop");

        _coordinator.IsYoloMode.Should().BeFalse();
        _coordinator.IsAutoTrainingActive.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // Dispose
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Dispose_SetsInactive()
    {
        _coordinator.Start();
        _coordinator.Dispose();

        _coordinator.IsActive.Should().BeFalse();
    }
}
