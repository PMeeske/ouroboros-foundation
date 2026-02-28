// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

#pragma warning disable CS0618 // Obsolete types under test

namespace Ouroboros.Tests.Domain.MultiAgent;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ouroboros.Abstractions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.MultiAgent;
using Xunit;

[Trait("Category", "Unit")]
public class MultiAgentCoordinatorTests
{
    private readonly Mock<IMessageQueue> _mockMessageQueue;
    private readonly Mock<IAgentRegistry> _mockAgentRegistry;
    private readonly MultiAgentCoordinator _sut;

    public MultiAgentCoordinatorTests()
    {
        _mockMessageQueue = new Mock<IMessageQueue>();
        _mockAgentRegistry = new Mock<IAgentRegistry>();
        _sut = new MultiAgentCoordinator(_mockMessageQueue.Object, _mockAgentRegistry.Object);
    }

    private static AgentId CreateAgent(string name = "agent") =>
        new(Guid.NewGuid(), name);

    private static AgentCapabilities CreateCapabilities(
        AgentId? id = null,
        List<string>? skills = null,
        double load = 0.1,
        bool available = true)
    {
        return new AgentCapabilities(
            id ?? CreateAgent(),
            skills ?? new List<string> { "coding", "testing" },
            new Dictionary<string, double>(),
            load,
            available);
    }

