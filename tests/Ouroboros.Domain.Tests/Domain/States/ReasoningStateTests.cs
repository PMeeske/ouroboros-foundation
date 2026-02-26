namespace Ouroboros.Tests.Domain.States;

using Ouroboros.Domain.States;

[Trait("Category", "Unit")]
public class ReasoningStateTests
{
    [Fact]
    public void Draft_Constructor_SetsKindAndText()
    {
        // Act
        var draft = new Draft("initial draft text");

        // Assert
        draft.Kind.Should().Be("Draft");
        draft.Text.Should().Be("initial draft text");
        draft.DraftText.Should().Be("initial draft text");
    }

    [Fact]
    public void Draft_InheritsFromReasoningState()
    {
        // Act
        var draft = new Draft("text");

        // Assert
        draft.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void Draft_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var draft1 = new Draft("same text");
        var draft2 = new Draft("same text");

        // Assert
        draft1.Should().Be(draft2);
    }

    [Fact]
    public void Critique_Constructor_SetsKindAndText()
    {
        // Act
        var critique = new Critique("needs more detail");

        // Assert
        critique.Kind.Should().Be("Critique");
        critique.Text.Should().Be("needs more detail");
        critique.CritiqueText.Should().Be("needs more detail");
    }

    [Fact]
    public void Critique_InheritsFromReasoningState()
    {
        // Act
        var critique = new Critique("text");

        // Assert
        critique.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void FinalSpec_Constructor_SetsKindAndText()
    {
        // Act
        var final = new FinalSpec("final refined text");

        // Assert
        final.Kind.Should().Be("Final");
        final.Text.Should().Be("final refined text");
        final.FinalText.Should().Be("final refined text");
    }

    [Fact]
    public void FinalSpec_InheritsFromReasoningState()
    {
        // Act
        var final = new FinalSpec("text");

        // Assert
        final.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void Thinking_Constructor_SetsKindAndText()
    {
        // Act
        var thinking = new Thinking("analyzing the problem");

        // Assert
        thinking.Kind.Should().Be("Thinking");
        thinking.Text.Should().Be("analyzing the problem");
    }

    [Fact]
    public void Thinking_InheritsFromReasoningState()
    {
        // Act
        var thinking = new Thinking("text");

        // Assert
        thinking.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void DocumentRevision_Constructor_SetsAllProperties()
    {
        // Act
        var revision = new DocumentRevision("/path/to/file.md", "revised content", 3, "improve clarity");

        // Assert
        revision.Kind.Should().Be("DocumentRevision");
        revision.Text.Should().Be("revised content");
        revision.FilePath.Should().Be("/path/to/file.md");
        revision.RevisionText.Should().Be("revised content");
        revision.Iteration.Should().Be(3);
        revision.Goal.Should().Be("improve clarity");
    }

    [Fact]
    public void DocumentRevision_NullGoal_IsAllowed()
    {
        // Act
        var revision = new DocumentRevision("/path.md", "text", 1, null);

        // Assert
        revision.Goal.Should().BeNull();
    }

    [Fact]
    public void DocumentRevision_InheritsFromReasoningState()
    {
        // Act
        var revision = new DocumentRevision("/path.md", "text", 1, null);

        // Assert
        revision.Should().BeAssignableTo<ReasoningState>();
    }
}
