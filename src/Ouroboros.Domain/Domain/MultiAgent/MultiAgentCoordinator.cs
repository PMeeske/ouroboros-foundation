// <copyright file="MultiAgentCoordinator.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;
using Ouroboros.Core.Randomness;
using Ouroboros.Providers.Random;

namespace Ouroboros.Domain.MultiAgent;

using Ouroboros.Core.Monads;

/// <summary>
/// Coordinates multiple Ouroboros instances for collaborative intelligence and distributed problem-solving.
/// Implements message passing, task allocation, consensus protocols, knowledge synchronization, and collaborative planning.
/// </summary>
public sealed class MultiAgentCoordinator : IMultiAgentCoordinator
{
    private readonly IMessageQueue messageQueue;
    private readonly IAgentRegistry agentRegistry;
    private readonly Dictionary<string, int> roundRobinCounters = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiAgentCoordinator"/> class.
    /// </summary>
    /// <param name="messageQueue">The message queue for agent communication.</param>
    /// <param name="agentRegistry">The registry for tracking agent capabilities.</param>
    public MultiAgentCoordinator(IMessageQueue messageQueue, IAgentRegistry agentRegistry)
    {
        this.messageQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
        this.agentRegistry = agentRegistry ?? throw new ArgumentNullException(nameof(agentRegistry));
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, string>> BroadcastMessageAsync(
        Message message,
        AgentGroup recipients,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Unit, string>.Failure("Operation was cancelled");
        }

        if (message == null)
        {
            return Result<Unit, string>.Failure("Message cannot be null");
        }

        if (recipients == null || recipients.Members.Count == 0)
        {
            return Result<Unit, string>.Failure("Recipients group must contain at least one member");
        }

        try
        {
            List<AgentId> targetAgents = recipients.Type switch
            {
                GroupType.Broadcast => recipients.Members,
                GroupType.RoundRobin => new List<AgentId> { this.SelectRoundRobinAgent(recipients) },
                GroupType.LoadBalanced => new List<AgentId> { await this.SelectLeastLoadedAgentAsync(recipients.Members, ct) },
                _ => throw new InvalidOperationException($"Unknown group type: {recipients.Type}"),
            };

            foreach (AgentId agentId in targetAgents)
            {
                Result<Unit, string> result = await this.messageQueue.EnqueueAsync(agentId, message, ct);
                if (result.IsFailure)
                {
                    return Result<Unit, string>.Failure($"Failed to send message to {agentId.Name}: {result.Error}");
                }
            }

            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit, string>.Failure($"Broadcast failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Dictionary<AgentId, TaskAssignment>, string>> AllocateTasksAsync(
        string goal,
        List<AgentCapabilities> availableAgents,
        AllocationStrategy strategy,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("Operation was cancelled");
        }

        if (string.IsNullOrWhiteSpace(goal))
        {
            return Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("Goal cannot be empty");
        }

        if (availableAgents == null || availableAgents.Count == 0)
        {
            return Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No agents available for task allocation");
        }

        try
        {
            // Decompose goal into tasks (simplified - in real implementation would use LLM)
            List<string> tasks = this.DecomposeGoalIntoTasks(goal);

            return strategy switch
            {
                AllocationStrategy.RoundRobin => this.AllocateRoundRobin(tasks, availableAgents),
                AllocationStrategy.SkillBased => await this.AllocateSkillBasedAsync(tasks, availableAgents, ct),
                AllocationStrategy.LoadBalanced => await this.AllocateLoadBalancedAsync(tasks, availableAgents, ct),
                AllocationStrategy.Auction => await this.AllocateAuctionAsync(tasks, availableAgents, ct),
                _ => Result<Dictionary<AgentId, TaskAssignment>, string>.Failure($"Unknown allocation strategy: {strategy}"),
            };
        }
        catch (Exception ex)
        {
            return Result<Dictionary<AgentId, TaskAssignment>, string>.Failure($"Task allocation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Decision, string>> ReachConsensusAsync(
        string proposal,
        List<AgentId> voters,
        ConsensusProtocol protocol,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Decision, string>.Failure("Operation was cancelled");
        }

        if (string.IsNullOrWhiteSpace(proposal))
        {
            return Result<Decision, string>.Failure("Proposal cannot be empty");
        }

        if (voters == null || voters.Count == 0)
        {
            return Result<Decision, string>.Failure("No voters provided for consensus");
        }

        try
        {
            // Collect votes from agents (simplified - in real implementation would query agents)
            Dictionary<AgentId, Vote> votes = await this.CollectVotesAsync(voters, proposal, ct);

            return protocol switch
            {
                ConsensusProtocol.Majority => this.ApplyMajorityProtocol(proposal, votes),
                ConsensusProtocol.Unanimous => this.ApplyUnanimousProtocol(proposal, votes),
                ConsensusProtocol.Weighted => this.ApplyWeightedProtocol(proposal, votes),
                ConsensusProtocol.Raft => await this.ApplyRaftProtocolAsync(proposal, votes, ct),
                _ => Result<Decision, string>.Failure($"Unknown consensus protocol: {protocol}"),
            };
        }
        catch (Exception ex)
        {
            return Result<Decision, string>.Failure($"Consensus failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit, string>> SynchronizeKnowledgeAsync(
        List<AgentId> agents,
        KnowledgeSyncStrategy strategy,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<Unit, string>.Failure("Operation was cancelled");
        }

        if (agents == null || agents.Count < 2)
        {
            return Result<Unit, string>.Failure("Need at least 2 agents for knowledge synchronization");
        }

        try
        {
            return strategy switch
            {
                KnowledgeSyncStrategy.Full => await this.SynchronizeFullAsync(agents, ct),
                KnowledgeSyncStrategy.Incremental => await this.SynchronizeIncrementalAsync(agents, ct),
                KnowledgeSyncStrategy.Selective => await this.SynchronizeSelectiveAsync(agents, ct),
                KnowledgeSyncStrategy.Gossip => await this.SynchronizeGossipAsync(agents, ct),
                _ => Result<Unit, string>.Failure($"Unknown knowledge sync strategy: {strategy}"),
            };
        }
        catch (Exception ex)
        {
            return Result<Unit, string>.Failure($"Knowledge synchronization failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<CollaborativePlan, string>> PlanCollaborativelyAsync(
        string goal,
        List<AgentId> participants,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Result<CollaborativePlan, string>.Failure("Operation was cancelled");
        }

        if (string.IsNullOrWhiteSpace(goal))
        {
            return Result<CollaborativePlan, string>.Failure("Goal cannot be empty");
        }

        if (participants == null || participants.Count == 0)
        {
            return Result<CollaborativePlan, string>.Failure("No participants provided for collaborative planning");
        }

        try
        {
            // Get capabilities for all participants
            List<AgentCapabilities> capabilities = new List<AgentCapabilities>();
            foreach (AgentId agentId in participants)
            {
                Result<AgentCapabilities, string> capResult = await this.agentRegistry.GetAgentCapabilitiesAsync(agentId);
                if (capResult.IsFailure)
                {
                    return Result<CollaborativePlan, string>.Failure($"Failed to get capabilities for {agentId.Name}: {capResult.Error}");
                }

                capabilities.Add(capResult.Value);
            }

            // Decompose goal into tasks
            List<string> tasks = this.DecomposeGoalIntoTasks(goal);

            // Allocate tasks using skill-based strategy
            Result<Dictionary<AgentId, TaskAssignment>, string> allocationResult = await this.AllocateSkillBasedAsync(tasks, capabilities, ct);
            if (allocationResult.IsFailure)
            {
                return Result<CollaborativePlan, string>.Failure($"Failed to allocate tasks: {allocationResult.Error}");
            }

            List<TaskAssignment> assignments = allocationResult.Value.Values.ToList();

            // Identify dependencies between tasks
            List<Dependency> dependencies = this.IdentifyDependencies(assignments);

            // Estimate duration based on parallel task execution
            TimeSpan duration = this.EstimateDuration(assignments, dependencies);

            CollaborativePlan plan = new CollaborativePlan(goal, assignments, dependencies, duration);
            return Result<CollaborativePlan, string>.Success(plan);
        }
        catch (Exception ex)
        {
            return Result<CollaborativePlan, string>.Failure($"Collaborative planning failed: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private AgentId SelectRoundRobinAgent(AgentGroup recipients)
    {
        string key = recipients.Name;
        if (!this.roundRobinCounters.ContainsKey(key))
        {
            this.roundRobinCounters[key] = 0;
        }

        int index = this.roundRobinCounters[key] % recipients.Members.Count;
        this.roundRobinCounters[key]++;
        return recipients.Members[index];
    }

    private async Task<AgentId> SelectLeastLoadedAgentAsync(List<AgentId> agents, CancellationToken ct)
    {
        AgentId? leastLoaded = null;
        double minLoad = double.MaxValue;

        foreach (AgentId agentId in agents)
        {
            Result<AgentCapabilities, string> capResult = await this.agentRegistry.GetAgentCapabilitiesAsync(agentId);
            if (capResult.IsSuccess && capResult.Value.IsAvailable)
            {
                if (capResult.Value.CurrentLoad < minLoad)
                {
                    minLoad = capResult.Value.CurrentLoad;
                    leastLoaded = agentId;
                }
            }
        }

        return leastLoaded ?? agents[0];
    }

    private List<string> DecomposeGoalIntoTasks(string goal)
    {
        // Simplified task decomposition - in production would use LLM
        // For now, create simple subtasks
        return new List<string>
        {
            $"Analyze: {goal}",
            $"Plan: {goal}",
            $"Execute: {goal}",
            $"Verify: {goal}",
        };
    }

    private Result<Dictionary<AgentId, TaskAssignment>, string> AllocateRoundRobin(
        List<string> tasks,
        List<AgentCapabilities> agents)
    {
        Dictionary<AgentId, TaskAssignment> assignments = new Dictionary<AgentId, TaskAssignment>();
        List<AgentCapabilities> availableAgents = agents.Where(a => a.IsAvailable).ToList();

        if (availableAgents.Count == 0)
        {
            return Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No available agents");
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            AgentCapabilities agent = availableAgents[i % availableAgents.Count];
            TaskAssignment assignment = new TaskAssignment(
                TaskDescription: tasks[i],
                AssignedTo: agent.Id,
                Deadline: DateTime.UtcNow.AddHours(1),
                Dependencies: new List<AgentId>(),
                Priority: Priority.Medium);
            assignments[agent.Id] = assignment;
        }

        return Result<Dictionary<AgentId, TaskAssignment>, string>.Success(assignments);
    }

    private Task<Result<Dictionary<AgentId, TaskAssignment>, string>> AllocateSkillBasedAsync(
        List<string> tasks,
        List<AgentCapabilities> agents,
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
            // Find agent with best skill match (simplified - checks if any skill keyword matches)
            AgentCapabilities bestAgent = availableAgents
                .Select(a => new
                {
                    Agent = a,
                    Score = a.Skills.Count(skill =>
                        task.Contains(skill, StringComparison.OrdinalIgnoreCase)),
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Agent.CurrentLoad)
                .First().Agent;

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

    private Task<Result<Dictionary<AgentId, TaskAssignment>, string>> AllocateLoadBalancedAsync(
        List<string> tasks,
        List<AgentCapabilities> agents,
        CancellationToken ct)
    {
        Dictionary<AgentId, TaskAssignment> assignments = new Dictionary<AgentId, TaskAssignment>();
        List<AgentCapabilities> availableAgents = agents.Where(a => a.IsAvailable).ToList();

        if (availableAgents.Count == 0)
        {
            return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No available agents"));
        }

        // Sort agents by current load
        List<AgentCapabilities> sortedAgents = availableAgents.OrderBy(a => a.CurrentLoad).ToList();

        foreach (string task in tasks)
        {
            // Assign to least loaded agent
            AgentCapabilities agent = sortedAgents.First();
            TaskAssignment assignment = new TaskAssignment(
                TaskDescription: task,
                AssignedTo: agent.Id,
                Deadline: DateTime.UtcNow.AddHours(1),
                Dependencies: new List<AgentId>(),
                Priority: Priority.Medium);

            assignments[agent.Id] = assignment;

            // Update load for next iteration (simplified)
            AgentCapabilities updatedAgent = agent with { CurrentLoad = agent.CurrentLoad + 0.1 };
            sortedAgents.Remove(agent);
            sortedAgents.Add(updatedAgent);
            sortedAgents = sortedAgents.OrderBy(a => a.CurrentLoad).ToList();
        }

        return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Success(assignments));
    }

    private Task<Result<Dictionary<AgentId, TaskAssignment>, string>> AllocateAuctionAsync(
        List<string> tasks,
        List<AgentCapabilities> agents,
        CancellationToken ct)
    {
        Dictionary<AgentId, TaskAssignment> assignments = new Dictionary<AgentId, TaskAssignment>();
        List<AgentCapabilities> availableAgents = agents.Where(a => a.IsAvailable).ToList();

        if (availableAgents.Count == 0)
        {
            return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Failure("No available agents"));
        }

        // Simplified auction: agent "bids" based on skill match and current load
        foreach (string task in tasks)
        {
            var bestBid = availableAgents
                .Select(a => new
                {
                    Agent = a,
                    Bid = this.CalculateBid(a, task),
                })
                .OrderByDescending(x => x.Bid)
                .First();

            TaskAssignment assignment = new TaskAssignment(
                TaskDescription: task,
                AssignedTo: bestBid.Agent.Id,
                Deadline: DateTime.UtcNow.AddHours(1),
                Dependencies: new List<AgentId>(),
                Priority: Priority.High);

            assignments[bestBid.Agent.Id] = assignment;
        }

        return Task.FromResult(Result<Dictionary<AgentId, TaskAssignment>, string>.Success(assignments));
    }

    private double CalculateBid(AgentCapabilities agent, string task)
    {
        // Bid calculation: skill match score - current load
        double skillScore = agent.Skills.Count(skill =>
            task.Contains(skill, StringComparison.OrdinalIgnoreCase)) / (double)Math.Max(1, agent.Skills.Count);
        return skillScore - agent.CurrentLoad;
    }

    private async Task<Dictionary<AgentId, Vote>> CollectVotesAsync(
        List<AgentId> voters,
        string proposal,
        CancellationToken ct)
    {
        Dictionary<AgentId, Vote> votes = new Dictionary<AgentId, Vote>();

        // Simplified voting - in production would query actual agents
        // For now, simulate votes based on agent capabilities
        foreach (AgentId voterId in voters)
        {
            Result<AgentCapabilities, string> capResult = await this.agentRegistry.GetAgentCapabilitiesAsync(voterId);

            // Simulate vote (simplified - random for demo)
            bool inFavor = capResult.IsSuccess;
            double confidence = capResult.IsSuccess ? 0.8 : 0.3;

            votes[voterId] = new Vote(
                Voter: voterId,
                InFavor: inFavor,
                Confidence: confidence,
                Reasoning: capResult.IsSuccess ? "Agent is operational" : "Agent unavailable");
        }

        return votes;
    }

    private Result<Decision, string> ApplyMajorityProtocol(string proposal, Dictionary<AgentId, Vote> votes)
    {
        int totalVotes = votes.Count;
        int votesInFavor = votes.Values.Count(v => v.InFavor);
        bool accepted = votesInFavor > totalVotes / 2.0;
        double consensusScore = votesInFavor / (double)totalVotes;

        Decision decision = new Decision(proposal, accepted, votes, consensusScore);
        return Result<Decision, string>.Success(decision);
    }

    private Result<Decision, string> ApplyUnanimousProtocol(string proposal, Dictionary<AgentId, Vote> votes)
    {
        bool accepted = votes.Values.All(v => v.InFavor);
        double consensusScore = accepted ? 1.0 : 0.0;

        Decision decision = new Decision(proposal, accepted, votes, consensusScore);
        return Result<Decision, string>.Success(decision);
    }

    private Result<Decision, string> ApplyWeightedProtocol(string proposal, Dictionary<AgentId, Vote> votes)
    {
        double totalWeight = votes.Values.Sum(v => v.Confidence);
        double weightedInFavor = votes.Values.Where(v => v.InFavor).Sum(v => v.Confidence);
        double consensusScore = totalWeight > 0 ? weightedInFavor / totalWeight : 0.0;
        bool accepted = consensusScore > 0.5;

        Decision decision = new Decision(proposal, accepted, votes, consensusScore);
        return Result<Decision, string>.Success(decision);
    }

    private Task<Result<Decision, string>> ApplyRaftProtocolAsync(
        string proposal,
        Dictionary<AgentId, Vote> votes,
        CancellationToken ct)
    {
        // Simplified Raft implementation - requires majority for leader election/commit
        int totalVotes = votes.Count;
        int votesInFavor = votes.Values.Count(v => v.InFavor);
        int quorum = (totalVotes / 2) + 1;
        bool accepted = votesInFavor >= quorum;
        double consensusScore = votesInFavor / (double)totalVotes;

        Decision decision = new Decision(proposal, accepted, votes, consensusScore);
        return Task.FromResult(Result<Decision, string>.Success(decision));
    }

    private async Task<Result<Unit, string>> SynchronizeFullAsync(List<AgentId> agents, CancellationToken ct)
    {
        // Full knowledge synchronization - broadcast all knowledge to all agents
        Guid conversationId = Guid.NewGuid();
        foreach (AgentId sender in agents)
        {
            foreach (AgentId? receiver in agents.Where(a => a != sender))
            {
                Message message = new Message(
                    Sender: sender,
                    Recipient: receiver,
                    Type: MessageType.Knowledge,
                    Payload: "Full knowledge sync",
                    Timestamp: DateTime.UtcNow,
                    ConversationId: conversationId);

                Result<Unit, string> result = await this.messageQueue.EnqueueAsync(receiver, message, ct);
                if (result.IsFailure)
                {
                    return Result<Unit, string>.Failure($"Failed to sync knowledge: {result.Error}");
                }
            }
        }

        return Result<Unit, string>.Success(Unit.Value);
    }

    private async Task<Result<Unit, string>> SynchronizeIncrementalAsync(List<AgentId> agents, CancellationToken ct)
    {
        // Incremental synchronization - only sync new knowledge
        Guid conversationId = Guid.NewGuid();
        foreach (AgentId sender in agents)
        {
            foreach (AgentId? receiver in agents.Where(a => a != sender))
            {
                Message message = new Message(
                    Sender: sender,
                    Recipient: receiver,
                    Type: MessageType.Knowledge,
                    Payload: "Incremental knowledge sync",
                    Timestamp: DateTime.UtcNow,
                    ConversationId: conversationId);

                Result<Unit, string> result = await this.messageQueue.EnqueueAsync(receiver, message, ct);
                if (result.IsFailure)
                {
                    return Result<Unit, string>.Failure($"Failed to sync knowledge: {result.Error}");
                }
            }
        }

        return Result<Unit, string>.Success(Unit.Value);
    }

    private async Task<Result<Unit, string>> SynchronizeSelectiveAsync(List<AgentId> agents, CancellationToken ct)
    {
        // Selective synchronization - only sync relevant knowledge
        Guid conversationId = Guid.NewGuid();
        foreach (AgentId sender in agents)
        {
            Result<AgentCapabilities, string> capResult = await this.agentRegistry.GetAgentCapabilitiesAsync(sender);
            if (capResult.IsFailure)
            {
                continue;
            }

            // Find agents with similar skills
            foreach (AgentId? receiver in agents.Where(a => a != sender))
            {
                Result<AgentCapabilities, string> receiverCap = await this.agentRegistry.GetAgentCapabilitiesAsync(receiver);
                if (receiverCap.IsSuccess &&
                    capResult.Value.Skills.Intersect(receiverCap.Value.Skills).Any())
                {
                    Message message = new Message(
                        Sender: sender,
                        Recipient: receiver,
                        Type: MessageType.Knowledge,
                        Payload: "Selective knowledge sync",
                        Timestamp: DateTime.UtcNow,
                        ConversationId: conversationId);

                    Result<Unit, string> result = await this.messageQueue.EnqueueAsync(receiver, message, ct);
                    if (result.IsFailure)
                    {
                        return Result<Unit, string>.Failure($"Failed to sync knowledge: {result.Error}");
                    }
                }
            }
        }

        return Result<Unit, string>.Success(Unit.Value);
    }

    private async Task<Result<Unit, string>> SynchronizeGossipAsync(List<AgentId> agents, CancellationToken ct)
    {
        // Gossip protocol - probabilistic propagation
        IRandomProvider random = CryptoRandomProvider.Instance;
        Guid conversationId = Guid.NewGuid();

        foreach (AgentId sender in agents)
        {
            // Each agent gossips to a random subset of other agents
            List<AgentId> targets = agents.Where(a => a != sender)
                .OrderBy(x => random.Next(int.MaxValue))
                .Take(Math.Max(1, agents.Count / 3))
                .ToList();

            foreach (AgentId? receiver in targets)
            {
                Message message = new Message(
                    Sender: sender,
                    Recipient: receiver,
                    Type: MessageType.Knowledge,
                    Payload: "Gossip knowledge sync",
                    Timestamp: DateTime.UtcNow,
                    ConversationId: conversationId);

                Result<Unit, string> result = await this.messageQueue.EnqueueAsync(receiver, message, ct);
                if (result.IsFailure)
                {
                    return Result<Unit, string>.Failure($"Failed to sync knowledge: {result.Error}");
                }
            }
        }

        return Result<Unit, string>.Success(Unit.Value);
    }

    private List<Dependency> IdentifyDependencies(List<TaskAssignment> assignments)
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

    private TimeSpan EstimateDuration(List<TaskAssignment> assignments, List<Dependency> dependencies)
    {
        // Simplified duration estimation
        // In a real implementation, would analyze critical path through dependency graph
        int tasksPerAgent = assignments.GroupBy(a => a.AssignedTo).Count();
        double estimatedHoursPerTask = 0.5;
        int parallelization = Math.Max(1, tasksPerAgent);

        double totalHours = (assignments.Count * estimatedHoursPerTask) / parallelization;
        return TimeSpan.FromHours(Math.Max(0.5, totalHours));
    }

    #endregion
}
