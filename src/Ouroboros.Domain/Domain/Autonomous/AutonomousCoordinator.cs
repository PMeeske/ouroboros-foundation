// <copyright file="AutonomousCoordinator.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// The AutonomousCoordinator manages Ouroboros's push-based autonomous behavior.
/// It coordinates the neural network, intention bus, and provides a unified interface.
/// </summary>
public sealed partial class AutonomousCoordinator : IDisposable
{
    private readonly IntentionBus _intentionBus;
    private readonly OuroborosNeuralNetwork _network;
    private readonly AutonomousConfiguration _config;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<ProactiveMessageEventArgs> _messageHistory = [];

    private Task? _coordinationTask;
    private Task? _executionTask;
    private bool _isActive;
    private DateTime _lastTick = DateTime.MinValue;

    /// <summary>
    /// Creates a new autonomous coordinator.
    /// </summary>
    public AutonomousCoordinator(AutonomousConfiguration? config = null)
    {
        _config = config ?? new AutonomousConfiguration();
        _intentionBus = new IntentionBus();
        _network = new OuroborosNeuralNetwork(_intentionBus);
        IsYoloMode = _config.YoloMode;

        // Set culture for localization
        if (!string.IsNullOrEmpty(_config.Culture))
        {
            _intentionBus.Culture = _config.Culture;
        }

        // Wire up events
        _intentionBus.OnProactiveMessage += HandleProactiveMessage;
        _intentionBus.OnIntentionRequiresAttention += HandleIntentionRequiresAttention;

        // Initialize core neurons
        InitializeCoreNeurons();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Events
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Event fired when Ouroboros wants to communicate proactively.
    /// </summary>
    public event Action<ProactiveMessageEventArgs>? OnProactiveMessage;

    /// <summary>
    /// Event fired when a new intention requires user attention.
    /// </summary>
    public event Action<Intention>? OnIntentionRequiresAttention;

    // ═══════════════════════════════════════════════════════════════════════════
    // Core Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the intention bus.
    /// </summary>
    public IntentionBus IntentionBus => _intentionBus;

    /// <summary>
    /// Gets the neural network.
    /// </summary>
    public OuroborosNeuralNetwork Network => _network;

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public AutonomousConfiguration Configuration => _config;

    /// <summary>
    /// Whether the coordinator is running.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Gets the count of pending intentions.
    /// </summary>
    public int PendingIntentionsCount => _intentionBus.PendingCount;

    // ═══════════════════════════════════════════════════════════════════════════
    // Delegate Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Delegate for executing tool actions.
    /// </summary>
    public Func<string, string, CancellationToken, Task<string>>? ExecuteToolFunction { get; set; }

    /// <summary>
    /// Delegate for embedding text.
    /// </summary>
    public Func<string, CancellationToken, Task<float[]>>? EmbedFunction { get; set; }

    /// <summary>
    /// Delegate for storing to Qdrant.
    /// </summary>
    public Func<string, string, float[], CancellationToken, Task>? StoreToQdrantFunction { get; set; }

    /// <summary>
    /// Delegate for searching Qdrant.
    /// </summary>
    public Func<float[], int, CancellationToken, Task<IReadOnlyList<string>>>? SearchQdrantFunction { get; set; }

    /// <summary>
    /// Delegate for storing intentions to Qdrant.
    /// </summary>
    public Func<Intention, CancellationToken, Task>? StoreIntentionFunction { get; set; }

    /// <summary>
    /// Delegate for storing neuron messages to Qdrant.
    /// </summary>
    public Func<NeuronMessage, CancellationToken, Task>? StoreNeuronMessageFunction { get; set; }

    /// <summary>
    /// Delegate for LLM thinking/generation.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? ThinkFunction { get; set; }

    /// <summary>
    /// Delegate for MeTTa symbolic query execution.
    /// Returns the query result or error message.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? MeTTaQueryFunction { get; set; }

    /// <summary>
    /// Delegate for adding MeTTa facts.
    /// </summary>
    public Func<string, CancellationToken, Task<bool>>? MeTTaAddFactFunction { get; set; }

    /// <summary>
    /// Delegate for verifying DAG constraints via MeTTa.
    /// Parameters: branchName, constraint, cancellationToken
    /// Returns true if constraint is satisfied.
    /// </summary>
    public Func<string, string, CancellationToken, Task<bool>>? VerifyDagConstraintFunction { get; set; }

    /// <summary>
    /// Delegate for processing auto-training messages (sends to main chat pipeline).
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? ProcessChatFunction { get; set; }

    /// <summary>
    /// Delegate for displaying a message and waiting for TTS to complete.
    /// Parameters: message, persona (null=Ouroboros), cancellationToken
    /// This ensures proper sequencing of User/Ouroboros in auto-training.
    /// </summary>
    public Func<string, string?, CancellationToken, Task>? DisplayAndSpeakFunction { get; set; }

    /// <summary>
    /// Delegate for full chat with tools (for User persona in problem-solving mode).
    /// This gives the User persona access to tools, memory, and MeTTa.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? FullChatWithToolsFunction { get; set; }

    /// <summary>
    /// Action to suppress or enable proactive messages from AutonomousMind.
    /// Parameter: true to suppress, false to enable.
    /// </summary>
    public Action<bool>? SetSuppressProactiveMessages { get; set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // State Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recent conversation context for topic discovery.
    /// </summary>
    private readonly List<string> _conversationContext = [];

    /// <summary>
    /// Last time we discovered a new topic.
    /// </summary>
    private DateTime _lastTopicDiscovery = DateTime.MinValue;

    /// <summary>
    /// Interval between autonomous topic discovery (in seconds).
    /// </summary>
    public int TopicDiscoveryIntervalSeconds { get; set; } = 90;

    /// <summary>
    /// Whether to validate intentions using MeTTa symbolic reasoning before execution.
    /// </summary>
    public bool EnableMeTTaValidation { get; set; } = true;

    /// <summary>
    /// Whether auto-training mode is active.
    /// </summary>
    public bool IsAutoTrainingActive { get; private set; }

    /// <summary>
    /// Whether YOLO mode is active (auto-approve ALL intentions). Runtime toggle.
    /// Initialized from config but can be changed via /yolo command.
    /// </summary>
    public bool IsYoloMode { get; private set; }

    /// <summary>
    /// Whether voice output (TTS) is enabled.
    /// </summary>
    public bool IsVoiceEnabled { get; set; } = true;

    /// <summary>
    /// Whether voice input (STT/listening) is enabled.
    /// </summary>
    public bool IsListeningEnabled { get; set; }

    /// <summary>
    /// Action to enable/disable TTS in the voice side channel.
    /// </summary>
    public Action<bool>? SetVoiceEnabled { get; set; }

    /// <summary>
    /// Action to start/stop speech recognition.
    /// </summary>
    public Action<bool>? SetListeningEnabled { get; set; }

    /// <summary>
    /// Event fired when speech is recognized from the user.
    /// </summary>
#pragma warning disable CS0067 // Event is never used - public API for external subscribers
    public event Action<string>? OnSpeechRecognized;
#pragma warning restore CS0067

    /// <summary>
    /// Set of available tool names for priority resolution.
    /// Populated from the tool registry during setup.
    /// </summary>
    public HashSet<string> AvailableTools { get; set; } = [];

    // ═══════════════════════════════════════════════════════════════════════════
    // Tool Priority Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the preferred tool from a priority list based on availability.
    /// Returns the first available tool in the priority list, or the fallback.
    /// </summary>
    /// <param name="priorityList">Ordered list of preferred tools.</param>
    /// <param name="fallback">Fallback tool if none are available.</param>
    /// <returns>The name of the best available tool.</returns>
    public string GetPreferredTool(IEnumerable<string> priorityList, string fallback = "web_search")
    {
        foreach (string toolName in priorityList)
        {
            if (AvailableTools.Contains(toolName))
            {
                return toolName;
            }
        }

        return fallback;
    }

    /// <summary>
    /// Gets the preferred research tool based on configuration.
    /// </summary>
    public string GetPreferredResearchTool() =>
        GetPreferredTool(_config.ResearchToolPriority, "web_search");

    /// <summary>
    /// Gets the preferred code tool based on configuration.
    /// </summary>
    public string GetPreferredCodeTool() =>
        GetPreferredTool(_config.CodeToolPriority, "file_read");

    /// <summary>
    /// Gets the preferred general tool based on configuration.
    /// </summary>
    public string GetPreferredGeneralTool() =>
        GetPreferredTool(_config.GeneralToolPriority, "recall");

    /// <summary>
    /// Adds context from recent conversation for topic discovery.
    /// </summary>
    public void AddConversationContext(string message)
    {
        _conversationContext.Add(message);
        // Keep last 20 messages
        while (_conversationContext.Count > 20)
        {
            _conversationContext.RemoveAt(0);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Lifecycle
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Starts just the neural network for status visibility (without coordination loops).
    /// Use this for passive mode where you want status checks but not proactive behavior.
    /// </summary>
    public void StartNetwork()
    {
        // Configure neurons with functions
        ConfigureNeurons();

        // Start network (activates neurons for status reporting)
        _network.Start();
    }

    /// <summary>
    /// Starts the autonomous coordinator.
    /// </summary>
    public void Start()
    {
        if (_isActive) return;
        _isActive = true;

        // Configure neurons with functions (if not already done by StartNetwork)
        ConfigureNeurons();

        // Start network and bus
        _network.Start();

        // Start coordination and execution loops
        _coordinationTask = Task.Run(CoordinationLoopAsync);
        _executionTask = Task.Run(ExecutionLoopAsync);

        string modeDescription = _config.YoloMode
            ? "🤠 **YOLO Mode (I will act autonomously without asking!)**"
            : (_config.PushBasedMode ? "Push-Based (I'll propose actions for your approval)" : "Reactive");

        RaiseProactiveMessage(
            "🐍 **Ouroboros Autonomous Mode Activated**\n" +
            $"Mode: {modeDescription}\n" +
            (_config.YoloMode
                ? "⚠️ YOLO mode is enabled! I will execute actions without approval. Use with caution!"
                : "I am now thinking, reflecting, and planning. I will ask before acting."),
            IntentionPriority.Normal,
            "coordinator");
    }

    /// <summary>
    /// Stops the autonomous coordinator.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isActive) return;
        _isActive = false;
        _cts.Cancel();

        await _network.StopAsync();

        if (_coordinationTask != null) await _coordinationTask;
        if (_executionTask != null) await _executionTask;

        RaiseProactiveMessage(
            "💤 Autonomous mode deactivated. I'll wait for your instructions.",
            IntentionPriority.Normal,
            "coordinator");
    }

    /// <summary>
    /// Gets a status summary.
    /// </summary>
    public string GetStatus()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("🐍 **Ouroboros Autonomous Status**\n");
        sb.AppendLine($"**Mode:** {(_config.PushBasedMode ? "Push-Based 🔔" : "Reactive 🔇")}");
        sb.AppendLine($"**Status:** {(_isActive ? "Active 🟢" : "Inactive 🔴")}");
        sb.AppendLine();
        sb.AppendLine(_intentionBus.GetSummary());
        sb.AppendLine();
        sb.AppendLine($"**Neurons:** {_network.Neurons.Count}");
        sb.AppendLine($"**Recent Messages:** {_network.GetRecentMessages(1).Count > 0}");
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Internal Coordination
    // ═══════════════════════════════════════════════════════════════════════════

    private void InitializeCoreNeurons()
    {
        _network.RegisterNeuron(new ExecutiveNeuron());
        _network.RegisterNeuron(new MemoryNeuron());
        _network.RegisterNeuron(new CodeReflectionNeuron());
        _network.RegisterNeuron(new SymbolicNeuron());
        _network.RegisterNeuron(new SafetyNeuron());
        _network.RegisterNeuron(new CommunicationNeuron());
        _network.RegisterNeuron(new AffectNeuron());
    }

    private void ConfigureNeurons()
    {
        // Configure memory neuron with Qdrant functions
        if (_network.GetNeuron("neuron.memory") is MemoryNeuron memoryNeuron)
        {
            memoryNeuron.EmbedFunction = EmbedFunction;
            memoryNeuron.StoreFunction = StoreToQdrantFunction;
            memoryNeuron.SearchFunction = SearchQdrantFunction;
        }

        // Configure symbolic neuron with MeTTa functions
        if (_network.GetNeuron("neuron.symbolic") is SymbolicNeuron symbolicNeuron)
        {
            // Wire MeTTa query capability
            symbolicNeuron.MeTTaQueryFunction = MeTTaQueryFunction;
            symbolicNeuron.MeTTaAddFactFunction = MeTTaAddFactFunction;
        }

        // Configure communication neuron events
        if (_network.GetNeuron("neuron.communication") is CommunicationNeuron commNeuron)
        {
            commNeuron.OnUserMessage += (msg, priority) =>
                RaiseProactiveMessage(msg, priority, "neuron.communication");
        }

        // Configure network embedding and persistence
        _network.EmbedFunction = EmbedFunction;
        _network.PersistMessageFunction = StoreNeuronMessageFunction;
    }

    private async Task CoordinationLoopAsync()
    {
        while (_isActive && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_config.TickIntervalSeconds), _cts.Token);

                // Auto-approve configured categories
                AutoApproveConfiguredCategories();

                // Autonomous topic discovery
                if ((DateTime.UtcNow - _lastTopicDiscovery).TotalSeconds >= TopicDiscoveryIntervalSeconds)
                {
                    await DiscoverAndProposeTopicsAsync(_cts.Token);
                    _lastTopicDiscovery = DateTime.UtcNow;
                }

                // Check for stale intentions
                IReadOnlyList<Intention> pending = _intentionBus.GetPendingIntentions();
                if (pending.Count > _config.MaxPendingIntentions)
                {
                    RaiseProactiveMessage(
                        $"⚠️ I have {pending.Count} pending intentions waiting for your decision. " +
                        "Consider using `/approve-all-safe` or reviewing `/intentions`.",
                        IntentionPriority.High, "coordinator");
                }

                // Periodic status broadcast to neurons
                _network.Broadcast("system.tick", new { Time = DateTime.UtcNow, Pending = pending.Count }, "coordinator");

                _lastTick = DateTime.UtcNow;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Coordination error: {ex.Message}");
            }
        }
    }

