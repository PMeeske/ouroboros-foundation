namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionTests
{
    [Fact]
    public void Constructor_RequiredProperties_AreSet()
    {
        // Act
        var intention = new Intention
        {
            Title = "Learn about AI",
            Description = "Research latest AI papers",
            Rationale = "Stay current",
            Category = IntentionCategory.Learning,
            Source = "CoreNeuron",
        };

        // Assert
        intention.Title.Should().Be("Learn about AI");
        intention.Description.Should().Be("Research latest AI papers");
        intention.Rationale.Should().Be("Stay current");
        intention.Category.Should().Be(IntentionCategory.Learning);
        intention.Source.Should().Be("CoreNeuron");
    }

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        // Act
        var intention = new Intention
        {
            Title = "Test",
            Description = "Test desc",
            Rationale = "Test rationale",
            Category = IntentionCategory.SelfReflection,
            Source = "TestSource",
        };

        // Assert
        intention.Id.Should().NotBeEmpty();
        intention.Priority.Should().Be(IntentionPriority.Normal);
        intention.ExpiresAt.Should().BeNull();
        intention.Target.Should().BeNull();
        intention.Action.Should().BeNull();
        intention.ExpectedOutcomes.Should().BeEmpty();
        intention.Risks.Should().BeEmpty();
        intention.RequiresApproval.Should().BeTrue();
        intention.Status.Should().Be(IntentionStatus.Pending);
        intention.UserComment.Should().BeNull();
        intention.ActedAt.Should().BeNull();
        intention.ExecutionResult.Should().BeNull();
        intention.Embedding.Should().BeNull();
        intention.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithAction_SetsActionDetails()
    {
        // Arrange
        var action = new IntentionAction
        {
            ActionType = "tool",
            ToolName = "web_search",
            ToolInput = "AI papers 2025",
        };

        // Act
        var intention = new Intention
        {
            Title = "Search",
            Description = "Search web",
            Rationale = "Need info",
            Category = IntentionCategory.Exploration,
            Source = "CoreNeuron",
            Action = action,
        };

        // Assert
        intention.Action.Should().NotBeNull();
        intention.Action!.ActionType.Should().Be("tool");
        intention.Action.ToolName.Should().Be("web_search");
    }

    [Fact]
    public void RecordWith_CreatesNewInstance()
    {
        // Arrange
        var intention = new Intention
        {
            Title = "Test",
            Description = "Desc",
            Rationale = "Rationale",
            Category = IntentionCategory.Learning,
            Source = "Src",
        };

        // Act
        var approved = intention with
        {
            Status = IntentionStatus.Approved,
            ActedAt = DateTime.UtcNow,
        };

        // Assert
        approved.Status.Should().Be(IntentionStatus.Approved);
        approved.ActedAt.Should().NotBeNull();
        intention.Status.Should().Be(IntentionStatus.Pending);
    }
}
