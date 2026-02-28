using System.Text;
using System.Text.Json;
using Ouroboros.Core.Randomness;
using Ouroboros.Providers.Random;

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The UserPersona neuron simulates an automatic chat user for training.
/// It generates contextual questions, evaluates responses, and provides feedback
/// to help the system learn and improve autonomously.
/// </summary>
public sealed partial class UserPersonaNeuron : Neuron
{
    private readonly List<TrainingInteraction> _interactions = [];
    private readonly List<string> _conversationHistory = [];
    private readonly Queue<string> _pendingQuestions = new();
    private readonly IRandomProvider _random = CryptoRandomProvider.Instance;

    private UserPersonaConfig _config = new();
    private bool _isTrainingActive;
    private int _sessionMessageCount;
    private DateTime _lastMessageTime = DateTime.MinValue;
    private string? _currentTopic;
    private int _followUpDepth;

    /// <inheritdoc/>
    public override string Id => "neuron.user_persona";

    /// <inheritdoc/>
    public override string Name => "Automatic User Persona";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Cognitive;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "training.*",
        "user_persona.*",
        "response.generated",
        "system.tick",
    };

    /// <summary>
    /// Delegate for generating questions using LLM.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? GenerateFunction { get; set; }

    /// <summary>
    /// Delegate for evaluating response quality.
    /// </summary>
    public Func<string, string, CancellationToken, Task<double>>? EvaluateFunction { get; set; }

    /// <summary>
    /// Delegate for web research using Firecrawl/web search.
    /// Input: search query, Output: research content.
    /// </summary>
    public Func<string, CancellationToken, Task<string>>? ResearchFunction { get; set; }

    /// <summary>
    /// Cached research content for current topic.
    /// </summary>
#pragma warning disable CS0169 // Field is never used - reserved for caching implementation
    private string? _currentResearchContent;
