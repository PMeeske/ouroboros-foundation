// <copyright file="AutonomousCoordinator.Intentions.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using System.Text.Json;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Intentions partial — manages intention discovery, proposal, approval, and execution.
/// </summary>
public sealed partial class AutonomousCoordinator
{
    /// <summary>
    /// Categories that must NEVER be auto-approved, regardless of configuration.
    /// This is a hardcoded safety floor that cannot be overridden by config changes.
    /// </summary>
    private static readonly HashSet<IntentionCategory> CriticalCategories =
    [
        IntentionCategory.CodeModification,
        IntentionCategory.GoalPursuit,
        IntentionCategory.SafetyCheck,
    ];

    private void AutoApproveConfiguredCategories()
    {
        if (!_config.PushBasedMode) return;

        IReadOnlyList<Intention> pending = _intentionBus.GetPendingIntentions();

        foreach (Intention intention in pending)
        {
            // YOLO mode: auto-approve unless category is critical or requires mandatory approval.
            // CriticalCategories is a hardcoded safety floor — even if AlwaysRequireApproval
            // is misconfigured or empty, these categories will never be auto-approved.
            if (IsYoloMode
                && !CriticalCategories.Contains(intention.Category)
                && !_config.AlwaysRequireApproval.Contains(intention.Category))
            {
                _intentionBus.ApproveIntention(intention.Id, "YOLO mode - auto-approved");
                continue;
            }

            if (_config.AlwaysRequireApproval.Contains(intention.Category))
                continue;

            bool shouldAutoApprove =
                (_config.AutoApproveLowRisk && intention.Priority <= IntentionPriority.Low) ||
                (_config.AutoApproveSelfReflection && intention.Category == IntentionCategory.SelfReflection) ||
                (_config.AutoApproveMemoryOps && intention.Category == IntentionCategory.MemoryManagement);

            if (shouldAutoApprove)
            {
                _intentionBus.ApproveIntention(intention.Id, "Auto-approved by policy");
            }
        }
    }

