namespace Ouroboros.Tests.Domain.MultiAgent;

using Ouroboros.Domain.MultiAgent;

[Trait("Category", "Unit")]
public class MultiAgentRecordTests
{
    private static AgentId CreateAgentId(string name = "agent1") =>
        new(Guid.NewGuid(), name);

    [Fact]
    public void AgentId_Constructor_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var agentId = new AgentId(id, "TestAgent");

        // Assert
        agentId.Value.Should().Be(id);
        agentId.Name.Should().Be("TestAgent");
    }

    [Fact]
    public void AgentId_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var a1 = new AgentId(id, "Same");
        var a2 = new AgentId(id, "Same");

        // Assert
        a1.Should().Be(a2);
    }

    [Fact]
    public void AgentCapabilities_Constructor_SetsAllProperties()
    {
        // Arrange
        var agentId = CreateAgentId();
        var skills = new List<string> { "coding", "testing" };
        var scores = new Dictionary<string, double> { ["coding"] = 0.9, ["testing"] = 0.7 };

        // Act
        var caps = new AgentCapabilities(agentId, skills, scores, 0.5, true);

        // Assert
        caps.Id.Should().Be(agentId);
        caps.Skills.Should().HaveCount(2);
        caps.PerformanceScores["coding"].Should().Be(0.9);
        caps.CurrentLoad.Should().Be(0.5);
        caps.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void AgentGroup_Constructor_SetsAllProperties()
    {
        // Arrange
        var members = new List<AgentId> { CreateAgentId("a1"), CreateAgentId("a2") };

        // Act
        var group = new AgentGroup("TeamAlpha", members, GroupType.Broadcast);

        // Assert
        group.Name.Should().Be("TeamAlpha");
        group.Members.Should().HaveCount(2);
        group.Type.Should().Be(GroupType.Broadcast);
    }

    [Fact]
    public void Message_Constructor_SetsAllProperties()
    {
        // Arrange
        var sender = CreateAgentId("sender");
        var recipient = CreateAgentId("recipient");
        var conversationId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        // Act
        var msg = new Message(sender, recipient, MessageType.Query, "Hello", timestamp, conversationId);

        // Assert
        msg.Sender.Should().Be(sender);
        msg.Recipient.Should().Be(recipient);
        msg.Type.Should().Be(MessageType.Query);
        msg.Payload.Should().Be("Hello");
        msg.Timestamp.Should().Be(timestamp);
        msg.ConversationId.Should().Be(conversationId);
    }

    [Fact]
    public void Message_BroadcastRecipient_IsNull()
    {
        // Act
        var msg = new Message(
            CreateAgentId(), null, MessageType.Notification,
            "broadcast", DateTime.UtcNow, Guid.NewGuid());

        // Assert
        msg.Recipient.Should().BeNull();
    }

    [Fact]
    public void Vote_Constructor_SetsAllProperties()
    {
        // Arrange
        var voter = CreateAgentId("voter1");

        // Act
        var vote = new Vote(voter, true, 0.85, "Looks good");

        // Assert
        vote.Voter.Should().Be(voter);
        vote.InFavor.Should().BeTrue();
        vote.Confidence.Should().Be(0.85);
        vote.Reasoning.Should().Be("Looks good");
    }

    [Fact]
    public void Vote_NullReasoning_IsAllowed()
    {
        // Act
        var vote = new Vote(CreateAgentId(), false, 0.5, null);

        // Assert
        vote.Reasoning.Should().BeNull();
    }

    [Fact]
    public void Decision_Constructor_SetsAllProperties()
    {
        // Arrange
        var agentId = CreateAgentId();
        var votes = new Dictionary<AgentId, Vote>
        {
            [agentId] = new Vote(agentId, true, 0.9, null),
        };

        // Act
        var decision = new Decision("Use feature X", true, votes, 0.9);

        // Assert
        decision.Proposal.Should().Be("Use feature X");
        decision.Accepted.Should().BeTrue();
        decision.Votes.Should().HaveCount(1);
        decision.ConsensusScore.Should().Be(0.9);
    }

    [Fact]
    public void CollaborativePlan_Constructor_SetsAllProperties()
    {
        // Arrange
        var agent = CreateAgentId();
        var assignments = new List<TaskAssignment>
        {
            new("Build API", agent, DateTime.UtcNow.AddHours(2), new List<AgentId>(), Priority.High),
        };
        var deps = new List<Dependency>
        {
            new("task1", "task2", DependencyType.BlockedBy),
        };

        // Act
        var plan = new CollaborativePlan("Build system", assignments, deps, TimeSpan.FromHours(4));

        // Assert
        plan.Goal.Should().Be("Build system");
        plan.Assignments.Should().HaveCount(1);
        plan.Dependencies.Should().HaveCount(1);
        plan.EstimatedDuration.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void TaskAssignment_Constructor_SetsAllProperties()
    {
        // Arrange
        var agent = CreateAgentId();
        var deadline = DateTime.UtcNow.AddHours(2);
        var depAgents = new List<AgentId> { CreateAgentId("dep1") };

        // Act
        var assignment = new TaskAssignment("Write tests", agent, deadline, depAgents, Priority.Critical);

        // Assert
        assignment.TaskDescription.Should().Be("Write tests");
        assignment.AssignedTo.Should().Be(agent);
        assignment.Deadline.Should().Be(deadline);
        assignment.Dependencies.Should().HaveCount(1);
        assignment.Priority.Should().Be(Priority.Critical);
    }

    [Fact]
    public void Dependency_Constructor_SetsAllProperties()
    {
        // Act
        var dep = new Dependency("task-a", "task-b", DependencyType.Requires);

        // Assert
        dep.TaskA.Should().Be("task-a");
        dep.TaskB.Should().Be("task-b");
        dep.Type.Should().Be(DependencyType.Requires);
    }
}
