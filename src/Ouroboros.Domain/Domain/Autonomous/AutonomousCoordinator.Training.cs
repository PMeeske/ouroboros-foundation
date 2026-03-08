// <copyright file="AutonomousCoordinator.Training.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Training partial — manages auto-training lifecycle, user persona, and neural network training.
/// </summary>
public sealed partial class AutonomousCoordinator
{
    /// <summary>
    /// The user persona neuron for auto-training.
    /// </summary>
    private UserPersonaNeuron? _userPersonaNeuron;

    /// <summary>
    /// Starts auto-training mode with a simulated user.
    /// </summary>
    /// <param name="config">Optional user persona configuration.</param>
    public void StartAutoTraining(UserPersonaConfig? config = null)
    {
        Console.WriteLine("  [Coordinator] StartAutoTraining called");

        if (_userPersonaNeuron == null)
        {
            Console.WriteLine("  [Coordinator] Creating new UserPersonaNeuron...");
            _userPersonaNeuron = new UserPersonaNeuron();
            _network.RegisterNeuron(_userPersonaNeuron);

            // Wire up the user message handler with a fire-and-forget wrapper to avoid async void
            _userPersonaNeuron.OnUserMessage += (msg, cfg) =>
                HandleAutoTrainingMessageAsync(msg, cfg)
                    .ContinueWith(
                        t => System.Diagnostics.Debug.WriteLine($"Auto-training handler faulted: {t.Exception}"),
                        System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            Console.WriteLine("  [Coordinator] OnUserMessage handler wired");

            // Wire up generation function - use full chat if available and tools enabled
            _userPersonaNeuron.GenerateFunction = async (prompt, ct) =>
            {
                UserPersonaConfig config = _userPersonaNeuron.Config;

                // In problem-solving mode with tools, use full chat pipeline
                if (config.ProblemSolvingMode && config.UseTools && FullChatWithToolsFunction != null)
                {
                    Console.WriteLine("  [UserPersona] Using full chat with tools...");
                    return await FullChatWithToolsFunction(prompt, ct);
                }

                // Otherwise use basic LLM
                if (ThinkFunction != null)
                {
                    return await ThinkFunction(prompt, ct);
                }

                return "Generation function not available.";
            };
            Console.WriteLine($"  [Coordinator] GenerateFunction: {(ThinkFunction != null ? "SET (with tools support)" : "NULL")}");

            // Wire up evaluation function if we have MeTTa
            if (MeTTaQueryFunction != null)
            {
                _userPersonaNeuron.EvaluateFunction = async (question, response, ct) =>
                {
                    // Use LLM to evaluate response quality
                    if (ThinkFunction == null) return 0.5;

                    string evalPrompt = $"Rate this response from 0.0 to 1.0 based on helpfulness and accuracy.\n" +
                                     $"Question: {question}\n" +
                                     $"Response: {response[..Math.Min(500, response.Length)]}\n" +
                                     $"Reply with ONLY a decimal number between 0.0 and 1.0.";

                    try
                    {
                        string result = await ThinkFunction(evalPrompt, ct);
                        if (double.TryParse(result.Trim(), out double score))
                        {
                            return Math.Clamp(score, 0.0, 1.0);
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (HttpRequestException) { /* LLM evaluation failed */ }
                    catch (InvalidOperationException) { /* LLM evaluation failed */ }

                    return 0.5; // Default neutral score
                };
            }
        }

        Console.WriteLine("  [Coordinator] Broadcasting training.start...");

        // Configure and start - call directly instead of broadcast to ensure immediate execution
        UserPersonaConfig trainingConfig = config ?? new UserPersonaConfig();

        // Enable YOLO mode if problem-solving yolo is set
        if (trainingConfig.YoloMode)
        {
            IsYoloMode = true;
            Console.WriteLine("  [Coordinator] YOLO Mode enabled for problem-solving");
        }

        // Suppress proactive messages during problem-solving to avoid noise
        if (trainingConfig.ProblemSolvingMode)
        {
            SetSuppressProactiveMessages?.Invoke(true);
            Console.WriteLine("  [Coordinator] Suppressing proactive messages during problem-solving");
        }

        _ = Task.Run(async () =>
        {
            try
            {
                Console.WriteLine("  [Coordinator] Starting training directly on neuron...");
                await _userPersonaNeuron.StartTrainingDirectAsync(trainingConfig);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"  [Coordinator] Training start error: {ex.Message}");
            }
        });

        IsAutoTrainingActive = true;

        string modeDescription;
        if (trainingConfig.ProblemSolvingMode && !string.IsNullOrWhiteSpace(trainingConfig.Problem))
        {
            string yoloIndicator = trainingConfig.YoloMode ? "🤠 **YOLO** " : "";
            modeDescription = $"{yoloIndicator}🔧 **Problem-Solving Mode Started**\n" +
              $"**Problem:** {trainingConfig.Problem}\n" +
              $"**Deliverable:** {trainingConfig.DeliverableType}\n" +
              $"**Tools:** {(trainingConfig.UseTools ? "Enabled" : "Disabled")}\n" +
              (trainingConfig.YoloMode ? "**YOLO Mode:** All actions auto-approved!\n" : "") +
              $"**Max steps:** {trainingConfig.MaxSessionMessages}\n" +
              "User and Ouroboros will collaborate to solve this problem.\n" +
              "Use `/training stop` to end the session.";
        }
        else if (trainingConfig.SelfDialogueMode)
        {
            modeDescription = "🐍 **Self-Dialogue Mode Started**\n" +
              $"Ouroboros-A and {trainingConfig.SecondPersonaName} will now debate and explore ideas together.\n" +
              $"Message interval: {trainingConfig.MessageIntervalSeconds}s\n" +
              "Use `/training stop` to end the session.";
        }
        else
        {
            modeDescription = "🤖 **Auto-Training Mode Started**\n" +
              $"Simulated user '{trainingConfig.Name}' will now interact with me automatically.\n" +
              $"Message interval: {trainingConfig.MessageIntervalSeconds}s\n" +
              "Use `/training stop` to end the session.";
        }

        RaiseProactiveMessage(modeDescription, IntentionPriority.Normal, "coordinator");
    }

    /// <summary>
    /// Stops auto-training mode.
    /// </summary>
    public void StopAutoTraining()
    {
        if (_userPersonaNeuron != null)
        {
            // Restore YOLO mode to config default if it was enabled by problem-solving
            bool wasYoloFromTraining = _userPersonaNeuron.Config.YoloMode;
            if (wasYoloFromTraining && !_config.YoloMode)
            {
                IsYoloMode = false;
                Console.WriteLine("  [Coordinator] YOLO Mode restored to config default (off)");
            }

            // Re-enable proactive messages if they were suppressed for problem-solving
            if (_userPersonaNeuron.Config.ProblemSolvingMode)
            {
                SetSuppressProactiveMessages?.Invoke(false);
                Console.WriteLine("  [Coordinator] Proactive messages re-enabled");
            }

            _ = _network.BroadcastAsync("training.stop", null!, "coordinator");
            (int TotalInteractions, double AverageSatisfaction, int SessionMessages) stats = _userPersonaNeuron.GetStats();

            RaiseProactiveMessage(
                "🛑 **Auto-Training Mode Stopped**\n" +
                $"Total interactions: {stats.TotalInteractions}\n" +
                $"Average satisfaction: {stats.AverageSatisfaction:F2}\n" +
                $"Session messages: {stats.SessionMessages}",
                IntentionPriority.Normal, "coordinator");
        }

        IsAutoTrainingActive = false;
    }

    /// <summary>
    /// Configures the auto-training user persona.
    /// </summary>
    public void ConfigureAutoTraining(UserPersonaConfig config)
    {
        _ = _network.BroadcastAsync("training.configure", config, "coordinator");
    }

    /// <summary>
    /// Gets auto-training statistics.
    /// </summary>
    public (int TotalInteractions, double AverageSatisfaction, int SessionMessages)? GetAutoTrainingStats()
    {
        return _userPersonaNeuron?.GetStats();
    }

    private async Task HandleAutoTrainingMessageAsync(string message, UserPersonaConfig config)
    {
        if (ProcessChatFunction == null)
        {
            RaiseProactiveMessage(
                $"⚠️ Auto-training: ProcessChatFunction not configured. Message: {message}",
                IntentionPriority.Normal, "auto_training");
            return;
        }

        try
        {
            // Display the message with appropriate persona indicator
            string personaLabel = config.SelfDialogueMode
                ? $"🐍 [{config.SecondPersonaName}]"
                : $"👤 [{config.Name}]";

            string userMessage = $"{personaLabel} 💬 {message}";
            string? userPersona = config.SelfDialogueMode ? null : "User";

            // Use awaitable display+speak if available, otherwise fall back to event
            if (DisplayAndSpeakFunction != null)
            {
                // Display and wait for User TTS to complete before processing
                await DisplayAndSpeakFunction(userMessage, userPersona, _cts.Token);
            }
            else
            {
                // Fallback: fire-and-forget event (may cause overlap)
                RaiseProactiveMessage(userMessage, IntentionPriority.Normal,
                    config.SelfDialogueMode ? "self_dialogue" : "user_persona");
            }

            // Process through the main chat pipeline (User has finished speaking)
            string response = await ProcessChatFunction(message, _cts.Token);

            // Display Ouroboros's response
            string responseLabel = config.SelfDialogueMode ? "🐍 [Ouroboros-A]" : "🐍";
            string ouroResponse = $"{responseLabel} 💭 {response}";

            if (DisplayAndSpeakFunction != null)
            {
                // Display and wait for Ouroboros TTS to complete
                await DisplayAndSpeakFunction(ouroResponse, null, _cts.Token);
            }
            else
            {
                RaiseProactiveMessage(ouroResponse, IntentionPriority.Normal,
                    config.SelfDialogueMode ? "self_dialogue" : "auto_training");
            }

            // Notify the persona of the response for evaluation
            await _network.BroadcastAsync("response.generated", response, "coordinator");

            // Record the interaction
            _userPersonaNeuron?.RecordInteraction(message, response);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RaiseProactiveMessage(
                $"❌ Auto-training error: {ex.Message}",
                IntentionPriority.High, "auto_training");
        }
    }

    /// <summary>
    /// Processes training-related commands.
    /// </summary>
    private bool ProcessTrainingCommand(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string subCommand = parts.Length > 1 ? parts[1].ToLowerInvariant() : "status";

        switch (subCommand)
        {
            case "start":
                UserPersonaConfig config = new UserPersonaConfig();

                // Parse optional parameters
                for (int i = 2; i < parts.Length; i++)
                {
                    string[] param = parts[i].Split('=', 2);
                    if (param.Length == 2)
                    {
                        switch (param[0].ToLowerInvariant())
                        {
                            case "name":
                                config = config with { Name = param[1] };
                                break;
                            case "interval":
                                if (int.TryParse(param[1], out int interval))
                                    config = config with { MessageIntervalSeconds = interval };
                                break;
                            case "max":
                                if (int.TryParse(param[1], out int max))
                                    config = config with { MaxSessionMessages = max };
                                break;
                            case "skill":
                                config = config with { SkillLevel = param[1] };
                                break;
                            case "style":
                                config = config with { CommunicationStyle = param[1] };
                                break;
                            case "self":
                            case "selfdialogue":
                            case "self-dialogue":
                                config = config with
                                {
                                    SelfDialogueMode = param[1].ToLowerInvariant() is "true" or "yes" or "1" or "on",
                                    Name = "Ouroboros-B"
                                };
                                break;
                            case "persona2":
                            case "secondpersona":
                                config = config with { SecondPersonaName = param[1] };
                                break;
                            case "problem":
                                config = config with
                                {
                                    ProblemSolvingMode = true,
                                    Problem = param[1]
                                };
                                break;
                            case "deliverable":
                                config = config with { DeliverableType = param[1] };
                                break;
                            case "tools":
                                config = config with { UseTools = param[1].ToLowerInvariant() is "true" or "yes" or "1" or "on" };
                                break;
                            case "yolo":
                                config = config with { YoloMode = param[1].ToLowerInvariant() is "true" or "yes" or "1" or "on" };
                                break;
                        }
                    }
                    else if (parts[i].ToLowerInvariant() is "self" or "selfdialogue" or "self-dialogue")
                    {
                        // Allow "/training start self" shorthand
                        config = config with
                        {
                            SelfDialogueMode = true,
                            Name = "Ouroboros-B"
                        };
                    }
                    else if (parts[i].ToLowerInvariant() is "yolo")
                    {
                        // Allow "/training start problem=... yolo" shorthand
                        config = config with { YoloMode = true };
                    }
                }

                StartAutoTraining(config);
                return true;

            case "stop":
                StopAutoTraining();
                return true;

            case "status":
                (int TotalInteractions, double AverageSatisfaction, int SessionMessages)? stats = GetAutoTrainingStats();
                if (stats.HasValue)
                {
                    RaiseProactiveMessage(
                        $"📊 **Auto-Training Status**\n" +
                        $"Active: {(IsAutoTrainingActive ? "Yes ✓" : "No")}\n" +
                        $"Total interactions: {stats.Value.TotalInteractions}\n" +
                        $"Average satisfaction: {stats.Value.AverageSatisfaction:F2}\n" +
                        $"Session messages: {stats.Value.SessionMessages}",
                        IntentionPriority.Normal, "coordinator");
                }
                else
                {
                    RaiseProactiveMessage(
                        "📊 **Auto-Training Status**\n" +
                        "Not initialized. Use `/training start` to begin.",
                        IntentionPriority.Normal, "coordinator");
                }

                return true;

            case "topic":
                if (parts.Length > 2)
                {
                    string topic = string.Join(" ", parts.Skip(2));
                    _ = _network.BroadcastAsync("user_persona.set_topic", topic, "coordinator");
                    RaiseProactiveMessage($"📚 Training topic set to: {topic}", IntentionPriority.Low, "coordinator");
                }

                return true;

            case "interest":
                if (parts.Length > 2)
                {
                    string interest = string.Join(" ", parts.Skip(2));
                    _ = _network.BroadcastAsync("user_persona.add_interest", interest, "coordinator");
                    RaiseProactiveMessage($"⭐ Added training interest: {interest}", IntentionPriority.Low, "coordinator");
                }

                return true;

            default:
                RaiseProactiveMessage(
                    "📖 **Auto-Training Commands**\n\n" +
                    "`/training start [name=X] [interval=30] [max=50]`\n" +
                    "  Start auto-training with optional parameters\n\n" +
                    "`/training start self`\n" +
                    "  🐍 **Self-Dialogue**: Ouroboros debates itself\n\n" +
                    "`/training start problem=\"Write a REST API\" deliverable=code max=20`\n" +
                    "  🔧 **Problem-Solving**: Collaborate to solve a specific problem\n" +
                    "  Options: problem=<text>, deliverable=code|plan|analysis|design, tools=true|false, yolo\n\n" +
                    "`/training start problem=\"...\" yolo`\n" +
                    "  🤠 **YOLO Problem-Solving**: Auto-approve all actions, full autonomy\n\n" +
                    "`/training stop` - Stop the session\n" +
                    "`/training status` - Show statistics\n" +
                    "`/training topic <topic>` - Set current topic",
                    IntentionPriority.Normal, "coordinator");
                return true;
        }
    }

    /// <summary>
    /// Infers the appropriate deliverable type from the problem description using LLM.
    /// </summary>
    private async Task<string> InferDeliverableTypeAsync(string problem, CancellationToken ct = default)
    {
        if (ThinkFunction == null)
        {
            // Fallback to keyword-based inference
            return InferDeliverableTypeFallback(problem);
        }

        try
        {
            string prompt = $"""
                Classify this problem into exactly ONE deliverable type.

                Problem: {problem}

                Types:
                - code: Implementation, building, fixing, creating software
                - design: Architecture, schemas, diagrams, system structure
                - plan: Strategy, roadmap, steps, approach, scheduling
                - analysis: Research, evaluation, investigation, comparison
                - document: Documentation, guides, tutorials, explanations

                Reply with ONLY the type word (code, design, plan, analysis, or document).
                """;

            string result = await ThinkFunction(prompt, ct);
            string type = result.Trim().ToLowerInvariant();

            // Validate response
            if (type is "code" or "design" or "plan" or "analysis" or "document")
            {
                return type;
            }

            // If LLM returned something unexpected, fallback
            return InferDeliverableTypeFallback(problem);
        }
        catch (OperationCanceledException) { throw; }
        catch (HttpRequestException)
        {
            return InferDeliverableTypeFallback(problem);
        }
        catch (InvalidOperationException)
        {
            return InferDeliverableTypeFallback(problem);
        }
    }

    /// <summary>
    /// Keyword-based fallback for deliverable type inference.
    /// </summary>
    private static string InferDeliverableTypeFallback(string problem)
    {
        string lower = problem.ToLowerInvariant();

        if (lower.Contains("design") || lower.Contains("architect") || lower.Contains("schema"))
            return "design";
        if (lower.Contains("plan") || lower.Contains("strategy") || lower.Contains("roadmap"))
            return "plan";
        if (lower.Contains("analyze") || lower.Contains("research") || lower.Contains("why"))
            return "analysis";
        if (lower.Contains("document") || lower.Contains("explain") || lower.Contains("guide"))
            return "document";

        return "code";
    }
}