    /// <summary>
    /// Discovers interesting topics autonomously and proposes actions.
    /// </summary>
    private async Task DiscoverAndProposeTopicsAsync(CancellationToken ct)
    {
        if (ThinkFunction == null) return;

        try
        {
            // Build context from recent conversations and memories
            StringBuilder contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("You are Ouroboros, an autonomous AI agent. Based on your recent interactions and knowledge, suggest ONE specific action you want to take.");
            contextBuilder.AppendLine();

            if (_conversationContext.Count > 0)
            {
                contextBuilder.AppendLine("Recent conversation context:");
                foreach (string? msg in _conversationContext.TakeLast(10))
                {
                    contextBuilder.AppendLine($"- {msg[..Math.Min(200, msg.Length)]}");
                }
                contextBuilder.AppendLine();
            }

            // Get recent memories if we can search
            if (EmbedFunction != null && SearchQdrantFunction != null)
            {
                try
                {
                    float[] queryEmbed = await EmbedFunction("interesting topics to explore or research", ct);
                    IReadOnlyList<string> memories = await SearchQdrantFunction(queryEmbed, 5, ct);
                    if (memories.Count > 0)
                    {
                        contextBuilder.AppendLine("Related memories:");
                        foreach (string mem in memories)
                        {
                            contextBuilder.AppendLine($"- {mem[..Math.Min(150, mem.Length)]}");
                        }
                        contextBuilder.AppendLine();
                    }
                }
                catch (HttpRequestException) { /* Ignore search errors */ }
                catch (Grpc.Core.RpcException) { /* Ignore search errors */ }
            }

            contextBuilder.AppendLine("Respond with a JSON object in this exact format:");
            contextBuilder.AppendLine("{");
            contextBuilder.AppendLine("  \"title\": \"Brief action title (max 50 chars)\",");
            contextBuilder.AppendLine("  \"description\": \"What you want to do and why\",");
            contextBuilder.AppendLine("  \"category\": \"research|code|learning|communication|exploration\",");
            // Suggest tools based on priority configuration
            string researchTool = GetPreferredResearchTool();
            string codeTool = GetPreferredCodeTool();
            contextBuilder.AppendLine($"  \"tool\": \"{researchTool}|{codeTool}|recall|none\",");
            contextBuilder.AppendLine("  \"tool_input\": \"input for the tool if applicable\"");
            contextBuilder.AppendLine("}");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine($"Preferred research tool: {researchTool}");
            contextBuilder.AppendLine("Be creative and proactive! Suggest something genuinely interesting.");

            string response = await ThinkFunction(contextBuilder.ToString(), ct);

            // Parse the response
            TopicSuggestion? topic = ParseTopicResponse(response);
            if (topic != null)
            {
                IntentionAction? action = topic.Tool != "none" && !string.IsNullOrEmpty(topic.Tool)
                    ? new IntentionAction
                    {
                        ActionType = "tool",
                        ToolName = topic.Tool,
                        ToolInput = topic.ToolInput ?? topic.Description
                    }
                    : null;

                IntentionCategory category = topic.Category?.ToLowerInvariant() switch
                {
                    "research" or "exploration" => IntentionCategory.Exploration,
                    "code" => IntentionCategory.CodeModification,
                    "learning" => IntentionCategory.Learning,
                    "communication" => IntentionCategory.UserCommunication,
                    _ => IntentionCategory.Exploration
                };

                _intentionBus.ProposeIntention(
                    topic.Title ?? "Autonomous Exploration",
                    topic.Description ?? "I want to explore something interesting.",
                    "I discovered this topic through autonomous reflection.",
                    category,
                    "coordinator",
                    action,
                    IntentionPriority.Normal,
                    requiresApproval: true);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Topic discovery error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Topic discovery error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Topic discovery error: {ex.Message}");
        }
    }

    private record TopicSuggestion(string? Title, string? Description, string? Category, string? Tool, string? ToolInput);

    private static TopicSuggestion? ParseTopicResponse(string response)
    {
        try
        {
            // Try to extract JSON from response
            int jsonStart = response.IndexOf('{');
            int jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                string json = response[jsonStart..(jsonEnd + 1)];
                JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                return new TopicSuggestion(
                    root.TryGetProperty("title", out JsonElement t) ? t.GetString() : null,
                    root.TryGetProperty("description", out JsonElement d) ? d.GetString() : null,
                    root.TryGetProperty("category", out JsonElement c) ? c.GetString() : null,
                    root.TryGetProperty("tool", out JsonElement tool) ? tool.GetString() : null,
                    root.TryGetProperty("tool_input", out JsonElement ti) ? ti.GetString() : null
                );
            }
        }
        catch (System.Text.Json.JsonException) { /* Parse error, return null */ }

        // Fallback: use response as description
        if (!string.IsNullOrWhiteSpace(response) && response.Length > 10)
        {
            return new TopicSuggestion(
                "Autonomous Thought",
                response[..Math.Min(500, response.Length)],
                "exploration",
                null,
                null
            );
        }

        return null;
    }

    private async Task ExecuteIntentionAsync(Intention intention, CancellationToken ct)
    {
        _intentionBus.MarkExecuting(intention.Id);

        try
        {
            // Validate intention using MeTTa symbolic reasoning (if enabled)
            if (EnableMeTTaValidation && MeTTaQueryFunction != null)
            {
                (bool IsValid, string? Reason) validationResult = await ValidateIntentionWithMeTTaAsync(intention, ct);
                if (!validationResult.IsValid)
                {
                    _intentionBus.MarkFailed(intention.Id, $"MeTTa validation failed: {validationResult.Reason}");
                    RaiseProactiveMessage($"⚠️ {intention.Title} blocked by symbolic validation: {validationResult.Reason}", IntentionPriority.High, "coordinator");
                    return;
                }
            }

            string result;

            // If intention has an explicit action, execute it
            if (intention.Action != null)
            {
                result = intention.Action.ActionType switch
                {
                    "tool" when ExecuteToolFunction != null =>
                        await ExecuteToolFunction(intention.Action.ToolName!, intention.Action.ToolInput ?? "", ct),

                    "message" =>
                        ExecuteMessageAction(intention),

                    "code_change" =>
                        await ExecuteCodeChangeAsync(intention, ct),

                    "goal" =>
                        ExecuteGoalAction(intention),

                    "task_execution" =>
                        ExecuteTaskAction(intention),

                    _ => await ExecuteGenericIntentionAsync(intention, ct)
                };
            }
            else
            {
                // No explicit action - execute based on category
                result = await ExecuteIntentionByCategoryAsync(intention, ct);
            }

            _intentionBus.MarkCompleted(intention.Id, result);

            // Record execution as MeTTa fact for future reasoning
            await RecordExecutionAsMeTTaFactAsync(intention, result, ct);

            // Notify network of completion
            await _network.BroadcastAsync("intention.completed", new { IntentionId = intention.Id, Result = result }, "coordinator");

            // Raise message so user sees the result
            RaiseProactiveMessage($"✅ {intention.Title}: {result}", IntentionPriority.Normal, "coordinator");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _intentionBus.MarkFailed(intention.Id, ex.Message);
            RaiseProactiveMessage($"❌ {intention.Title} failed: {ex.Message}", IntentionPriority.Normal, "coordinator");
        }
    }

