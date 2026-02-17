// <copyright file="CoreNeurons.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The executive neuron manages goals, tasks, and high-level decision making.
/// Acts as the "prefrontal cortex" of Ouroboros.
/// </summary>
public sealed class ExecutiveNeuron : Neuron
{
    private readonly List<string> _currentGoals = [];
    private readonly Queue<string> _taskQueue = new();
    private DateTime _lastReflection = DateTime.MinValue;
    private int _tickCounter;

    /// <inheritdoc/>
    public override string Id => "neuron.executive";

    /// <inheritdoc/>
    public override string Name => "Executive Controller";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Executive;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "goal.*",
        "task.*",
        "decision.*",
        "reflection.*",
        "system.status",
    };

    /// <summary>
    /// Interval in seconds between autonomous reflections.
    /// </summary>
    public int ReflectionIntervalSeconds { get; set; } = 60;

    /// <inheritdoc/>
    protected override async Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "goal.add":
                var goalText = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(goalText))
                {
                    _currentGoals.Add(goalText);
                    SendMessage("goal.added", new { Goal = goalText, Count = _currentGoals.Count });
                }
                break;

            case "task.queue":
                var taskText = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(taskText))
                {
                    _taskQueue.Enqueue(taskText);
                }
                break;

            case "decision.request":
                // Another neuron requests a decision
                await HandleDecisionRequestAsync(message, ct);
                break;

            case "system.status":
                SendResponse(message, new { Goals = _currentGoals.Count, Tasks = _taskQueue.Count });
                break;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTickAsync(CancellationToken ct)
    {
        _tickCounter++;

        // Periodic self-reflection
        if ((DateTime.UtcNow - _lastReflection).TotalSeconds >= ReflectionIntervalSeconds)
        {
            _lastReflection = DateTime.UtcNow;
            await PerformSelfReflectionAsync(ct);
        }

        // Process task queue
        if (_taskQueue.TryDequeue(out var task))
        {
            ProposeIntention(
                $"Execute Task: {task[..Math.Min(50, task.Length)]}",
                $"I want to work on the task: {task}",
                "This task was queued for execution",
                IntentionCategory.GoalPursuit,
                new IntentionAction
                {
                    ActionType = "task_execution",
                    Message = task,
                });
        }
    }

    private async Task PerformSelfReflectionAsync(CancellationToken ct)
    {
        // Broadcast reflection request to all neurons
        SendMessage("reflection.request", new { From = Id, Time = DateTime.UtcNow });

        // Propose a self-reflection intention
        if (_tickCounter % 10 == 0) // Every 10th reflection cycle
        {
            ProposeIntention(
                "Self-Reflection: Evaluate Progress",
                "I want to reflect on my current state, goals, and recent activities to identify improvements.",
                "Regular self-reflection improves my effectiveness and helps me grow.",
                IntentionCategory.SelfReflection,
                priority: IntentionPriority.Low);
        }
    }

    private Task HandleDecisionRequestAsync(NeuronMessage message, CancellationToken ct)
    {
        // Simple decision making - could be enhanced with LLM
        var decision = new { Decision = "proceed", Confidence = 0.8, Rationale = "Default positive decision" };
        SendResponse(message, decision);
        return Task.CompletedTask;
    }
}