#pragma warning restore CS0169

    /// <summary>
    /// Last time research was performed.
    /// </summary>
    private DateTime _lastResearchTime = DateTime.MinValue;

    /// <summary>
    /// TaskCompletionSource to wait for response before sending next message.
    /// </summary>
    private TaskCompletionSource<bool>? _responseWaiter;

    /// <summary>
    /// Event raised when the persona wants to send a message.
    /// </summary>
    public event Action<string, UserPersonaConfig>? OnUserMessage;

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    public UserPersonaConfig Config => _config;

    /// <summary>
    /// Gets whether training is currently active.
    /// </summary>
    public bool IsTrainingActive => _isTrainingActive;

    /// <summary>
    /// Starts training directly (called from coordinator).
    /// </summary>
    public async Task StartTrainingDirectAsync(UserPersonaConfig config)
    {
        Console.WriteLine("  [UserPersonaNeuron] StartTrainingDirectAsync called");
        _config = config;
        await StartTrainingAsync(config, CancellationToken.None);
    }

    /// <summary>
    /// Gets training statistics.
    /// </summary>
    public (int TotalInteractions, double AverageSatisfaction, int SessionMessages) GetStats()
    {
        double avgSatisfaction = _interactions.Where(i => i.UserSatisfaction.HasValue)
            .Select(i => i.UserSatisfaction!.Value)
            .DefaultIfEmpty(0)
            .Average();

        return (_interactions.Count, avgSatisfaction, _sessionMessageCount);
    }

    /// <summary>
    /// Records an interaction manually (called from coordinator).
    /// </summary>
    public void RecordInteraction(string userMessage, string systemResponse)
    {
        Console.WriteLine("  [UserPersonaNeuron] RecordInteraction - Ouroboros responded, signaling completion");
        _conversationHistory.Add($"User: {userMessage}");
        _conversationHistory.Add($"Ouroboros: {systemResponse}");

        // Signal that response is complete - training loop can proceed
        _responseWaiter?.TrySetResult(true);

        TrainingInteraction interaction = new TrainingInteraction(
            Guid.NewGuid(),
            userMessage,
            systemResponse,
            DateTime.UtcNow,
            null, // Will be evaluated async later if EvaluateFunction is set
            null,
            new Dictionary<string, object>
            {
                ["topic"] = _currentTopic ?? "general",
                ["followUpDepth"] = _followUpDepth,
                ["sessionMessage"] = _sessionMessageCount,
                ["source"] = "manual_record"
            });

        _interactions.Add(interaction);

        // Keep interactions bounded
        while (_interactions.Count > 1000)
        {
            _interactions.RemoveAt(0);
        }
    }

    /// <inheritdoc/>
    protected override async Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "training.start":
                await StartTrainingAsync(message.Payload, ct);
                break;

            case "training.stop":
                StopTraining();
                break;

            case "training.configure":
                ConfigurePersona(message.Payload);
                break;

            case "user_persona.set_topic":
                _currentTopic = message.Payload?.ToString();
                _followUpDepth = 0;
                break;

            case "user_persona.add_interest":
                string? interest = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(interest) && !_config.Interests.Contains(interest))
                {
                    _config = _config with { Interests = [.. _config.Interests, interest] };
                }

                break;

            case "response.generated":
                await HandleSystemResponseAsync(message.Payload?.ToString() ?? "", ct);
                break;

            case "system.tick":
                if (_isTrainingActive)
                {
                    await TickTrainingAsync(ct);
                }

                break;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTickAsync(CancellationToken ct)
    {
        if (!_isTrainingActive) return;

        // Check if it's time to generate a new message
        double elapsed = (DateTime.UtcNow - _lastMessageTime).TotalSeconds;
        if (elapsed >= _config.MessageIntervalSeconds && _pendingQuestions.Count == 0)
        {
            await GenerateNextMessageAsync(ct);
        }

        // Send pending question
        if (_pendingQuestions.TryDequeue(out string? question))
        {
            _lastMessageTime = DateTime.UtcNow;
            _sessionMessageCount++;
            _conversationHistory.Add($"User: {question}");

            OnUserMessage?.Invoke(question, _config);
            SendMessage("user_persona.message_sent", new { Message = question, SessionCount = _sessionMessageCount });

            // Check session limit
            if (_sessionMessageCount >= _config.MaxSessionMessages)
            {
                SendMessage("training.session_complete", new { TotalMessages = _sessionMessageCount });
                _sessionMessageCount = 0;

                // Optionally pause training
                if (_config.MaxSessionMessages > 0)
                {
                    _isTrainingActive = false;
                    SendMessage("training.paused", new { Reason = "Session limit reached" });
                }
            }
        }
    }

    private async Task HandleSystemResponseAsync(string response, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(response) || _conversationHistory.Count == 0) return;

        _conversationHistory.Add($"Assistant: {response[..Math.Min(500, response.Length)]}");

        // Keep history bounded
        while (_conversationHistory.Count > 20)
        {
            _conversationHistory.RemoveAt(0);
        }

        // Evaluate response quality
        double? satisfaction = null;
        if (EvaluateFunction != null)
        {
            try
            {
                string lastUserMsg = _conversationHistory.LastOrDefault(m => m.StartsWith("User:"))
                    ?.Replace("User: ", "") ?? "";
                satisfaction = await EvaluateFunction(lastUserMsg, response, ct);
            }
            catch (Exception)
            {
                // Ignore evaluation errors
            }
        }

        // Record interaction
        string? lastQuestion = _conversationHistory.LastOrDefault(m => m.StartsWith("User:"))?.Replace("User: ", "");
        if (!string.IsNullOrEmpty(lastQuestion))
        {
            TrainingInteraction interaction = new TrainingInteraction(
                Guid.NewGuid(),
                lastQuestion,
                response,
                DateTime.UtcNow,
                satisfaction,
                null,
                new Dictionary<string, object>
                {
                    ["topic"] = _currentTopic ?? "general",
                    ["followUpDepth"] = _followUpDepth,
                    ["sessionMessage"] = _sessionMessageCount
                });

            _interactions.Add(interaction);

            // Keep interactions bounded
            while (_interactions.Count > 1000)
            {
                _interactions.RemoveAt(0);
            }

            SendMessage("training.interaction_recorded", new
            {
                interaction.Id,
                Satisfaction = satisfaction,
                Topic = _currentTopic,
                TotalInteractions = _interactions.Count
            });
        }
    }

    private async Task TickTrainingAsync(CancellationToken ct)
    {
        // Periodic analysis of training progress
        if (_interactions.Count > 0 && _interactions.Count % 10 == 0)
        {
            double recentSatisfaction = _interactions.TakeLast(10)
                .Where(i => i.UserSatisfaction.HasValue)
                .Select(i => i.UserSatisfaction!.Value)
                .DefaultIfEmpty(0)
                .Average();

            SendMessage("training.progress", new
            {
                TotalInteractions = _interactions.Count,
                RecentAverageSatisfaction = recentSatisfaction,
                CurrentTopic = _currentTopic,
                SessionProgress = $"{_sessionMessageCount}/{_config.MaxSessionMessages}"
            });
        }

        await Task.CompletedTask;
    }
}