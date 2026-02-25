// <copyright file="MultiAgentCoordinatorPipeline.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Domain.MultiAgent;

using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Monads;
using Ouroboros.Core.Steps;

/// <summary>
/// Pipeline-based multi-agent coordinator using composable arrows.
/// Demonstrates transformation from imperative to functional pipeline architecture for collaborative planning.
/// </summary>
public static class MultiAgentCoordinatorPipeline
{
    /// <summary>
    /// Represents the context for collaborative planning.
    /// </summary>
    public sealed record PlanningContext(
        string Goal,
        List<AgentId> Participants,
        IAgentRegistry AgentRegistry,
        CancellationToken CancellationToken,
        IReadOnlyList<AgentCapabilities> Capabilities,
        IReadOnlyList<string> Tasks,
        Dictionary<AgentId, TaskAssignment>? Assignments,
        IReadOnlyList<Dependency> Dependencies,
        TimeSpan EstimatedDuration);

    /// <summary>
    /// Creates a complete collaborative planning pipeline.
    /// Composes: GatherCapabilities -> DecomposeTasks -> AllocateTasks -> IdentifyDependencies -> EstimateDuration -> CreatePlan.
    /// </summary>
    /// <param name="goal">The goal to plan for</param>
    /// <param name="participants">The participating agents</param>
    /// <param name="agentRegistry">The agent registry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A step that executes collaborative planning</returns>
    public static Step<Unit, Result<CollaborativePlan, string>> CollaborativePlanningPipeline(
        string goal,
        List<AgentId> participants,
        IAgentRegistry agentRegistry,
        CancellationToken cancellationToken = default) =>
        ValidateInputArrow(goal, participants, agentRegistry, cancellationToken)
            .Then(GatherCapabilitiesArrow())
            .Then(DecomposeTasksArrow())
            .Then(AllocateTasksArrow())
            .Then(IdentifyDependenciesArrow())
            .Then(EstimateDurationArrow())
            .Map(CreatePlan);

