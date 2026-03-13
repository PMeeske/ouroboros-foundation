using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class ThoughtResultTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var thoughtId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var executionTime = TimeSpan.FromMilliseconds(150);

        var result = new ThoughtResult(
            id, thoughtId, ThoughtResult.Types.Action, "Executed tool call",
            true, 0.95, createdAt, executionTime);

        result.Id.Should().Be(id);
        result.ThoughtId.Should().Be(thoughtId);
        result.ResultType.Should().Be("action");
        result.Content.Should().Be("Executed tool call");
        result.Success.Should().BeTrue();
        result.Confidence.Should().Be(0.95);
        result.CreatedAt.Should().Be(createdAt);
        result.ExecutionTime.Should().Be(executionTime);
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void Construction_WithMetadata_SetsMetadata()
    {
        var metadata = new Dictionary<string, object> { ["tool"] = "search" };

        var result = new ThoughtResult(
            Guid.NewGuid(), Guid.NewGuid(), "action", "content",
            true, 0.9, DateTime.UtcNow, null, metadata);

        result.Metadata.Should().ContainKey("tool");
    }

    [Fact]
    public void Construction_WithoutOptionalParams_DefaultsToNull()
    {
        var result = new ThoughtResult(
            Guid.NewGuid(), Guid.NewGuid(), "action", "content",
            true, 0.9, DateTime.UtcNow);

        result.ExecutionTime.Should().BeNull();
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void Types_HasExpectedConstants()
    {
        ThoughtResult.Types.Action.Should().Be("action");
        ThoughtResult.Types.Response.Should().Be("response");
        ThoughtResult.Types.Insight.Should().Be("insight");
        ThoughtResult.Types.Decision.Should().Be("decision");
        ThoughtResult.Types.SkillLearned.Should().Be("skill_learned");
        ThoughtResult.Types.FactDiscovered.Should().Be("fact_discovered");
        ThoughtResult.Types.Error.Should().Be("error");
        ThoughtResult.Types.Deferred.Should().Be("deferred");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var thoughtId = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var r1 = new ThoughtResult(id, thoughtId, "action", "content", true, 0.9, ts);
        var r2 = new ThoughtResult(id, thoughtId, "action", "content", true, 0.9, ts);

        r1.Should().Be(r2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var result = new ThoughtResult(Guid.NewGuid(), Guid.NewGuid(), "action", "content",
            true, 0.9, DateTime.UtcNow);

        var modified = result with { Success = false, Confidence = 0.1 };

        modified.Success.Should().BeFalse();
        modified.Confidence.Should().Be(0.1);
        result.Success.Should().BeTrue();
    }
}
