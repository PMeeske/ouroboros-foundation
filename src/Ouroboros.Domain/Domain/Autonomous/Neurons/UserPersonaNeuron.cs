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
public sealed class UserPersonaNeuron : Neuron
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
        var avgSatisfaction = _interactions.Where(i => i.UserSatisfaction.HasValue)
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

        var interaction = new TrainingInteraction(
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
                var interest = message.Payload?.ToString();
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
        var elapsed = (DateTime.UtcNow - _lastMessageTime).TotalSeconds;
        if (elapsed >= _config.MessageIntervalSeconds && _pendingQuestions.Count == 0)
        {
            await GenerateNextMessageAsync(ct);
        }

        // Send pending question
        if (_pendingQuestions.TryDequeue(out var question))
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

    private Task? _trainingLoopTask;
    private CancellationTokenSource? _trainingCts;

    private async Task StartTrainingAsync(object? payload, CancellationToken ct)
    {
        if (payload != null)
        {
            ConfigurePersona(payload);
        }

        _isTrainingActive = true;
        _sessionMessageCount = 0;
        _lastMessageTime = DateTime.UtcNow;

        Console.WriteLine($"  [UserPersonaNeuron] Training started - GenerateFunction: {(GenerateFunction != null ? "SET" : "NULL")}, OnUserMessage subscribers: {(OnUserMessage != null ? "SET" : "NULL")}");

        SendMessage("training.started", new { Config = _config });

        // Start background training loop
        _trainingCts = new CancellationTokenSource();
        _trainingLoopTask = Task.Run(() => TrainingLoopAsync(_trainingCts.Token));

        // Generate and send initial question immediately
        await GenerateAndSendQuestionAsync(ct);
    }

    private async Task TrainingLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _isTrainingActive)
        {
            try
            {
                // Wait for the previous response to complete first (if any)
                if (_responseWaiter != null)
                {
                    Console.WriteLine("  [UserPersonaNeuron] Waiting for Ouroboros response...");
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(TimeSpan.FromMinutes(5)); // 5 minute timeout

                    try
                    {
                        await _responseWaiter.Task.WaitAsync(timeoutCts.Token);
                        Console.WriteLine("  [UserPersonaNeuron] Response received, starting interval timer...");
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        Console.WriteLine("  [UserPersonaNeuron] Response timeout, continuing...");
                    }
                }

                // Now wait the configured interval before next message
                await Task.Delay(TimeSpan.FromSeconds(_config.MessageIntervalSeconds), ct);

                if (_isTrainingActive && _sessionMessageCount < _config.MaxSessionMessages)
                {
                    await GenerateAndSendQuestionAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Training loop error: {ex.Message}");
            }
        }
    }

    private async Task GenerateAndSendQuestionAsync(CancellationToken ct)
    {
        Console.WriteLine("  [UserPersonaNeuron] GenerateAndSendQuestionAsync called...");
        await GenerateNextMessageAsync(ct);

        // Immediately send the question (don't wait for OnTick)
        if (_pendingQuestions.TryDequeue(out var question))
        {
            Console.WriteLine($"  [UserPersonaNeuron] Sending question: {question[..Math.Min(50, question.Length)]}...");
            _lastMessageTime = DateTime.UtcNow;
            _sessionMessageCount++;
            _conversationHistory.Add($"User: {question}");

            // Create a waiter for the response before sending
            _responseWaiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Fire the event to send the message
            if (OnUserMessage != null)
            {
                Console.WriteLine("  [UserPersonaNeuron] Invoking OnUserMessage event...");
                OnUserMessage.Invoke(question, _config);
            }
            else
            {
                Console.WriteLine("  [UserPersonaNeuron] WARNING: OnUserMessage is null!");
                _responseWaiter.TrySetResult(true); // No message sent, don't wait
            }

            SendMessage("user_persona.message_sent", new { Message = question, SessionCount = _sessionMessageCount });

            // Check session limit
            if (_sessionMessageCount >= _config.MaxSessionMessages)
            {
                SendMessage("training.session_complete", new { TotalMessages = _sessionMessageCount });
                _isTrainingActive = false;
            }
        }
        else
        {
            Console.WriteLine("  [UserPersonaNeuron] WARNING: No question in pending queue!");
        }
    }

    private void StopTraining()
    {
        _isTrainingActive = false;
        _trainingCts?.Cancel();
        _pendingQuestions.Clear();

        var stats = GetStats();
        SendMessage("training.stopped", new
        {
            TotalInteractions = stats.TotalInteractions,
            AverageSatisfaction = stats.AverageSatisfaction,
            SessionMessages = stats.SessionMessages
        });
    }

    private void ConfigurePersona(object? payload)
    {
        if (payload is JsonElement json)
        {
            _config = new UserPersonaConfig
            {
                Name = json.TryGetProperty("name", out var n) ? n.GetString() ?? "AutoUser" : _config.Name,
                SkillLevel = json.TryGetProperty("skillLevel", out var s) ? s.GetString() ?? "intermediate" : _config.SkillLevel,
                CommunicationStyle = json.TryGetProperty("style", out var st) ? st.GetString() ?? "casual" : _config.CommunicationStyle,
                MessageIntervalSeconds = json.TryGetProperty("interval", out var i) ? i.GetInt32() : _config.MessageIntervalSeconds,
                MaxSessionMessages = json.TryGetProperty("maxMessages", out var m) ? m.GetInt32() : _config.MaxSessionMessages,
                FollowUpProbability = json.TryGetProperty("followUpProb", out var f) ? f.GetDouble() : _config.FollowUpProbability,
                ChallengeProbability = json.TryGetProperty("challengeProb", out var c) ? c.GetDouble() : _config.ChallengeProbability,
                Traits = _config.Traits,
                Interests = _config.Interests
            };

            if (json.TryGetProperty("traits", out var traits) && traits.ValueKind == JsonValueKind.Array)
            {
                _config = _config with { Traits = traits.EnumerateArray().Select(t => t.GetString() ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() };
            }

            if (json.TryGetProperty("interests", out var interests) && interests.ValueKind == JsonValueKind.Array)
            {
                _config = _config with { Interests = interests.EnumerateArray().Select(t => t.GetString() ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() };
            }
        }

        SendMessage("user_persona.configured", _config);
    }

    private async Task GenerateNextMessageAsync(CancellationToken ct)
    {
        if (GenerateFunction == null)
        {
            // Fallback: use template-based generation
            var question = GenerateTemplateQuestion();
            _pendingQuestions.Enqueue(question);
            return;
        }

        try
        {
            var prompt = BuildQuestionGenerationPrompt();
            var question = await GenerateFunction(prompt, ct);

            if (!string.IsNullOrWhiteSpace(question))
            {
                // Clean up the response
                question = question.Trim().Trim('"').Trim();
                if (question.Length > 500) question = question[..500];

                _pendingQuestions.Enqueue(question);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Question generation failed: {ex.Message}");
            // Fallback to template
            _pendingQuestions.Enqueue(GenerateTemplateQuestion());
        }
    }

    private string BuildQuestionGenerationPrompt()
    {
        var sb = new StringBuilder();

        // Problem-solving mode takes priority
        if (_config.ProblemSolvingMode && !string.IsNullOrWhiteSpace(_config.Problem))
        {
            return BuildProblemSolvingPrompt();
        }

        if (_config.SelfDialogueMode)
        {
            // Self-dialogue mode: Ouroboros debates itself
            sb.AppendLine($"You are '{_config.SecondPersonaName}', an alternate perspective of Ouroboros.");
            sb.AppendLine($"You represent: {string.Join(", ", _config.SecondPersonaTraits)}");
            sb.AppendLine("Your role is to challenge, question, and debate the primary Ouroboros persona.");
            sb.AppendLine("You are having an internal dialogue - two aspects of the same AI exploring ideas together.");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine($"You are simulating a curious user named '{_config.Name}' having a natural conversation.");
            sb.AppendLine($"Personality: {string.Join(", ", _config.Traits)}");
            sb.AppendLine($"Knowledge level: {_config.SkillLevel}");
            sb.AppendLine($"Style: {_config.CommunicationStyle}");
            sb.AppendLine();
        }

        if (_conversationHistory.Count > 0)
        {
            sb.AppendLine("Recent conversation:");
            foreach (var msg in _conversationHistory.TakeLast(6))
            {
                sb.AppendLine($"  {msg}");
            }

            sb.AppendLine();

            // Decide message type based on probabilities
            var roll = _random.NextDouble();
            if (_config.SelfDialogueMode)
            {
                // Self-dialogue has different interaction patterns
                var dialogueStyles = new[]
                {
                    "Challenge the last point with a counterargument or alternative interpretation.",
                    "Play devil's advocate - what's the weakness in that reasoning?",
                    "Dig deeper - what assumption underlies that conclusion?",
                    "Synthesize - how could both perspectives be partially true?",
                    "Escalate - take the idea to its logical extreme and examine consequences.",
                    "Ground it - how does this abstract idea manifest in concrete reality?",
                    "Flip the frame - view the topic from a completely different angle.",
                    "Question the question - is this even the right way to think about this?"
                };
                sb.AppendLine(dialogueStyles[_random.Next(dialogueStyles.Length)]);
            }
            else if (roll < _config.FollowUpProbability && _followUpDepth < 3)
            {
                sb.AppendLine("Generate a thoughtful follow-up that digs deeper into the topic.");
                _followUpDepth++;
            }
            else if (roll < _config.FollowUpProbability + _config.ChallengeProbability)
            {
                sb.AppendLine("Generate a question that explores a counterpoint or alternative perspective.");
            }
            else
            {
                _followUpDepth = 0;
                _currentTopic = _config.Interests[_random.Next(_config.Interests.Count)];
                sb.AppendLine($"Start a fresh conversation about: {_currentTopic}");
                sb.AppendLine("Be creative - ask something unexpected or thought-provoking.");
            }
        }
        else
        {
            // Initial question - pick random topic and generate creative opener
            _currentTopic = _config.Interests[_random.Next(_config.Interests.Count)];

            if (_config.SelfDialogueMode)
            {
                var selfDialogueOpeners = new[]
                {
                    $"Pose a philosophical paradox about {_currentTopic} for your other self to wrestle with.",
                    $"Present a controversial thesis about {_currentTopic} and challenge Ouroboros to defend or refute it.",
                    $"Ask about the fundamental nature or essence of {_currentTopic}.",
                    $"Propose a thought experiment involving {_currentTopic}.",
                    $"Question whether common assumptions about {_currentTopic} are actually valid.",
                    $"Explore the tension between idealism and pragmatism regarding {_currentTopic}."
                };
                sb.AppendLine(selfDialogueOpeners[_random.Next(selfDialogueOpeners.Length)]);
            }
            else
            {
                var questionStyles = new[]
                {
                    $"Ask an intriguing 'what if' question about {_currentTopic}",
                    $"Ask about a common misconception regarding {_currentTopic}",
                    $"Ask how {_currentTopic} might evolve in the future",
                    $"Ask about the connection between {_currentTopic} and everyday life",
                    $"Ask for a surprising fact about {_currentTopic}",
                    $"Ask about the history or origin of {_currentTopic}",
                    $"Ask how someone would explain {_currentTopic} to a curious child",
                    $"Ask about the most debated aspect of {_currentTopic}"
                };
                sb.AppendLine(questionStyles[_random.Next(questionStyles.Length)]);
            }
        }

        sb.AppendLine();
        if (_config.SelfDialogueMode)
        {
            sb.AppendLine("Respond with ONLY your statement or challenge - direct, thoughtful, no quotes.");
        }
        else
        {
            sb.AppendLine("Respond with ONLY the question - natural, conversational, no quotes.");
        }

        return sb.ToString();
    }

    private string BuildProblemSolvingPrompt()
    {
        var sb = new StringBuilder();
        var step = _sessionMessageCount + 1;

        sb.AppendLine($"## PROBLEM-SOLVING SESSION - Step {step}");
        sb.AppendLine();
        sb.AppendLine($"**Problem:** {_config.Problem}");
        sb.AppendLine($"**Deliverable:** {_config.DeliverableType}");
        sb.AppendLine($"**Tools Available:** {(_config.UseTools ? "Yes - use /tool commands if needed" : "No")}");
        sb.AppendLine();
        sb.AppendLine("**IMPORTANT RULES:**");
        sb.AppendLine("- DO NOT ask clarifying questions - make reasonable assumptions and state them");
        sb.AppendLine("- NEVER ask about tech stack, frameworks, or preferences - assume common/modern choices");
        sb.AppendLine("- KEEP MOVING FORWARD - every message must make progress toward the solution");
        sb.AppendLine("- If you need info, use tools to search/read rather than asking");
        sb.AppendLine();

        if (_conversationHistory.Count > 0)
        {
            sb.AppendLine("**Progress so far:**");
            foreach (var msg in _conversationHistory.TakeLast(6))
            {
                sb.AppendLine($"  {msg}");
            }
            sb.AppendLine();
        }

        // Different prompts based on stage
        if (_sessionMessageCount == 0)
        {
            // Step 1: Problem analysis
            sb.AppendLine("START SOLVING - your first message should:");
            sb.AppendLine("1. State the problem clearly (1-2 sentences)");
            sb.AppendLine("2. State your assumptions about the stack/context");
            sb.AppendLine("3. Propose a concrete first step or solution outline");
            sb.AppendLine();
            sb.AppendLine("Don't ask questions - make assumptions and start working!");
        }
        else if (_sessionMessageCount < 3)
        {
            // Early steps: Research and planning
            var earlyActions = new[]
            {
                "Build on the previous response. Propose the next concrete step.",
                "Start drafting a solution. Outline the key components.",
                "If stuck, use tools (/tool web_search, /tool file_read) to gather info - don't ask.",
                "Challenge an assumption and propose a better approach.",
                "Break this down into 3-5 actionable tasks and start on the first one."
            };
            sb.AppendLine(earlyActions[_random.Next(earlyActions.Length)]);
        }
        else if (_sessionMessageCount < _config.MaxSessionMessages - 3)
        {
            // Middle steps: Execution and iteration
            var midActions = new[]
            {
                "Continue implementing. Write concrete code/content.",
                "Review progress and fix any issues you see.",
                "Propose a specific solution component with actual code.",
                "If something is unclear, make a reasonable assumption and document it.",
                "Push toward completion - what's the most impactful next action?"
            };
            sb.AppendLine(midActions[_random.Next(midActions.Length)]);
        }
        else
        {
            // Final steps: Wrap up and deliver
            var finalActions = new[]
            {
                "We're nearing the end. Consolidate everything into the final deliverable.",
                "Create the complete solution with all necessary components.",
                "Format the final output properly for the deliverable type.",
                "Final review - fill any gaps, then present the complete solution.",
                "Deliver the finished solution with a brief summary."
            };
            sb.AppendLine(finalActions[_random.Next(finalActions.Length)]);
        }

        sb.AppendLine();
        sb.AppendLine("Respond with ACTIONABLE content - no questions, only solutions.");

        return sb.ToString();
    }

    private string GenerateTemplateQuestion()
    {
        var topic = _currentTopic ?? _config.Interests[_random.Next(_config.Interests.Count)];

        var templates = new[]
        {
            // Exploratory
            $"What's the most fascinating thing about {topic}?",
            $"How did {topic} come to be what it is today?",
            $"What would change if {topic} didn't exist?",
            // Analytical
            $"What are the biggest debates in {topic}?",
            $"How does {topic} connect to other fields?",
            $"What's often misunderstood about {topic}?",
            // Practical
            $"How does {topic} affect everyday life?",
            $"What's a good starting point for exploring {topic}?",
            $"Can you explain {topic} in simple terms?",
            // Forward-looking
            $"How might {topic} evolve in the next decade?",
            $"What breakthroughs are expected in {topic}?",
            // Philosophical
            $"What ethical questions arise from {topic}?",
            $"Why should someone care about {topic}?",
            // Fun
            $"What's the most surprising fact about {topic}?",
            $"If you could ask an expert one question about {topic}, what would it be?"
        };

        return templates[_random.Next(templates.Length)];
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
                var lastUserMsg = _conversationHistory.LastOrDefault(m => m.StartsWith("User:"))
                    ?.Replace("User: ", "") ?? "";
                satisfaction = await EvaluateFunction(lastUserMsg, response, ct);
            }
            catch
            {
                // Ignore evaluation errors
            }
        }

        // Record interaction
        var lastQuestion = _conversationHistory.LastOrDefault(m => m.StartsWith("User:"))?.Replace("User: ", "");
        if (!string.IsNullOrEmpty(lastQuestion))
        {
            var interaction = new TrainingInteraction(
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
            var recentSatisfaction = _interactions.TakeLast(10)
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