    /// <summary>
    /// Arrow that validates input parameters and creates initial context.
    /// </summary>
    private static Step<Unit, Result<PlanningContext, string>> ValidateInputArrow(
        string goal,
        List<AgentId> participants,
        IAgentRegistry agentRegistry,
        CancellationToken cancellationToken) =>
        async _ =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(goal))
            {
                return Result<PlanningContext, string>.Failure("Goal cannot be empty");
            }

            if (participants == null || participants.Count == 0)
            {
                return Result<PlanningContext, string>.Failure("No participants provided for collaborative planning");
            }

            PlanningContext context = new PlanningContext(
                Goal: goal,
                Participants: participants,
                AgentRegistry: agentRegistry,
                CancellationToken: cancellationToken,
                Capabilities: new List<AgentCapabilities>(),
                Tasks: new List<string>(),
                Assignments: null,
                Dependencies: new List<Dependency>(),
                EstimatedDuration: TimeSpan.Zero);

            return Result<PlanningContext, string>.Success(context);
        };

    /// <summary>
    /// Arrow that gathers capabilities from all participating agents.
    /// </summary>
    private static Step<Result<PlanningContext, string>, Result<PlanningContext, string>> GatherCapabilitiesArrow() =>
        async contextResult =>
        {
            if (contextResult.IsFailure)
            {
                return contextResult;
            }

            PlanningContext context = contextResult.Value;

            try
            {
                List<AgentCapabilities> capabilities = new List<AgentCapabilities>();
                foreach (AgentId agentId in context.Participants)
                {
                    Result<AgentCapabilities, string> capResult = await context.AgentRegistry.GetAgentCapabilitiesAsync(agentId);
                    if (capResult.IsFailure)
                    {
                        return Result<PlanningContext, string>.Failure(
                            $"Failed to get capabilities for {agentId.Name}: {capResult.Error}");
                    }

                    capabilities.Add(capResult.Value);
                }

                PlanningContext updatedContext = context with { Capabilities = capabilities };
                return Result<PlanningContext, string>.Success(updatedContext);
            }
            catch (Exception ex)
            {
                return Result<PlanningContext, string>.Failure($"Failed to gather capabilities: {ex.Message}");
            }
        };

    /// <summary>
    /// Arrow that decomposes the goal into tasks.
    /// </summary>
    private static Step<Result<PlanningContext, string>, Result<PlanningContext, string>> DecomposeTasksArrow() =>
        async contextResult =>
        {
            if (contextResult.IsFailure)
            {
                return contextResult;
            }

            PlanningContext context = contextResult.Value;

            try
            {
                List<string> tasks = DecomposeGoalIntoTasks(context.Goal);
                PlanningContext updatedContext = context with { Tasks = tasks };
                return Result<PlanningContext, string>.Success(updatedContext);
            }
            catch (Exception ex)
            {
                return Result<PlanningContext, string>.Failure($"Failed to decompose tasks: {ex.Message}");
            }
        };

    /// <summary>
    /// Arrow that allocates tasks to agents using skill-based strategy.
    /// </summary>
    private static Step<Result<PlanningContext, string>, Result<PlanningContext, string>> AllocateTasksArrow() =>
        async contextResult =>
        {
            if (contextResult.IsFailure)
            {
                return contextResult;
            }

            PlanningContext context = contextResult.Value;

            try
            {
                Result<Dictionary<AgentId, TaskAssignment>, string> allocationResult = await AllocateSkillBasedAsync(context.Tasks, context.Capabilities, context.CancellationToken);
                if (allocationResult.IsFailure)
                {
                    return Result<PlanningContext, string>.Failure($"Failed to allocate tasks: {allocationResult.Error}");
                }

                PlanningContext updatedContext = context with { Assignments = allocationResult.Value };
                return Result<PlanningContext, string>.Success(updatedContext);
            }
            catch (Exception ex)
            {
                return Result<PlanningContext, string>.Failure($"Failed to allocate tasks: {ex.Message}");
            }
        };

    /// <summary>
    /// Arrow that identifies dependencies between tasks.
    /// </summary>
    private static Step<Result<PlanningContext, string>, Result<PlanningContext, string>> IdentifyDependenciesArrow() =>
        async contextResult =>
        {
            if (contextResult.IsFailure)
            {
                return contextResult;
            }

            PlanningContext context = contextResult.Value;

            try
            {
                if (context.Assignments == null)
                {
                    return Result<PlanningContext, string>.Failure("Assignments not available for dependency identification");
                }

                List<TaskAssignment> assignments = context.Assignments.Values.ToList();
                List<Dependency> dependencies = IdentifyDependencies(assignments);
                PlanningContext updatedContext = context with { Dependencies = dependencies };
                return Result<PlanningContext, string>.Success(updatedContext);
            }
            catch (Exception ex)
            {
                return Result<PlanningContext, string>.Failure($"Failed to identify dependencies: {ex.Message}");
            }
        };

    /// <summary>
    /// Arrow that estimates the duration of the plan.
    /// </summary>
    private static Step<Result<PlanningContext, string>, Result<PlanningContext, string>> EstimateDurationArrow() =>
        async contextResult =>
        {
            if (contextResult.IsFailure)
            {
                return contextResult;
            }

            PlanningContext context = contextResult.Value;

            try
            {
                if (context.Assignments == null)
                {
                    return Result<PlanningContext, string>.Failure("Assignments not available for duration estimation");
                }

                List<TaskAssignment> assignments = context.Assignments.Values.ToList();
                TimeSpan duration = EstimateDuration(assignments, context.Dependencies);
                PlanningContext updatedContext = context with { EstimatedDuration = duration };
                return Result<PlanningContext, string>.Success(updatedContext);
            }
            catch (Exception ex)
            {
                return Result<PlanningContext, string>.Failure($"Failed to estimate duration: {ex.Message}");
            }
        };

    /// <summary>
    /// Pure function that creates a CollaborativePlan from the context.
    /// </summary>
    private static Result<CollaborativePlan, string> CreatePlan(Result<PlanningContext, string> contextResult)
    {
        if (contextResult.IsFailure)
        {
            return Result<CollaborativePlan, string>.Failure(contextResult.Error);
        }

        PlanningContext context = contextResult.Value;

        if (context.Assignments == null)
        {
            return Result<CollaborativePlan, string>.Failure("Assignments not available for plan creation");
        }

        List<TaskAssignment> assignments = context.Assignments.Values.ToList();
        CollaborativePlan plan = new CollaborativePlan(
            context.Goal,
            assignments,
            context.Dependencies.ToList(),
            context.EstimatedDuration);

        return Result<CollaborativePlan, string>.Success(plan);
    }

    #region Helper Methods (extracted from MultiAgentCoordinator)

    private static List<string> DecomposeGoalIntoTasks(string goal)
    {
        // Simplified task decomposition - in production would use LLM
        return new List<string>
        {
            $"Analyze: {goal}",
            $"Plan: {goal}",
            $"Execute: {goal}",
            $"Verify: {goal}",
        };
    }

    private static Task<Result<Dictionary<AgentId, TaskAssignment>, string>> AllocateSkillBasedAsync(
        IReadOnlyList<string> tasks,
        IReadOnlyList<AgentCapabilities> agents,
        CancellationToken ct)
    {
        Dictionary<AgentId, TaskAssignment> assignments = new Dictionary<AgentId, TaskAssignment>();
        List<AgentCapabilities> availableAgents = agents.Where(a => a.IsAvailable).ToList();

        if (availableAgents.Count == 0)
        {
            return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No available agents"));
        }

        foreach (string task in tasks)
        {
            // Find agent with best skill match
            var bestMatch = availableAgents
                .Select(a => new
                {
                    Agent = a,
                    Score = a.Skills.Count(skill =>
                        task.Contains(skill, StringComparison.OrdinalIgnoreCase)),
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Agent.CurrentLoad)
                .FirstOrDefault();

            if (bestMatch == null)
            {
                return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No suitable agent found for task allocation"));
            }

            AgentCapabilities bestAgent = bestMatch.Agent;

            TaskAssignment assignment = new TaskAssignment(
                TaskDescription: task,
                AssignedTo: bestAgent.Id,
                Deadline: DateTime.UtcNow.AddHours(2),
                Dependencies: new List<AgentId>(),
                Priority: Priority.Medium);

            assignments[bestAgent.Id] = assignment;
        }

        return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Success(assignments));
    }

    private static List<Dependency> IdentifyDependencies(List<TaskAssignment> assignments)
    {
        List<Dependency> dependencies = new List<Dependency>();

        // Simple dependency detection based on task descriptions
        for (int i = 0; i < assignments.Count - 1; i++)
        {
            string taskA = assignments[i].TaskDescription;
            string taskB = assignments[i + 1].TaskDescription;

            // If taskB follows taskA sequentially, create dependency
            if (taskA.StartsWith("Analyze") && taskB.StartsWith("Plan"))
            {
                dependencies.Add(new Dependency(taskB, taskA, DependencyType.BlockedBy));
            }
            else if (taskA.StartsWith("Plan") && taskB.StartsWith("Execute"))
            {
                dependencies.Add(new Dependency(taskB, taskA, DependencyType.Requires));
            }
            else if (taskA.StartsWith("Execute") && taskB.StartsWith("Verify"))
            {
                dependencies.Add(new Dependency(taskB, taskA, DependencyType.BlockedBy));
            }
        }

        return dependencies;
    }

    private static TimeSpan EstimateDuration(List<TaskAssignment> assignments, IReadOnlyList<Dependency> dependencies)
    {
        // Simplified duration estimation
        int tasksPerAgent = assignments.GroupBy(a => a.AssignedTo).Count();
        double estimatedHoursPerTask = 0.5;
        int parallelization = Math.Max(1, tasksPerAgent);

        double totalHours = (assignments.Count * estimatedHoursPerTask) / parallelization;
        return TimeSpan.FromHours(Math.Max(0.5, totalHours));
    }

    #endregion
}
