using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryStatisticsAdditionalTests
{
    [Fact]
    public void MemoryStatistics_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new MemoryStatistics(100, 80, 20, 50, 30, ts, ts, 0.75);
        var b = new MemoryStatistics(100, 80, 20, 50, 30, ts, ts, 0.75);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void MemoryStatistics_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new MemoryStatistics(100, 80, 20, 50, 30);

        // Act
        var modified = original with { TotalExperiences = 200, SuccessfulExperiences = 180 };

        // Assert
        modified.TotalExperiences.Should().Be(200);
        modified.SuccessfulExperiences.Should().Be(180);
        modified.FailedExperiences.Should().Be(20);
    }

    [Fact]
    public void Permission_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new Permission("res", PermissionLevel.Read, "reason");
        var b = new Permission("res", PermissionLevel.Read, "reason");

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Permission_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Permission("res", PermissionLevel.Read, "reason");

        // Act
        var modified = original with { Level = PermissionLevel.Admin };

        // Assert
        modified.Level.Should().Be(PermissionLevel.Admin);
        modified.Resource.Should().Be("res");
    }

    [Fact]
    public void SandboxResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var restrictions = new List<string> { "no-network" };
        var a = new SandboxResult(true, null, restrictions, null);
        var b = new SandboxResult(true, null, restrictions, null);

        // Assert
        a.Should().Be(b);
    }
}