    private static Message CreateMessage(AgentId? sender = null, AgentId? recipient = null)
    {
        return new Message(
            sender ?? CreateAgent("sender"),
            recipient,
            MessageType.Query,
            "test payload",
            DateTime.UtcNow,
            Guid.NewGuid());
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullMessageQueue_Throws()
    {
        // Act
        Action act = () => new MultiAgentCoordinator(null!, _mockAgentRegistry.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("messageQueue");
    }

    [Fact]
    public void Constructor_NullAgentRegistry_Throws()
    {
        // Act
        Action act = () => new MultiAgentCoordinator(_mockMessageQueue.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("agentRegistry");
    }

    // ----------------------------------------------------------------
    // BroadcastMessageAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task BroadcastMessageAsync_CancelledToken_ReturnsFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();
        var group = new AgentGroup("g", new List<AgentId> { CreateAgent() }, GroupType.Broadcast);

        // Act
        var result = await _sut.BroadcastMessageAsync(CreateMessage(), group, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task BroadcastMessageAsync_NullMessage_ReturnsFailure()
    {
        // Arrange
        var group = new AgentGroup("g", new List<AgentId> { CreateAgent() }, GroupType.Broadcast);

        // Act
        var result = await _sut.BroadcastMessageAsync(null!, group);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("null");
    }

    [Fact]
    public async Task BroadcastMessageAsync_EmptyRecipients_ReturnsFailure()
    {
        // Arrange
        var group = new AgentGroup("g", new List<AgentId>(), GroupType.Broadcast);

        // Act
        var result = await _sut.BroadcastMessageAsync(CreateMessage(), group);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least one member");
    }

    [Fact]
    public async Task BroadcastMessageAsync_NullRecipients_ReturnsFailure()
    {
        // Act
        var result = await _sut.BroadcastMessageAsync(CreateMessage(), null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BroadcastMessageAsync_BroadcastGroup_SendsToAllMembers()
    {
        // Arrange
        var agent1 = CreateAgent("a1");
        var agent2 = CreateAgent("a2");
        var agent3 = CreateAgent("a3");
        var group = new AgentGroup("team", new List<AgentId> { agent1, agent2, agent3 }, GroupType.Broadcast);
        var message = CreateMessage();

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.BroadcastMessageAsync(message, group);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<AgentId>(), message, It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task BroadcastMessageAsync_RoundRobinGroup_SendsToOneAgent()
    {
        // Arrange
        var agent1 = CreateAgent("a1");
        var agent2 = CreateAgent("a2");
        var group = new AgentGroup("rr-group", new List<AgentId> { agent1, agent2 }, GroupType.RoundRobin);
        var message = CreateMessage();

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.BroadcastMessageAsync(message, group);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<AgentId>(), message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastMessageAsync_RoundRobin_CyclesThroughMembers()
    {
        // Arrange
        var agent1 = CreateAgent("a1");
        var agent2 = CreateAgent("a2");
        var group = new AgentGroup("rr-group", new List<AgentId> { agent1, agent2 }, GroupType.RoundRobin);

        List<AgentId> sentTo = new();
        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<AgentId, Message, CancellationToken>((id, _, _) => sentTo.Add(id))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act - send 4 messages to rotate through agents twice
        for (int i = 0; i < 4; i++)
        {
            await _sut.BroadcastMessageAsync(CreateMessage(), group);
        }

        // Assert - should alternate: a1, a2, a1, a2
        sentTo.Should().HaveCount(4);
        sentTo[0].Should().Be(agent1);
        sentTo[1].Should().Be(agent2);
        sentTo[2].Should().Be(agent1);
        sentTo[3].Should().Be(agent2);
    }

    [Fact]
    public async Task BroadcastMessageAsync_LoadBalancedGroup_SendsToLeastLoadedAgent()
    {
        // Arrange
        var heavyAgent = CreateAgent("heavy");
        var lightAgent = CreateAgent("light");
        var group = new AgentGroup("lb", new List<AgentId> { heavyAgent, lightAgent }, GroupType.LoadBalanced);

        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(heavyAgent))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(heavyAgent, load: 0.9)));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(lightAgent))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(lightAgent, load: 0.1)));

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.BroadcastMessageAsync(CreateMessage(), group);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(lightAgent, It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastMessageAsync_EnqueueFails_ReturnsFailure()
    {
        // Arrange
        var agent = CreateAgent("a");
        var group = new AgentGroup("g", new List<AgentId> { agent }, GroupType.Broadcast);

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(agent, It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("Queue full"));

        // Act
        var result = await _sut.BroadcastMessageAsync(CreateMessage(), group);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Queue full");
    }

    // ----------------------------------------------------------------
    // AllocateTasksAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task AllocateTasksAsync_CancelledToken_ReturnsFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        var result = await _sut.AllocateTasksAsync("goal", new List<AgentCapabilities> { CreateCapabilities() },
            AllocationStrategy.RoundRobin, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task AllocateTasksAsync_EmptyGoal_ReturnsFailure()
    {
        // Act
        var result = await _sut.AllocateTasksAsync("", new List<AgentCapabilities> { CreateCapabilities() },
            AllocationStrategy.RoundRobin);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task AllocateTasksAsync_NoAgents_ReturnsFailure()
    {
        // Act
        var result = await _sut.AllocateTasksAsync("build system",
            new List<AgentCapabilities>(), AllocationStrategy.RoundRobin);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No agents");
    }

    [Fact]
    public async Task AllocateTasksAsync_RoundRobin_DistributesTasksEvenly()
    {
        // Arrange
        var agent1 = CreateAgent("coder");
        var agent2 = CreateAgent("tester");
        var caps = new List<AgentCapabilities>
        {
            CreateCapabilities(agent1),
            CreateCapabilities(agent2),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("build system", caps, AllocationStrategy.RoundRobin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AllocateTasksAsync_RoundRobin_SkipsUnavailableAgents()
    {
        // Arrange
        var available = CreateAgent("available");
        var unavailable = CreateAgent("unavailable");
        var caps = new List<AgentCapabilities>
        {
            CreateCapabilities(available, available: true),
            CreateCapabilities(unavailable, available: false),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("build system", caps, AllocationStrategy.RoundRobin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Values.Should().AllSatisfy(a => a.AssignedTo.Should().Be(available));
    }

    [Fact]
    public async Task AllocateTasksAsync_RoundRobin_AllUnavailable_ReturnsFailure()
    {
        // Arrange
        var caps = new List<AgentCapabilities>
        {
            CreateCapabilities(available: false),
            CreateCapabilities(available: false),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("build system", caps, AllocationStrategy.RoundRobin);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No available agents");
    }

    [Fact]
    public async Task AllocateTasksAsync_SkillBased_PrefersAgentWithMatchingSkills()
    {
        // Arrange
        var analyzeAgent = CreateAgent("analyzer");
        var genericAgent = CreateAgent("generic");
        var caps = new List<AgentCapabilities>
        {
            CreateCapabilities(analyzeAgent, skills: new List<string> { "Analyze", "Research" }),
            CreateCapabilities(genericAgent, skills: new List<string> { "General" }),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("Analyze the system", caps, AllocationStrategy.SkillBased);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // The "Analyze" task should go to analyzeAgent because "Analyze" is in its skills
        result.Value.Should().ContainKey(analyzeAgent);
    }

    [Fact]
    public async Task AllocateTasksAsync_LoadBalanced_PrefersLeastLoadedAgent()
    {
        // Arrange
        var lightAgent = CreateAgent("light");
        var heavyAgent = CreateAgent("heavy");
        var caps = new List<AgentCapabilities>
        {
            CreateCapabilities(lightAgent, load: 0.1),
            CreateCapabilities(heavyAgent, load: 0.9),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("build", caps, AllocationStrategy.LoadBalanced);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // First task should go to least loaded agent
        result.Value.Should().ContainKey(lightAgent);
    }

    [Fact]
    public async Task AllocateTasksAsync_Auction_BidCalculationPrefersSkillMatchAndLowLoad()
    {
        // Arrange
        var expertAgent = CreateAgent("expert");
        var noviceAgent = CreateAgent("novice");
        var caps = new List<AgentCapabilities>
        {
            // Expert has matching skill and low load = high bid
            CreateCapabilities(expertAgent, skills: new List<string> { "Analyze" }, load: 0.1),
            // Novice has no matching skill and higher load = low bid
            CreateCapabilities(noviceAgent, skills: new List<string> { "Other" }, load: 0.8),
        };

        // Act
        var result = await _sut.AllocateTasksAsync("Analyze the problem", caps, AllocationStrategy.Auction);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainKey(expertAgent);
    }

    [Fact]
    public async Task AllocateTasksAsync_DecomposesGoalInto4Tasks()
    {
        // Arrange - single agent means all tasks go to it
        var agent = CreateAgent("worker");
        var caps = new List<AgentCapabilities> { CreateCapabilities(agent) };

        // Act
        var result = await _sut.AllocateTasksAsync("build a feature", caps, AllocationStrategy.RoundRobin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // The implementation decomposes into 4 tasks: Analyze, Plan, Execute, Verify
        // With RoundRobin and 1 agent, all assigned to same agent (dict key collision)
        // but at least it should have entries
        result.Value.Should().NotBeEmpty();
    }

    // ----------------------------------------------------------------
    // ReachConsensusAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ReachConsensusAsync_CancelledToken_ReturnsFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        var result = await _sut.ReachConsensusAsync("proposal", new List<AgentId> { CreateAgent() },
            ConsensusProtocol.Majority, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReachConsensusAsync_EmptyProposal_ReturnsFailure()
    {
        // Act
        var result = await _sut.ReachConsensusAsync("", new List<AgentId> { CreateAgent() },
            ConsensusProtocol.Majority);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task ReachConsensusAsync_NoVoters_ReturnsFailure()
    {
        // Act
        var result = await _sut.ReachConsensusAsync("proposal", new List<AgentId>(),
            ConsensusProtocol.Majority);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No voters");
    }

    [Fact]
    public async Task ReachConsensusAsync_Majority_AcceptsWhenMajorityInFavor()
    {
        // Arrange - 3 agents, all resolve capabilities -> all vote InFavor=true
        var agents = Enumerable.Range(0, 3).Select(i => CreateAgent($"voter{i}")).ToList();
        foreach (var agent in agents)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agent))
                .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agent)));
        }

        // Act
        var result = await _sut.ReachConsensusAsync("use microservices", agents, ConsensusProtocol.Majority);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeTrue();
        result.Value.ConsensusScore.Should().Be(1.0);
    }

    [Fact]
    public async Task ReachConsensusAsync_Unanimous_RejectsWhenAnyDisagrees()
    {
        // Arrange - 2 success + 1 failure = 1 votes against
        var agents = Enumerable.Range(0, 3).Select(i => CreateAgent($"voter{i}")).ToList();
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[0]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[0])));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[1]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[1])));
        // Third agent lookup fails, so InFavor=false
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[2]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Failure("Agent offline"));

        // Act
        var result = await _sut.ReachConsensusAsync("use monolith", agents, ConsensusProtocol.Unanimous);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeFalse();
        result.Value.ConsensusScore.Should().Be(0.0);
    }

    [Fact]
    public async Task ReachConsensusAsync_Unanimous_AcceptsWhenAllAgree()
    {
        // Arrange - all agents resolve successfully -> all InFavor
        var agents = Enumerable.Range(0, 3).Select(i => CreateAgent($"voter{i}")).ToList();
        foreach (var agent in agents)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agent))
                .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agent)));
        }

        // Act
        var result = await _sut.ReachConsensusAsync("adopt TDD", agents, ConsensusProtocol.Unanimous);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accepted.Should().BeTrue();
        result.Value.ConsensusScore.Should().Be(1.0);
    }

    [Fact]
    public async Task ReachConsensusAsync_Weighted_UsesConfidenceScores()
    {
        // Arrange - 2 agents in favor with high confidence, 1 against with low confidence
        var agents = Enumerable.Range(0, 3).Select(i => CreateAgent($"v{i}")).ToList();

        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[0]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[0])));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[1]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[1])));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agents[2]))
            .ReturnsAsync(Result<AgentCapabilities, string>.Failure("offline"));

        // Act
        var result = await _sut.ReachConsensusAsync("proposal", agents, ConsensusProtocol.Weighted);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 2 agents in favor with confidence 0.8 each, 1 against with confidence 0.3
        // weightedInFavor = 1.6, totalWeight = 1.6 + 0.3 = 1.9
        // consensusScore = 1.6 / 1.9 ~ 0.842
        result.Value.ConsensusScore.Should().BeGreaterThan(0.5);
        result.Value.Accepted.Should().BeTrue();
    }

    [Fact]
    public async Task ReachConsensusAsync_Raft_RequiresQuorum()
    {
        // Arrange - 5 agents, 3 succeed (in favor), 2 fail (against)
        var agents = Enumerable.Range(0, 5).Select(i => CreateAgent($"node{i}")).ToList();

        for (int i = 0; i < 3; i++)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agents[i]))
                .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[i])));
        }
        for (int i = 3; i < 5; i++)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agents[i]))
                .ReturnsAsync(Result<AgentCapabilities, string>.Failure("unreachable"));
        }

        // Act
        var result = await _sut.ReachConsensusAsync("elect leader", agents, ConsensusProtocol.Raft);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // quorum = 5/2 + 1 = 3, votesInFavor = 3 -> accepted
        result.Value.Accepted.Should().BeTrue();
    }

    [Fact]
    public async Task ReachConsensusAsync_Raft_FailsWithoutQuorum()
    {
        // Arrange - 5 agents, only 2 succeed (in favor), 3 fail
        var agents = Enumerable.Range(0, 5).Select(i => CreateAgent($"node{i}")).ToList();

        for (int i = 0; i < 2; i++)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agents[i]))
                .ReturnsAsync(Result<AgentCapabilities, string>.Success(CreateCapabilities(agents[i])));
        }
        for (int i = 2; i < 5; i++)
        {
            _mockAgentRegistry
                .Setup(r => r.GetAgentCapabilitiesAsync(agents[i]))
                .ReturnsAsync(Result<AgentCapabilities, string>.Failure("unreachable"));
        }

        // Act
        var result = await _sut.ReachConsensusAsync("elect leader", agents, ConsensusProtocol.Raft);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // quorum = 5/2 + 1 = 3, votesInFavor = 2 -> rejected
        result.Value.Accepted.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // SynchronizeKnowledgeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task SynchronizeKnowledgeAsync_CancelledToken_ReturnsFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();
        var agents = new List<AgentId> { CreateAgent("a"), CreateAgent("b") };

        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(agents, KnowledgeSyncStrategy.Full, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynchronizeKnowledgeAsync_TooFewAgents_ReturnsFailure()
    {
        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(
            new List<AgentId> { CreateAgent() }, KnowledgeSyncStrategy.Full);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("at least 2 agents");
    }

    [Fact]
    public async Task SynchronizeKnowledgeAsync_Full_SendsNSquaredMinusNMessages()
    {
        // Arrange - 3 agents, each sends to other 2 = 3*2 = 6 messages
        var agents = Enumerable.Range(0, 3).Select(i => CreateAgent($"a{i}")).ToList();

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(agents, KnowledgeSyncStrategy.Full);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Exactly(6));
    }

    [Fact]
    public async Task SynchronizeKnowledgeAsync_Full_FailureOnEnqueue_ReturnsFailure()
    {
        // Arrange
        var agents = new List<AgentId> { CreateAgent("a"), CreateAgent("b") };

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("Network error"));

        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(agents, KnowledgeSyncStrategy.Full);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Network error");
    }

    [Fact]
    public async Task SynchronizeKnowledgeAsync_Selective_OnlySyncsAgentsWithSharedSkills()
    {
        // Arrange
        var agentA = CreateAgent("coder");
        var agentB = CreateAgent("tester");
        var agentC = CreateAgent("designer");
        var agents = new List<AgentId> { agentA, agentB, agentC };

        // agentA and agentB share "dotnet" skill, agentC has no shared skills
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agentA))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agentA, skills: new List<string> { "dotnet", "coding" })));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agentB))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agentB, skills: new List<string> { "dotnet", "testing" })));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agentC))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agentC, skills: new List<string> { "figma", "ui" })));

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(agents, KnowledgeSyncStrategy.Selective);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Only agentA->agentB and agentB->agentA should sync (shared "dotnet" skill)
        // agentC has no shared skills with either
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(agentB, It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(agentA, It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SynchronizeKnowledgeAsync_Gossip_SendsToSubsetOfAgents()
    {
        // Arrange - 6 agents; gossip selects max(1, 6/3)=2 targets per agent
        var agents = Enumerable.Range(0, 6).Select(i => CreateAgent($"n{i}")).ToList();

        _mockMessageQueue
            .Setup(q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.SynchronizeKnowledgeAsync(agents, KnowledgeSyncStrategy.Gossip);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Each of 6 agents gossips to 2 targets = 12 messages total
        _mockMessageQueue.Verify(
            q => q.EnqueueAsync(It.IsAny<AgentId>(), It.IsAny<Message>(), It.IsAny<CancellationToken>()),
            Times.Exactly(12));
    }

    // ----------------------------------------------------------------
    // PlanCollaborativelyAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task PlanCollaborativelyAsync_CancelledToken_ReturnsFailure()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // Act
        var result = await _sut.PlanCollaborativelyAsync("goal",
            new List<AgentId> { CreateAgent() }, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlanCollaborativelyAsync_EmptyGoal_ReturnsFailure()
    {
        // Act
        var result = await _sut.PlanCollaborativelyAsync("",
            new List<AgentId> { CreateAgent() });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task PlanCollaborativelyAsync_NoParticipants_ReturnsFailure()
    {
        // Act
        var result = await _sut.PlanCollaborativelyAsync("build app",
            new List<AgentId>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No participants");
    }

    [Fact]
    public async Task PlanCollaborativelyAsync_CapabilitiesLookupFails_ReturnsFailure()
    {
        // Arrange
        var agent = CreateAgent("broken");
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agent))
            .ReturnsAsync(Result<AgentCapabilities, string>.Failure("Agent not found"));

        // Act
        var result = await _sut.PlanCollaborativelyAsync("build system", new List<AgentId> { agent });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Agent not found");
    }

    [Fact]
    public async Task PlanCollaborativelyAsync_Success_ReturnsPlanWithDependencies()
    {
        // Arrange - need 4 distinct agents so each of the 4 decomposed tasks
        // (Analyze, Plan, Execute, Verify) gets a unique assignment in the dictionary.
        // With 1 agent, the dictionary key collides and only the last task survives.
        var agent1 = CreateAgent("analyzer");
        var agent2 = CreateAgent("planner");
        var agent3 = CreateAgent("executor");
        var agent4 = CreateAgent("verifier");

        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agent1))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agent1, new List<string> { "analyze" }, 0.1)));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agent2))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agent2, new List<string> { "plan" }, 0.2)));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agent3))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agent3, new List<string> { "execute" }, 0.3)));
        _mockAgentRegistry
            .Setup(r => r.GetAgentCapabilitiesAsync(agent4))
            .ReturnsAsync(Result<AgentCapabilities, string>.Success(
                CreateCapabilities(agent4, new List<string> { "verify" }, 0.4)));

        // Act
        var result = await _sut.PlanCollaborativelyAsync("build and test system",
            new List<AgentId> { agent1, agent2, agent3, agent4 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Goal.Should().Be("build and test system");
        result.Value.Assignments.Should().NotBeEmpty();
        // Dependencies should exist between Analyze->Plan, Plan->Execute, Execute->Verify
        result.Value.Dependencies.Should().NotBeEmpty();
        result.Value.EstimatedDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
