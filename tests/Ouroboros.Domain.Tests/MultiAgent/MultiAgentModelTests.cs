namespace Ouroboros.Tests.MultiAgent;

using Ouroboros.Domain.MultiAgent;

[Trait("Category", "Unit")]
public sealed class MultiAgentModelTests
{
    private static AgentId CreateAgentId(string name = "agent") =>
        new(Guid.NewGuid(), name);

    #region AgentId Tests

    [Fact]
    public void AgentId_WithSameValues_AreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id1 = new AgentId(guid, "Agent1");
        var id2 = new AgentId(guid, "Agent1");

        // Assert
        id1.Should().Be(id2);
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void AgentId_WithDifferentGuid_AreNotEqual()
    {
        // Arrange & Act
        var id1 = new AgentId(Guid.NewGuid(), "Agent1");
        var id2 = new AgentId(Guid.NewGuid(), "Agent1");

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void AgentId_WithDifferentName_AreNotEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id1 = new AgentId(guid, "Agent1");
        var id2 = new AgentId(guid, "Agent2");

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void AgentId_With_CreatesNewInstanceWithUpdatedValue()
    {
        // Arrange
        var original = new AgentId(Guid.NewGuid(), "Original");

        // Act
        var modified = original with { Name = "Modified" };

        // Assert
        modified.Name.Should().Be("Modified");
        modified.Value.Should().Be(original.Value);
        original.Name.Should().Be("Original");
    }

    [Fact]
    public void AgentId_ToString_ContainsProperties()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = new AgentId(guid, "TestAgent");

        // Act
        var str = id.ToString();

        // Assert
        str.Should().Contain("TestAgent");
        str.Should().Contain(guid.ToString());
    }

    #endregion

    #region AgentCapabilities Tests

    [Fact]
    public void AgentCapabilities_Constructor_SetsAllProperties()
    {
        // Arrange
        var agentId = CreateAgentId("capable");
        var skills = new List<string> { "analysis", "coding", "testing" };
        var scores = new Dictionary<string, double>
        {
            ["analysis"] = 0.95,
            ["coding"] = 0.88,
            ["testing"] = 0.72,
        };

        // Act
        var caps = new AgentCapabilities(agentId, skills, scores, 0.45, true);

        // Assert
        caps.Id.Should().Be(agentId);
        caps.Skills.Should().HaveCount(3);
        caps.Skills.Should().Contain("analysis");
        caps.PerformanceScores.Should().HaveCount(3);
        caps.PerformanceScores["analysis"].Should().Be(0.95);
        caps.CurrentLoad.Should().Be(0.45);
        caps.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void AgentCapabilities_WithUnavailableAgent_IsAvailableIsFalse()
    {
        // Act
        var caps = new AgentCapabilities(
            CreateAgentId(),
            new List<string>(),
            new Dictionary<string, double>(),
            1.0,
            false);

        // Assert
        caps.IsAvailable.Should().BeFalse();
        caps.CurrentLoad.Should().Be(1.0);
    }

    [Fact]
    public void AgentCapabilities_WithEmptySkills_HasNoSkills()
    {
        // Act
        var caps = new AgentCapabilities(
            CreateAgentId(),
            new List<string>(),
            new Dictionary<string, double>(),
            0.0,
            true);

        // Assert
        caps.Skills.Should().BeEmpty();
        caps.PerformanceScores.Should().BeEmpty();
    }

    [Fact]
    public void AgentCapabilities_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var agentId = CreateAgentId();
        var skills = new List<string> { "coding" };
        var scores = new Dictionary<string, double> { ["coding"] = 0.9 };

        // Act
        var caps1 = new AgentCapabilities(agentId, skills, scores, 0.5, true);
        var caps2 = new AgentCapabilities(agentId, skills, scores, 0.5, true);

        // Assert - records with same reference-type fields use reference equality for those fields
        caps1.Should().Be(caps2);
    }

