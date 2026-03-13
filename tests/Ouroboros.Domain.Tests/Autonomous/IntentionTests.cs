namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsExpectedDefaults()
    {
        var intention = new Intention
        {
            Title = "Test",
            Description = "Desc",
            Rationale = "Reason",
            Category = IntentionCategory.SelfReflection,
            Source = "test"
        };

        intention.Id.Should().NotBe(Guid.Empty);
        intention.Priority.Should().Be(IntentionPriority.Normal);
        intention.Status.Should().Be(IntentionStatus.Pending);
        intention.RequiresApproval.Should().BeTrue();
        intention.ExpiresAt.Should().BeNull();
        intention.Target.Should().BeNull();
        intention.Action.Should().BeNull();
        intention.UserComment.Should().BeNull();
        intention.ActedAt.Should().BeNull();
        intention.ExecutionResult.Should().BeNull();
        intention.Embedding.Should().BeNull();
        intention.ExpectedOutcomes.Should().BeEmpty();
        intention.Risks.Should().BeEmpty();
        intention.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_RequiredProperties_AreSet()
    {
        var intention = new Intention
        {
            Title = "Explore Topic",
            Description = "Research quantum computing",
            Rationale = "Expand knowledge",
            Category = IntentionCategory.Exploration,
            Source = "neuron.executive"
        };

        intention.Title.Should().Be("Explore Topic");
        intention.Description.Should().Be("Research quantum computing");
        intention.Rationale.Should().Be("Expand knowledge");
        intention.Category.Should().Be(IntentionCategory.Exploration);
        intention.Source.Should().Be("neuron.executive");
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new Intention
        {
            Title = "Test",
            Description = "Desc",
            Rationale = "Reason",
            Category = IntentionCategory.Learning,
            Source = "test"
        };

        var modified = original with { Status = IntentionStatus.Approved, UserComment = "OK" };

        modified.Status.Should().Be(IntentionStatus.Approved);
        modified.UserComment.Should().Be("OK");
        modified.Title.Should().Be("Test");
        original.Status.Should().Be(IntentionStatus.Pending);
    }

    [Fact]
    public void Id_IsUniquePerInstance()
    {
        var i1 = new Intention
        {
            Title = "A",
            Description = "D",
            Rationale = "R",
            Category = IntentionCategory.Learning,
            Source = "s"
        };
        var i2 = new Intention
        {
            Title = "B",
            Description = "D",
            Rationale = "R",
            Category = IntentionCategory.Learning,
            Source = "s"
        };

        i1.Id.Should().NotBe(i2.Id);
    }

    [Fact]
    public void CreatedAt_IsCloseToNow()
    {
        var before = DateTime.UtcNow;
        var intention = new Intention
        {
            Title = "T",
            Description = "D",
            Rationale = "R",
            Category = IntentionCategory.SelfReflection,
            Source = "s"
        };
        var after = DateTime.UtcNow;

        intention.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Metadata_CanStoreAndRetrieveValues()
    {
        var intention = new Intention
        {
            Title = "T",
            Description = "D",
            Rationale = "R",
            Category = IntentionCategory.Learning,
            Source = "s",
            Metadata = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 }
        };

        intention.Metadata.Should().ContainKey("key1");
        intention.Metadata["key1"].Should().Be("value1");
        intention.Metadata["key2"].Should().Be(42);
    }
}