    private async Task ExecutionLoopAsync()
    {
        while (_isActive && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);

                // Execute next approved intention
                Intention? intention = _intentionBus.GetNextApprovedIntention();
                if (intention != null)
                {
                    await ExecuteIntentionAsync(intention, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Execution error: {ex.Message}");
            }
        }
    }

    private void HandleProactiveMessage(string message, IntentionPriority priority)
    {
        RaiseProactiveMessage(message, priority, "intention_bus");
    }

    private void HandleIntentionRequiresAttention(Intention intention)
    {
        // Store intention to Qdrant for persistence
        _ = Task.Run(async () =>
        {
            try
            {
                if (StoreIntentionFunction != null)
                {
                    await StoreIntentionFunction(intention, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to store intention: {ex.Message}");
            }
        });

        OnIntentionRequiresAttention?.Invoke(intention);
    }

    private void RaiseProactiveMessage(string message, IntentionPriority priority, string source)
    {
        ProactiveMessageEventArgs args = new ProactiveMessageEventArgs(message, priority, source, DateTime.UtcNow);
        _messageHistory.Add(args);

        // Keep history bounded
        while (_messageHistory.Count > 100)
        {
            _messageHistory.RemoveAt(0);
        }

        OnProactiveMessage?.Invoke(args);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _isActive = false;
        _cts.Cancel();
        _network.Dispose();
        _cts.Dispose();
    }
}
