namespace Ouroboros.Tests.Domain.Tools;

using Ouroboros.Domain;

[Trait("Category", "Unit")]
public class ToolExecutionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var execution = new ToolExecution("web_search", "query=test", "found 5 results", timestamp);

        // Assert
        execution.ToolName.Should().Be("web_search");
        execution.Arguments.Should().Be("query=test");
        execution.Output.Should().Be("found 5 results");
        execution.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var e1 = new ToolExecution("tool", "args", "out", timestamp);
        var e2 = new ToolExecution("tool", "args", "out", timestamp);

        // Assert
        e1.Should().Be(e2);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        var e1 = new ToolExecution("tool1", "args", "out", timestamp);
        var e2 = new ToolExecution("tool2", "args", "out", timestamp);

        // Assert
        e1.Should().NotBe(e2);
    }
}