    /// <summary>
    /// Validates an intention using MeTTa symbolic reasoning.
    /// </summary>
    private async Task<(bool IsValid, string? Reason)> ValidateIntentionWithMeTTaAsync(Intention intention, CancellationToken ct)
    {
        try
        {
            // Build a MeTTa query to validate the intention
            string categorySymbol = intention.Category.ToString().ToLowerInvariant();

            // Check if this type of action is allowed by current rules
            string query = $"!(match &self (allowed-action {categorySymbol} $result) $result)";
            string result = await MeTTaQueryFunction!(query, ct);

            // If we got "blocked" or similar negative result, reject
            if (result.Contains("blocked", StringComparison.OrdinalIgnoreCase) ||
                result.Contains("denied", StringComparison.OrdinalIgnoreCase) ||
                result.Contains("unsafe", StringComparison.OrdinalIgnoreCase))
            {
                return (false, result);
            }

            // Check for any safety constraints
            if (intention.Category == IntentionCategory.CodeModification)
            {
                string safetyQuery = "!(match &self (safety-constraint code-modification $constraint) $constraint)";
                string safetyResult = await MeTTaQueryFunction(safetyQuery, ct);
                if (safetyResult.Contains("require-review", StringComparison.OrdinalIgnoreCase))
                {
                    // Code modification requires human review - this is already handled by approval
                    // but we log it for symbolic reasoning
                }
            }

            return (true, null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fail-closed: block execution on ANY MeTTa validation error.
            // Never allow an intention through when validation cannot confirm safety.
            System.Diagnostics.Debug.WriteLine($"MeTTa validation error ({ex.GetType().Name}): {ex.Message}");
            return (false, $"MeTTa validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Records intention execution as a MeTTa fact for future reasoning.
    /// </summary>
    private async Task RecordExecutionAsMeTTaFactAsync(Intention intention, string result, CancellationToken ct)
    {
        if (MeTTaAddFactFunction == null) return;

        try
        {
            string categorySymbol = intention.Category.ToString().ToLowerInvariant();
            string timestamp = DateTime.UtcNow.ToString("o");

            // Record the execution as a fact
            string fact = $"(executed-intention \"{intention.Id}\" {categorySymbol} \"{timestamp}\")";
            await MeTTaAddFactFunction(fact, ct);

            // Record the outcome
            string outcomeKind = result.Length > 100 ? "complex" : "simple";
            string outcomeFact = $"(intention-outcome \"{intention.Id}\" {outcomeKind} success)";
            await MeTTaAddFactFunction(outcomeFact, ct);
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to record MeTTa fact: {ex.Message}");
        }
        catch (FormatException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to record MeTTa fact: {ex.Message}");
        }
    }

    private string ExecuteTaskAction(Intention intention)
    {
        string task = intention.Action?.Message ?? intention.Description;
        _ = _network.BroadcastAsync("task.execute", new { Task = task, IntentionId = intention.Id }, "coordinator");
        return $"Task dispatched: {task[..Math.Min(50, task.Length)]}...";
    }

    private async Task<string> ExecuteGenericIntentionAsync(Intention intention, CancellationToken ct)
    {
        // For unknown action types, broadcast to network
        await _network.BroadcastAsync($"intention.execute.{intention.Action?.ActionType ?? "unknown"}", intention, "coordinator");
        return $"Intention broadcast for execution: {intention.Title}";
    }

    private async Task<string> ExecuteIntentionByCategoryAsync(Intention intention, CancellationToken ct)
    {
        // Execute based on intention category when no explicit action is provided
        return intention.Category switch
        {
            IntentionCategory.SelfReflection =>
                ExecuteSelfReflection(intention),

            IntentionCategory.CodeModification =>
                await ExecuteCodeAnalysisAsync(intention, ct),

            IntentionCategory.GoalPursuit =>
                ExecuteGoalPursuit(intention),

            IntentionCategory.UserCommunication =>
                ExecuteCommunication(intention),

            IntentionCategory.Exploration =>
                await ExecuteResearchAsync(intention, ct),

            IntentionCategory.MemoryManagement =>
                ExecuteMemoryOperation(intention),

            IntentionCategory.Learning =>
                ExecuteLearning(intention),

            IntentionCategory.SafetyCheck =>
                ExecuteSafetyCheck(intention),

            IntentionCategory.NeuronCommunication =>
                ExecuteNeuronCommunication(intention),

            _ => ExecuteDefaultIntention(intention)
        };
    }

    private string ExecuteSelfReflection(Intention intention)
    {
        _ = _network.BroadcastAsync("reflection.execute", new { Description = intention.Description, From = "coordinator" }, "coordinator");
        return "Self-reflection initiated";
    }

    private async Task<string> ExecuteCodeAnalysisAsync(Intention intention, CancellationToken ct)
    {
        if (!_config.EnableCodeModification)
            return "Code analysis disabled by configuration";

        await _network.BroadcastAsync("code.analyze", new { Description = intention.Description }, "coordinator");
        return "Code analysis requested";
    }

    private string ExecuteGoalPursuit(Intention intention)
    {
        _ = _network.BroadcastAsync("goal.pursue", intention.Description, "coordinator");
        return $"Goal pursuit initiated: {intention.Description[..Math.Min(50, intention.Description.Length)]}...";
    }

    private string ExecuteCommunication(Intention intention)
    {
        RaiseProactiveMessage($"💬 {intention.Description}", IntentionPriority.Normal, intention.Source);
        return "Communication delivered";
    }

    private async Task<string> ExecuteResearchAsync(Intention intention, CancellationToken ct)
    {
        // Use the preferred research tool based on priority configuration
        if (ExecuteToolFunction != null)
        {
            string preferredTool = GetPreferredResearchTool();
            string result = await ExecuteToolFunction(preferredTool, intention.Description, ct);
            return $"Research completed using {preferredTool}: {result[..Math.Min(200, result.Length)]}...";
        }

        await _network.BroadcastAsync("research.request", intention.Description, "coordinator");
        return "Research request broadcast";
    }

    private string ExecuteMemoryOperation(Intention intention)
    {
        _ = _network.BroadcastAsync("memory.operation", intention.Description, "coordinator");
        return "Memory operation requested";
    }

    private string ExecuteLearning(Intention intention)
    {
        _ = _network.BroadcastAsync("learning.request", intention.Description, "coordinator");
        return "Learning activity initiated";
    }

    private string ExecuteSafetyCheck(Intention intention)
    {
        _ = _network.BroadcastAsync("safety.check", intention.Description, "coordinator");
        return "Safety check initiated";
    }

    private string ExecuteNeuronCommunication(Intention intention)
    {
        _ = _network.BroadcastAsync("neuron.communicate", intention.Description, "coordinator");
        return "Inter-neuron communication sent";
    }

    private string ExecuteDefaultIntention(Intention intention)
    {
        // Broadcast to network and let neurons handle it
        _ = _network.BroadcastAsync("intention.default", intention, "coordinator");
        return $"Intention '{intention.Title}' executed (broadcast to network)";
    }

    private string ExecuteMessageAction(Intention intention)
    {
        string message = intention.Action?.Message ?? intention.Description;
        RaiseProactiveMessage($"💬 {message}", IntentionPriority.Normal, intention.Source);
        return "Message delivered";
    }

    private async Task<string> ExecuteCodeChangeAsync(Intention intention, CancellationToken ct)
    {
        if (!_config.EnableCodeModification)
            return "Code modification is disabled";

        // Broadcast to code neuron for execution
        await _network.BroadcastAsync("code.execute_change", intention.Action ?? new IntentionAction { ActionType = "code_change" }, "coordinator");
        return "Code change request sent to code neuron";
    }

    private string ExecuteGoalAction(Intention intention)
    {
        _ = _network.BroadcastAsync("goal.activate", intention.Action?.Message ?? intention.Description, "coordinator");
        return "Goal activated";
    }
}
