using Ouroboros.Domain.Autonomous;

namespace Ouroboros.Tests.Autonomous;

[Trait("Category", "Unit")]
public class IntentionTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        // Arrange & Act
        var intention = new Intention
        {
            Title = "Test",
            Description = "A test intention",
            Rationale = "For testing",
            Category = IntentionCategory.Learning,
            Source = "TestNeuron"
        };

        // Assert
        intention.Id.Should().NotBe(Guid.Empty);
        intention.Title.Should().Be("Test");
        intention.Description.Should().Be("A test intention");
        intention.Rationale.Should().Be("For testing");
        intention.Category.Should().Be(IntentionCategory.Learning);
        intention.Priority.Should().Be(IntentionPriority.Normal);
        intention.Status.Should().Be(IntentionStatus.Pending);
        intention.RequiresApproval.Should().BeTrue();
        intention.ExpiresAt.Should().BeNull();
        intention.UserComment.Should().BeNull();
        intention.ActedAt.Should().BeNull();
        intention.ExecutionResult.Should().BeNull();
        intention.Embedding.Should().BeNull();
        intention.Target.Should().BeNull();
        intention.Action.Should().BeNull();
        intention.ExpectedOutcomes.Should().BeEmpty();
        intention.Risks.Should().BeEmpty();
        intention.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Intention
        {
            Title = "Original",
            Description = "Desc",
            Rationale = "Reason",
            Category = IntentionCategory.CodeModification,
            Source = "Source"
        };

        // Act
        var updated = original with { Status = IntentionStatus.Approved, UserComment = "Go ahead" };

        // Assert
        updated.Status.Should().Be(IntentionStatus.Approved);
        updated.UserComment.Should().Be("Go ahead");
        updated.Title.Should().Be("Original");
        original.Status.Should().Be(IntentionStatus.Pending);
    }

    [Fact]
    public void CreatedAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var intention = new Intention
        {
            Title = "T",
            Description = "D",
            Rationale = "R",
            Category = IntentionCategory.Learning,
            Source = "S"
        };

        // Assert
        intention.CreatedAt.Should().BeOnOrAfter(before);
        intention.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void EachInstance_GetsUniqueId()
    {
        // Arrange & Act
        var a = new Intention { Title = "A", Description = "D", Rationale = "R", Category = IntentionCategory.Learning, Source = "S" };
        var b = new Intention { Title = "B", Description = "D", Rationale = "R", Category = IntentionCategory.Learning, Source = "S" };

        // Assert
        a.Id.Should().NotBe(b.Id);
    }
}
