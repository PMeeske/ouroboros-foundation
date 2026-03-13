using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class ThoughtRelationTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var relation = new ThoughtRelation(id, sourceId, targetId, ThoughtRelation.Types.CausedBy, 0.85, createdAt);

        relation.Id.Should().Be(id);
        relation.SourceThoughtId.Should().Be(sourceId);
        relation.TargetThoughtId.Should().Be(targetId);
        relation.RelationType.Should().Be("caused_by");
        relation.Strength.Should().Be(0.85);
        relation.CreatedAt.Should().Be(createdAt);
        relation.Metadata.Should().BeNull();
    }

    [Fact]
    public void Construction_WithMetadata_SetsMetadata()
    {
        var metadata = new Dictionary<string, object> { ["reason"] = "semantic similarity" };

        var relation = new ThoughtRelation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ThoughtRelation.Types.SimilarTo, 0.9, DateTime.UtcNow, metadata);

        relation.Metadata.Should().ContainKey("reason");
    }

    [Fact]
    public void Types_HasExpectedConstants()
    {
        ThoughtRelation.Types.CausedBy.Should().Be("caused_by");
        ThoughtRelation.Types.LeadsTo.Should().Be("leads_to");
        ThoughtRelation.Types.Contradicts.Should().Be("contradicts");
        ThoughtRelation.Types.Supports.Should().Be("supports");
        ThoughtRelation.Types.Refines.Should().Be("refines");
        ThoughtRelation.Types.Abstracts.Should().Be("abstracts");
        ThoughtRelation.Types.Elaborates.Should().Be("elaborates");
        ThoughtRelation.Types.SimilarTo.Should().Be("similar_to");
        ThoughtRelation.Types.InstanceOf.Should().Be("instance_of");
        ThoughtRelation.Types.PartOf.Should().Be("part_of");
        ThoughtRelation.Types.Triggers.Should().Be("triggers");
        ThoughtRelation.Types.Resolves.Should().Be("resolves");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var src = Guid.NewGuid();
        var tgt = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var r1 = new ThoughtRelation(id, src, tgt, "supports", 0.8, ts);
        var r2 = new ThoughtRelation(id, src, tgt, "supports", 0.8, ts);

        r1.Should().Be(r2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var relation = new ThoughtRelation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "supports", 0.5, DateTime.UtcNow);

        var modified = relation with { Strength = 0.9 };

        modified.Strength.Should().Be(0.9);
        relation.Strength.Should().Be(0.5);
    }
}
