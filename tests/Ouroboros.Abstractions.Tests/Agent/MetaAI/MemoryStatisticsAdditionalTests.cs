using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryStatisticsAdditionalTests
{

    [Fact]
    public void Permission_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new Permission("res", Ouroboros.Agent.PermissionLevel.Read, "reason");
        var b = new Permission("res", Ouroboros.Agent.PermissionLevel.Read, "reason");

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Permission_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Permission("res", Ouroboros.Agent.PermissionLevel.Read, "reason");

        // Act
        var modified = original with { Level = Ouroboros.Agent.PermissionLevel.Admin };

        // Assert
        modified.Level.Should().Be(Ouroboros.Agent.PermissionLevel.Admin);
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
