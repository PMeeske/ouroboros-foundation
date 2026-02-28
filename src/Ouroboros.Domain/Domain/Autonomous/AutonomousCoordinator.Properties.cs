// <copyright file="AutonomousCoordinator.Properties.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Delegate properties and state for the autonomous coordinator.
/// </summary>
public sealed partial class AutonomousCoordinator
{
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CS0067:Event is never used", Justification = "Public API for external subscribers")]
    public event Action<string>? OnSpeechRecognized;

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
}
