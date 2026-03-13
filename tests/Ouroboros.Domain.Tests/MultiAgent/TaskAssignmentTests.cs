using Ouroboros.Domain.MultiAgent;

namespace Ouroboros.Tests.MultiAgent;

[Trait("Category", "Unit")]
public class TaskAssignmentTests
{
    private static AgentId CreateAgentId(string name = "agent") => new(Guid.NewGuid(), name);

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var assignedTo = CreateAgentId("worker");
        var deadline = DateTime.UtcNow.AddHours(1);
        var deps = new List<AgentId> { CreateAgentId("dep1") };

        var assignment = new TaskAssignment("Build feature", assignedTo, deadline, deps, Priority.High);

        assignment.TaskDescription.Should().Be("Build feature");
        assignment.AssignedTo.Should().Be(assignedTo);
        assignment.Deadline.Should().Be(deadline);
        assignment.Dependencies.Should().HaveCount(1);
        assignment.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public void Constructor_NoDependencies_ShouldHaveEmptyList()
    {
        var assignment = new TaskAssignment(
            "Solo task", CreateAgentId(), DateTime.UtcNow.AddDays(1), new List<AgentId>(), Priority.Low);

        assignment.Dependencies.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public void Constructor_AllPriorities_ShouldWork(Priority priority)
    {
        var assignment = new TaskAssignment(
            "Task", CreateAgentId(), DateTime.UtcNow, new List<AgentId>(), priority);

        assignment.Priority.Should().Be(priority);
    }
}
