// <copyright file="AutonomousCoordinator.Commands.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using Ouroboros.Domain.Autonomous.Neurons;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Command processing partial — handles all /command style inputs.
/// </summary>
public sealed partial class AutonomousCoordinator
{
    /// <summary>
    /// Processes user input, potentially as a command.
    /// </summary>
    /// <returns>True if the input was a command, false if it should be processed normally.</returns>
    public bool ProcessCommand(string input)
    {
        string trimmed = input.Trim();

        // Approve command
        if (trimmed.StartsWith("/approve ", StringComparison.OrdinalIgnoreCase))
        {
            string id = trimmed[9..].Trim();
            bool success = _intentionBus.ApproveIntentionByPartialId(id, "User approved");
            RaiseProactiveMessage(
                success ? $"✅ Intention approved: {id}" : $"❌ Could not find pending intention: {id}",
                IntentionPriority.Normal, "coordinator");
            return true;
        }

        // Reject command
        if (trimmed.StartsWith("/reject ", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = trimmed[8..].Split(' ', 2);
            string id = parts[0];
            string? reason = parts.Length > 1 ? parts[1] : null;
            bool success = _intentionBus.RejectIntentionByPartialId(id, reason);
            RaiseProactiveMessage(
                success ? $"❌ Intention rejected: {id}" : $"Could not find pending intention: {id}",
                IntentionPriority.Normal, "coordinator");
            return true;
        }

        // Approve all low-risk
        if (trimmed.Equals("/approve-all-safe", StringComparison.OrdinalIgnoreCase))
        {
            int count = _intentionBus.ApproveAllLowRisk();
            RaiseProactiveMessage($"✅ Auto-approved {count} low-risk intentions", IntentionPriority.Normal, "coordinator");
            return true;
        }

        // List pending intentions
        if (trimmed.Equals("/intentions", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/pending", StringComparison.OrdinalIgnoreCase))
        {
            IReadOnlyList<Intention> pending = _intentionBus.GetPendingIntentions();
            if (pending.Count == 0)
            {
                RaiseProactiveMessage("📭 No pending intentions", IntentionPriority.Low, "coordinator");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"📋 **{pending.Count} Pending Intention(s)**\n");
                foreach (Intention? intention in pending.Take(10))
                {
                    sb.AppendLine($"• `{intention.Id.ToString()[..8]}` [{intention.Priority}] **{intention.Title}**");
                    sb.AppendLine($"  {intention.Description[..Math.Min(80, intention.Description.Length)]}...");
                }
                if (pending.Count > 10)
                {
                    sb.AppendLine($"\n... and {pending.Count - 10} more");
                }
                RaiseProactiveMessage(sb.ToString(), IntentionPriority.Normal, "coordinator");
            }
            return true;
        }

        // Neural network status
        if (trimmed.Equals("/network", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/neurons", StringComparison.OrdinalIgnoreCase))
        {
            RaiseProactiveMessage(_network.GetNetworkState(), IntentionPriority.Low, "coordinator");
            return true;
        }

        // Intention bus status
        if (trimmed.Equals("/bus", StringComparison.OrdinalIgnoreCase))
        {
            RaiseProactiveMessage(_intentionBus.GetSummary(), IntentionPriority.Low, "coordinator");
            return true;
        }

        // Help
        if (trimmed.Equals("/help", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/?", StringComparison.OrdinalIgnoreCase))
        {
            RaiseProactiveMessage(GetHelpText(), IntentionPriority.Normal, "coordinator");
            return true;
        }

        // Toggle push mode
        if (trimmed.Equals("/toggle-push", StringComparison.OrdinalIgnoreCase))
        {
            // Note: This would need to mutate config or use a mutable setting
            RaiseProactiveMessage(
                $"Push-based mode is currently: {(_config.PushBasedMode ? "ON" : "OFF")}",
                IntentionPriority.Normal, "coordinator");
            return true;
        }

        // YOLO mode toggle - auto-approve ALL intentions
        if (trimmed.Equals("/yolo", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/yolo on", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/yolo off", StringComparison.OrdinalIgnoreCase))
        {
            if (trimmed.Equals("/yolo on", StringComparison.OrdinalIgnoreCase))
                IsYoloMode = true;
            else if (trimmed.Equals("/yolo off", StringComparison.OrdinalIgnoreCase))
                IsYoloMode = false;
            else
                IsYoloMode = !IsYoloMode; // Toggle

            string emoji = IsYoloMode ? "🤠" : "🛡️";
            string status = IsYoloMode ? "ON - All intentions auto-approved!" : "OFF - Manual approval required";
            RaiseProactiveMessage(
                $"{emoji} **YOLO Mode**: {status}\n\n" +
                (IsYoloMode
                    ? "⚠️ All intentions will be executed without approval. Use `/yolo off` to disable."
                    : "Intentions will require approval based on your configuration."),
                IntentionPriority.High, "coordinator");

            // If we just enabled YOLO, approve all pending intentions
            if (IsYoloMode)
            {
                IReadOnlyList<Intention> pending = _intentionBus.GetPendingIntentions();
                foreach (Intention intention in pending)
                {
                    _intentionBus.ApproveIntention(intention.Id, "🤠 YOLO mode enabled - auto-approved");
                }

                if (pending.Count > 0)
                {
                    RaiseProactiveMessage(
                        $"🚀 Auto-approved {pending.Count} pending intention(s)",
                        IntentionPriority.Normal, "coordinator");
                }
            }

            return true;
        }

        // Voice-related commands are handled in AutonomousCoordinator.Voice.cs
        if (ProcessVoiceCommand(trimmed))
        {
            return true;
        }

        // YOLO + User training mode combined - fully autonomous
        if (trimmed.Equals("/yolo train", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/yolo user", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/auto", StringComparison.OrdinalIgnoreCase))
        {
            IsYoloMode = true;
            StartAutoTraining();

            RaiseProactiveMessage(
                "🤠🤖 **Full Autonomous Mode Activated!**\n\n" +
                "• YOLO mode: ON (all intentions auto-approved)\n" +
                "• Auto-training: ON (simulated user generating questions)\n\n" +
                "The system will now operate fully autonomously.\n" +
                "Use `/auto stop` or `/yolo off` to regain control.",
                IntentionPriority.Critical, "coordinator");

            return true;
        }

        // Stop full autonomous mode
        if (trimmed.Equals("/auto stop", StringComparison.OrdinalIgnoreCase))
        {
            IsYoloMode = false;
            StopAutoTraining();

            RaiseProactiveMessage(
                "🛡️ **Full Autonomous Mode Deactivated**\n\n" +
                "• YOLO mode: OFF\n" +
                "• Auto-training: OFF\n\n" +
                "Manual control restored.",
                IntentionPriority.High, "coordinator");

            return true;
        }

        // Quick problem-solving: /auto solve <problem>
        if (trimmed.StartsWith("/auto solve ", StringComparison.OrdinalIgnoreCase))
        {
            string problem = trimmed[12..].Trim(); // Remove "/auto solve "
            if (string.IsNullOrWhiteSpace(problem))
            {
                RaiseProactiveMessage(
                    "⚠️ Please provide a problem to solve.\n" +
                    "Usage: `/auto solve Build a rate limiter for our API`",
                    IntentionPriority.Normal, "coordinator");
                return true;
            }

            // Infer deliverable type from problem description using LLM
            _ = Task.Run(async () =>
            {
                string deliverable = await InferDeliverableTypeAsync(problem);
                Console.WriteLine($"  [Coordinator] Inferred deliverable type: {deliverable}");

                // Start problem-solving with YOLO + tools enabled
                UserPersonaConfig config = new UserPersonaConfig
                {
                    Name = "User",
                    ProblemSolvingMode = true,
                    Problem = problem,
                    DeliverableType = deliverable,
                    UseTools = true,
                    YoloMode = true,
                    MaxSessionMessages = 50,
                };

                StartAutoTraining(config);
            });

            return true;
        }

        // Auto-training commands
        if (trimmed.StartsWith("/training ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/training", StringComparison.OrdinalIgnoreCase))
        {
            return ProcessTrainingCommand(trimmed);
        }

        // Tool priority commands
        if (trimmed.StartsWith("/tools", StringComparison.OrdinalIgnoreCase))
        {
            return ProcessToolsCommand(trimmed);
        }

        return false;
    }

    /// <summary>
    /// Processes tool priority commands.
    /// </summary>
    private bool ProcessToolsCommand(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string subCommand = parts.Length > 1 ? parts[1].ToLowerInvariant() : "status";

        switch (subCommand)
        {
            case "status":
            case "list":
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("🔧 **Tool Priorities**\n");

                sb.AppendLine("**Research Tools** (preferred: " + GetPreferredResearchTool() + ")");
                foreach (string tool in _config.ResearchToolPriority)
                {
                    bool available = AvailableTools.Contains(tool);
                    sb.AppendLine($"  {(available ? "✅" : "❌")} {tool}");
                }

                sb.AppendLine("\n**Code Tools** (preferred: " + GetPreferredCodeTool() + ")");
                foreach (string tool in _config.CodeToolPriority)
                {
                    bool available = AvailableTools.Contains(tool);
                    sb.AppendLine($"  {(available ? "✅" : "❌")} {tool}");
                }

                sb.AppendLine("\n**General Tools** (preferred: " + GetPreferredGeneralTool() + ")");
                foreach (string tool in _config.GeneralToolPriority)
                {
                    bool available = AvailableTools.Contains(tool);
                    sb.AppendLine($"  {(available ? "✅" : "❌")} {tool}");
                }

                sb.AppendLine($"\n📊 Available tools: {AvailableTools.Count}");

                RaiseProactiveMessage(sb.ToString(), IntentionPriority.Normal, "coordinator");
                return true;

            case "available":
                StringBuilder availableSb = new StringBuilder();
                availableSb.AppendLine($"📋 **Available Tools** ({AvailableTools.Count} total)\n");
                foreach (string? tool in AvailableTools.OrderBy(t => t))
                {
                    availableSb.AppendLine($"  • {tool}");
                }

                RaiseProactiveMessage(availableSb.ToString(), IntentionPriority.Normal, "coordinator");
                return true;

            default:
                RaiseProactiveMessage(
                    "🔧 **Tool Priority Commands**\n\n" +
                    "`/tools` or `/tools status`\n" +
                    "  Show tool priorities and availability\n\n" +
                    "`/tools available`\n" +
                    "  List all available tools",
                    IntentionPriority.Normal, "coordinator");
                return true;
        }
    }

    /// <summary>
    /// Injects a goal for autonomous pursuit.
    /// </summary>
    public async Task InjectGoalAsync(string goal, IntentionPriority priority = IntentionPriority.Normal)
    {
        await _network.BroadcastAsync("goal.add", goal, "user");

        _intentionBus.ProposeIntention(
            $"Pursue Goal: {goal[..Math.Min(50, goal.Length)]}",
            $"I want to work towards the goal: {goal}",
            "This goal was provided by the user.",
            IntentionCategory.GoalPursuit,
            "user",
            new IntentionAction { ActionType = "goal", Message = goal },
            priority,
            requiresApproval: true);
    }

    /// <summary>
    /// Sends a message to a specific neuron.
    /// </summary>
    public async Task SendToNeuronAsync(string neuronId, string topic, object payload)
    {
        NeuronMessage message = new NeuronMessage
        {
            SourceNeuron = "user",
            TargetNeuron = neuronId,
            Topic = topic,
            Payload = payload,
        };
        await _network.RouteMessageAsync(message);
    }

    private static string GetHelpText()
    {
        return """
            🐍 **Ouroboros Autonomous Commands**

            **Intention Management:**
            • `/intentions` or `/pending` - List pending intentions
            • `/approve <id>` - Approve an intention
            • `/reject <id> [reason]` - Reject an intention
            • `/approve-all-safe` - Auto-approve all low-risk intentions
            • `/yolo` - Toggle YOLO mode (auto-approve ALL)
            • `/yolo on` / `/yolo off` - Explicitly set YOLO mode

            **System Status:**
            • `/network` or `/neurons` - Show neural network status
            • `/bus` - Show intention bus status
            • `/toggle-push` - Check push-mode status

            **Tool Priorities:**
            • `/tools` - Show tool priorities and availability
            • `/tools available` - List all available tools

            **Auto-Training:**
            • `/training start [options]` - Start auto-training session
            • `/training stop` - Stop training session
            • `/training status` - Show training statistics
            • `/training topic <topic>` - Set training focus topic
            • `/training interest <interest>` - Add user interest
            • `/training help` - Show training command details

            **Full Autonomous:**
            • `/auto` or `/yolo train` - Enable YOLO + auto-training
            • `/auto solve <problem>` - Start YOLO problem-solving mode
            • `/auto stop` - Disable full autonomous mode

            **Voice Control:**
            • `/voice` - Toggle voice output (TTS) on/off
            • `/voice on` / `/voice off` - Explicitly set voice output
            • `/listen` - Toggle voice input (STT) on/off
            • `/listen on` / `/listen off` - Explicitly set voice input

            **Other:**
            • `/help` or `/?` - Show this help

            **How it works:**
            In push-based mode, I will propose actions before executing them.
            Each intention shows an ID (e.g., `a1b2c3d4`).
            Use the first 4-8 characters of the ID to approve/reject.

            🤠 **YOLO Mode**: Auto-approves ALL intentions - use with caution!
            🤖 **Auto Mode**: YOLO + simulated user - fully autonomous operation!
            """;
    }
}