    [Fact]
    public void AgentCapabilities_With_CreatesModifiedCopy()
    {
        // Arrange
        var caps = new AgentCapabilities(
            CreateAgentId(),
            new List<string> { "coding" },
            new Dictionary<string, double>(),
            0.3,
            true);

        // Act
        var modified = caps with { CurrentLoad = 0.8, IsAvailable = false };

        // Assert
        modified.CurrentLoad.Should().Be(0.8);
        modified.IsAvailable.Should().BeFalse();
        caps.CurrentLoad.Should().Be(0.3);
        caps.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region AgentGroup Tests

    [Fact]
    public void AgentGroup_Constructor_SetsAllProperties()
    {
        // Arrange
        var members = new List<AgentId>
        {
            CreateAgentId("m1"),
            CreateAgentId("m2"),
            CreateAgentId("m3"),
        };

        // Act
        var group = new AgentGroup("AlphaTeam", members, GroupType.Broadcast);

        // Assert
        group.Name.Should().Be("AlphaTeam");
        group.Members.Should().HaveCount(3);
        group.Type.Should().Be(GroupType.Broadcast);
    }

    [Fact]
    public void AgentGroup_WithEmptyMembers_IsAllowed()
    {
        // Act
        var group = new AgentGroup("EmptyGroup", new List<AgentId>(), GroupType.RoundRobin);

        // Assert
        group.Members.Should().BeEmpty();
    }

    [Theory]
    [InlineData(GroupType.Broadcast)]
    [InlineData(GroupType.RoundRobin)]
    [InlineData(GroupType.LoadBalanced)]
    public void AgentGroup_WithDifferentGroupTypes_SetsTypeCorrectly(GroupType type)
    {
        // Act
        var group = new AgentGroup("Group", new List<AgentId>(), type);

        // Assert
        group.Type.Should().Be(type);
    }

    #endregion

    #region Vote Tests

    [Fact]
    public void Vote_Constructor_SetsAllProperties()
    {
        // Arrange
        var voter = CreateAgentId("voter");

        // Act
        var vote = new Vote(voter, true, 0.95, "Strong agreement");

        // Assert
        vote.Voter.Should().Be(voter);
        vote.InFavor.Should().BeTrue();
        vote.Confidence.Should().Be(0.95);
        vote.Reasoning.Should().Be("Strong agreement");
    }

    [Fact]
    public void Vote_WithNullReasoning_IsAllowed()
    {
        // Act
        var vote = new Vote(CreateAgentId(), false, 0.5, null);

        // Assert
        vote.Reasoning.Should().BeNull();
    }

    [Fact]
    public void Vote_AgainstProposal_SetsInFavorFalse()
    {
        // Act
        var vote = new Vote(CreateAgentId(), false, 0.3, "Disagree with approach");

        // Assert
        vote.InFavor.Should().BeFalse();
        vote.Confidence.Should().Be(0.3);
    }

    [Fact]
    public void Vote_WithZeroConfidence_IsAllowed()
    {
        // Act
        var vote = new Vote(CreateAgentId(), true, 0.0, null);

        // Assert
        vote.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void Vote_WithMaxConfidence_IsAllowed()
    {
        // Act
        var vote = new Vote(CreateAgentId(), true, 1.0, null);

        // Assert
        vote.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Vote_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var voter = CreateAgentId();

        // Act
        var vote1 = new Vote(voter, true, 0.8, "Agree");
        var vote2 = new Vote(voter, true, 0.8, "Agree");

        // Assert
        vote1.Should().Be(vote2);
    }

    #endregion

    #region Decision Tests

    [Fact]
    public void Decision_Constructor_SetsAllProperties()
    {
        // Arrange
        var agent1 = CreateAgentId("voter1");
        var agent2 = CreateAgentId("voter2");
        var votes = new Dictionary<AgentId, Vote>
        {
            [agent1] = new Vote(agent1, true, 0.9, "Yes"),
            [agent2] = new Vote(agent2, true, 0.7, "Agreed"),
        };

        // Act
        var decision = new Decision("Deploy to prod", true, votes, 0.85);

        // Assert
        decision.Proposal.Should().Be("Deploy to prod");
        decision.Accepted.Should().BeTrue();
        decision.Votes.Should().HaveCount(2);
        decision.ConsensusScore.Should().Be(0.85);
    }

    [Fact]
    public void Decision_Rejected_HasAcceptedFalse()
    {
        // Arrange
        var agent = CreateAgentId();
        var votes = new Dictionary<AgentId, Vote>
        {
            [agent] = new Vote(agent, false, 0.9, "Too risky"),
        };

        // Act
        var decision = new Decision("Risky change", false, votes, 0.1);

        // Assert
        decision.Accepted.Should().BeFalse();
        decision.ConsensusScore.Should().Be(0.1);
    }

    [Fact]
    public void Decision_WithEmptyVotes_IsAllowed()
    {
        // Act
        var decision = new Decision(
            "No voters",
            false,
            new Dictionary<AgentId, Vote>(),
            0.0);

        // Assert
        decision.Votes.Should().BeEmpty();
    }

    #endregion

    #region TaskAssignment Tests

    [Fact]
    public void TaskAssignment_Constructor_SetsAllProperties()
    {
        // Arrange
        var agent = CreateAgentId("worker");
        var deadline = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var deps = new List<AgentId> { CreateAgentId("dep1"), CreateAgentId("dep2") };

        // Act
        var assignment = new TaskAssignment("Implement feature", agent, deadline, deps, Priority.High);

        // Assert
        assignment.TaskDescription.Should().Be("Implement feature");
        assignment.AssignedTo.Should().Be(agent);
        assignment.Deadline.Should().Be(deadline);
        assignment.Dependencies.Should().HaveCount(2);
        assignment.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public void TaskAssignment_WithNoDependencies_HasEmptyList()
    {
        // Act
        var assignment = new TaskAssignment(
            "Solo task",
            CreateAgentId(),
            DateTime.UtcNow.AddDays(1),
            new List<AgentId>(),
            Priority.Low);

        // Assert
        assignment.Dependencies.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public void TaskAssignment_AllPriorityLevels_AreAccepted(Priority priority)
    {
        // Act
        var assignment = new TaskAssignment(
            "Task",
            CreateAgentId(),
            DateTime.UtcNow,
            new List<AgentId>(),
            priority);

        // Assert
        assignment.Priority.Should().Be(priority);
    }

    #endregion

    #region Dependency Tests

    [Fact]
    public void Dependency_Constructor_SetsAllProperties()
    {
        // Act
        var dep = new Dependency("build", "test", DependencyType.BlockedBy);

        // Assert
        dep.TaskA.Should().Be("build");
        dep.TaskB.Should().Be("test");
        dep.Type.Should().Be(DependencyType.BlockedBy);
    }

    [Theory]
    [InlineData(DependencyType.BlockedBy)]
    [InlineData(DependencyType.Requires)]
    [InlineData(DependencyType.Synchronize)]
    public void Dependency_AllDependencyTypes_AreAccepted(DependencyType type)
    {
        // Act
        var dep = new Dependency("a", "b", type);

        // Assert
        dep.Type.Should().Be(type);
    }

    [Fact]
    public void Dependency_RecordEquality_WorksCorrectly()
    {
        // Act
        var dep1 = new Dependency("a", "b", DependencyType.Requires);
        var dep2 = new Dependency("a", "b", DependencyType.Requires);

        // Assert
        dep1.Should().Be(dep2);
    }

    [Fact]
    public void Dependency_DifferentType_AreNotEqual()
    {
        // Act
        var dep1 = new Dependency("a", "b", DependencyType.Requires);
        var dep2 = new Dependency("a", "b", DependencyType.BlockedBy);

        // Assert
        dep1.Should().NotBe(dep2);
    }

    #endregion

    #region Enum Value Count Tests

    [Fact]
    public void AllocationStrategy_HasFourValues()
    {
        Enum.GetValues<AllocationStrategy>().Should().HaveCount(4);
    }

    [Fact]
    public void ConsensusProtocol_HasFourValues()
    {
        Enum.GetValues<ConsensusProtocol>().Should().HaveCount(4);
    }

    [Fact]
    public void DependencyType_HasThreeValues()
    {
        Enum.GetValues<DependencyType>().Should().HaveCount(3);
    }

    [Fact]
    public void GroupType_HasThreeValues()
    {
        Enum.GetValues<GroupType>().Should().HaveCount(3);
    }

    [Fact]
    public void KnowledgeSyncStrategy_HasFourValues()
    {
        Enum.GetValues<KnowledgeSyncStrategy>().Should().HaveCount(4);
    }

    [Fact]
    public void MessageType_HasEightValues()
    {
        Enum.GetValues<MessageType>().Should().HaveCount(8);
    }

    [Fact]
    public void Priority_HasFourValues()
    {
        Enum.GetValues<Priority>().Should().HaveCount(4);
    }

    #endregion

    #region Enum Defined Tests

    [Theory]
    [InlineData(AllocationStrategy.RoundRobin)]
    [InlineData(AllocationStrategy.SkillBased)]
    [InlineData(AllocationStrategy.LoadBalanced)]
    [InlineData(AllocationStrategy.Auction)]
    public void AllocationStrategy_AllValues_AreDefined(AllocationStrategy strategy)
    {
        Enum.IsDefined(strategy).Should().BeTrue();
    }

    [Theory]
    [InlineData(ConsensusProtocol.Majority)]
    [InlineData(ConsensusProtocol.Unanimous)]
    [InlineData(ConsensusProtocol.Weighted)]
    [InlineData(ConsensusProtocol.Raft)]
    public void ConsensusProtocol_AllValues_AreDefined(ConsensusProtocol protocol)
    {
        Enum.IsDefined(protocol).Should().BeTrue();
    }

    [Theory]
    [InlineData(DependencyType.BlockedBy)]
    [InlineData(DependencyType.Requires)]
    [InlineData(DependencyType.Synchronize)]
    public void DependencyType_AllValues_AreDefined(DependencyType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(GroupType.Broadcast)]
    [InlineData(GroupType.RoundRobin)]
    [InlineData(GroupType.LoadBalanced)]
    public void GroupType_AllValues_AreDefined(GroupType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(KnowledgeSyncStrategy.Full)]
    [InlineData(KnowledgeSyncStrategy.Incremental)]
    [InlineData(KnowledgeSyncStrategy.Selective)]
    [InlineData(KnowledgeSyncStrategy.Gossip)]
    public void KnowledgeSyncStrategy_AllValues_AreDefined(KnowledgeSyncStrategy strategy)
    {
        Enum.IsDefined(strategy).Should().BeTrue();
    }

    [Theory]
    [InlineData(MessageType.Query)]
    [InlineData(MessageType.Answer)]
    [InlineData(MessageType.Proposal)]
    [InlineData(MessageType.Vote)]
    [InlineData(MessageType.Knowledge)]
    [InlineData(MessageType.Request)]
    [InlineData(MessageType.Notification)]
    [InlineData(MessageType.Heartbeat)]
    public void MessageType_AllValues_AreDefined(MessageType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public void Priority_AllValues_AreDefined(Priority priority)
    {
        Enum.IsDefined(priority).Should().BeTrue();
    }

    #endregion

    #region Enum Undefined Values Tests

    [Fact]
    public void AllocationStrategy_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((AllocationStrategy)999).Should().BeFalse();
    }

    [Fact]
    public void ConsensusProtocol_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((ConsensusProtocol)999).Should().BeFalse();
    }

    [Fact]
    public void DependencyType_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((DependencyType)999).Should().BeFalse();
    }

    [Fact]
    public void GroupType_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((GroupType)999).Should().BeFalse();
    }

    [Fact]
    public void KnowledgeSyncStrategy_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((KnowledgeSyncStrategy)999).Should().BeFalse();
    }

    [Fact]
    public void MessageType_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((MessageType)999).Should().BeFalse();
    }

    [Fact]
    public void Priority_UndefinedValue_IsNotDefined()
    {
        Enum.IsDefined((Priority)999).Should().BeFalse();
    }

    #endregion
}
