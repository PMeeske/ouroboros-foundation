using Ouroboros.Domain.MultiAgent;

namespace Ouroboros.Tests.Domain.MultiAgent;

[Trait("Category", "Unit")]
public sealed class CollaborativePlanTests
{
    private static AgentId CreateAgent(string name) => new(Guid.NewGuid(), name);

    [Fact]
    public void CollaborativePlan_Creation()
    {
        AgentId agent1 = CreateAgent("Agent1");
        AgentId agent2 = CreateAgent("Agent2");

        List<TaskAssignment> assignments = new()
        {
            new("Design API", agent1, DateTime.UtcNow.AddHours(2), new List<AgentId>(), Priority.High),
            new("Write Tests", agent2, DateTime.UtcNow.AddHours(4), new List<AgentId> { agent1 }, Priority.Normal),
        };

        List<Dependency> deps = new()
        {
            new("Design API", "Write Tests", DependencyType.BlockedBy),
        };

        CollaborativePlan plan = new("Build Feature", assignments, deps, TimeSpan.FromHours(4));

        plan.Goal.Should().Be("Build Feature");
        plan.Assignments.Should().HaveCount(2);
        plan.Dependencies.Should().HaveCount(1);
        plan.EstimatedDuration.Should().Be(TimeSpan.FromHours(4));
    }

    [Fact]
    public void AgentCapabilities_Creation()
    {
        AgentId id = CreateAgent("Coder");
        AgentCapabilities caps = new(
            id,
            new List<string> { "coding", "testing" },
            new Dictionary<string, double> { ["coding"] = 0.9, ["testing"] = 0.7 },
            0.3,
            true);

        caps.Id.Should().Be(id);
        caps.Skills.Should().Contain("coding");
        caps.PerformanceScores["coding"].Should().Be(0.9);
        caps.CurrentLoad.Should().Be(0.3);
        caps.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Decision_Creation()
    {
        AgentId voter1 = CreateAgent("V1");
        AgentId voter2 = CreateAgent("V2");

        Dictionary<AgentId, Vote> votes = new()
        {
            [voter1] = new Vote(voter1, true, 0.9, "Looks good"),
            [voter2] = new Vote(voter2, false, 0.6, "Need more info"),
        };

        Decision decision = new("Adopt new framework", false, votes, 0.5);

        decision.Proposal.Should().Be("Adopt new framework");
        decision.Accepted.Should().BeFalse();
        decision.Votes.Should().HaveCount(2);
        decision.ConsensusScore.Should().Be(0.5);
    }

    [Fact]
    public void Vote_Creation()
    {
        AgentId voter = CreateAgent("Expert");
        Vote vote = new(voter, true, 0.95, "Strongly agree");

        vote.Voter.Should().Be(voter);
        vote.InFavor.Should().BeTrue();
        vote.Confidence.Should().Be(0.95);
        vote.Reasoning.Should().Be("Strongly agree");
    }

    [Fact]
    public void Vote_Reasoning_Can_Be_Null()
    {
        AgentId voter = CreateAgent("Quick");
        Vote vote = new(voter, true, 0.5, null);

        vote.Reasoning.Should().BeNull();
    }

    [Fact]
    public void Message_Creation()
    {
        AgentId sender = CreateAgent("Sender");
        AgentId recipient = CreateAgent("Receiver");
        Guid conversationId = Guid.NewGuid();

        Message msg = new(sender, recipient, MessageType.Request, "Hello", DateTime.UtcNow, conversationId);

        msg.Sender.Should().Be(sender);
        msg.Recipient.Should().Be(recipient);
        msg.Type.Should().Be(MessageType.Request);
        msg.Payload.Should().Be("Hello");
        msg.ConversationId.Should().Be(conversationId);
    }

    [Fact]
    public void Message_Recipient_Can_Be_Null_For_Broadcast()
    {
        AgentId sender = CreateAgent("Broadcaster");

        Message msg = new(sender, null, MessageType.Broadcast, "Announcement", DateTime.UtcNow, Guid.NewGuid());

        msg.Recipient.Should().BeNull();
    }

    [Fact]
    public void AgentGroup_Creation()
    {
        List<AgentId> members = new()
        {
            CreateAgent("A1"),
            CreateAgent("A2"),
            CreateAgent("A3"),
        };

        AgentGroup group = new("Dev Team", members, GroupType.Collaborative);

        group.Name.Should().Be("Dev Team");
        group.Members.Should().HaveCount(3);
        group.Type.Should().Be(GroupType.Collaborative);
    }

    [Fact]
    public void TaskAssignment_With_Dependencies()
    {
        AgentId assignee = CreateAgent("Worker");
        AgentId dep1 = CreateAgent("Dep1");
        AgentId dep2 = CreateAgent("Dep2");

        TaskAssignment task = new("Implement feature", assignee, DateTime.UtcNow.AddDays(1),
            new List<AgentId> { dep1, dep2 }, Priority.Critical);

        task.TaskDescription.Should().Be("Implement feature");
        task.AssignedTo.Should().Be(assignee);
        task.Dependencies.Should().HaveCount(2);
        task.Priority.Should().Be(Priority.Critical);
    }

    [Fact]
    public void Dependency_Record()
    {
        Dependency dep = new("taskA", "taskB", DependencyType.BlockedBy);

        dep.TaskA.Should().Be("taskA");
        dep.TaskB.Should().Be("taskB");
        dep.Type.Should().Be(DependencyType.BlockedBy);
    }
}
