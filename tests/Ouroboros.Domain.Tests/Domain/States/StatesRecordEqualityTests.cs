using Ouroboros.Domain.States;

namespace Ouroboros.Tests.Domain.States;

[Trait("Category", "Unit")]
public class StatesRecordEqualityTests
{
    [Fact]
    public void Draft_Equality_SameValues()
    {
        var a = new Draft("content");
        var b = new Draft("content");

        a.Should().Be(b);
    }

    [Fact]
    public void Draft_Equality_DifferentValues_NotEqual()
    {
        var a = new Draft("content1");
        var b = new Draft("content2");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Draft_WithExpression_ChangesText()
    {
        var draft = new Draft("original");
        var modified = draft with { DraftText = "modified" };

        modified.DraftText.Should().Be("modified");
        draft.DraftText.Should().Be("original");
    }

    [Fact]
    public void Critique_Equality_SameValues()
    {
        var a = new Critique("feedback");
        var b = new Critique("feedback");

        a.Should().Be(b);
    }

    [Fact]
    public void Critique_WithExpression_ChangesText()
    {
        var critique = new Critique("old feedback");
        var modified = critique with { CritiqueText = "new feedback" };

        modified.CritiqueText.Should().Be("new feedback");
        critique.CritiqueText.Should().Be("old feedback");
    }

    [Fact]
    public void FinalSpec_Equality_SameValues()
    {
        var a = new FinalSpec("final content");
        var b = new FinalSpec("final content");

        a.Should().Be(b);
    }

    [Fact]
    public void FinalSpec_WithExpression_ChangesText()
    {
        var spec = new FinalSpec("original");
        var modified = spec with { FinalText = "polished" };

        modified.FinalText.Should().Be("polished");
        spec.FinalText.Should().Be("original");
    }

    [Fact]
    public void Thinking_Equality_SameValues()
    {
        var a = new Thinking("thinking...");
        var b = new Thinking("thinking...");

        a.Should().Be(b);
    }

    [Fact]
    public void Thinking_WithExpression_ChangesText()
    {
        var thinking = new Thinking("step 1");
        var modified = thinking with { Text = "step 2" };

        modified.Text.Should().Be("step 2");
        thinking.Text.Should().Be("step 1");
    }

    [Fact]
    public void DocumentRevision_Equality_SameValues()
    {
        var a = new DocumentRevision("/path", "text", 1, "goal");
        var b = new DocumentRevision("/path", "text", 1, "goal");

        a.Should().Be(b);
    }

    [Fact]
    public void DocumentRevision_Equality_DifferentIteration_NotEqual()
    {
        var a = new DocumentRevision("/path", "text", 1, "goal");
        var b = new DocumentRevision("/path", "text", 2, "goal");

        a.Should().NotBe(b);
    }

    [Fact]
    public void DocumentRevision_WithExpression_ChangesIteration()
    {
        var revision = new DocumentRevision("/path", "text", 1, "goal");
        var modified = revision with { Iteration = 3 };

        modified.Iteration.Should().Be(3);
        revision.Iteration.Should().Be(1);
    }

    [Fact]
    public void DocumentRevision_WithExpression_ChangesGoal()
    {
        var revision = new DocumentRevision("/path", "text", 1, "old goal");
        var modified = revision with { Goal = "new goal" };

        modified.Goal.Should().Be("new goal");
        revision.Goal.Should().Be("old goal");
    }

    [Fact]
    public void CostCategoryAudit_WithExpression_ChangesCategory()
    {
        var audit = new CostCategoryAudit(
            "Heating", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);

        var modified = audit with { Category = "Water" };

        modified.Category.Should().Be("Water");
        audit.Category.Should().Be("Heating");
    }

    [Fact]
    public void CostCategoryAudit_WithExpression_ChangesFieldStatus()
    {
        var audit = new CostCategoryAudit(
            "Heating", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);

        var modified = audit with { TotalCosts = FieldStatus.MISSING };

        modified.TotalCosts.Should().Be(FieldStatus.MISSING);
        audit.TotalCosts.Should().Be(FieldStatus.OK);
    }

    [Fact]
    public void CostCategoryAudit_WithExpression_AddsComment()
    {
        var audit = new CostCategoryAudit(
            "Heating", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);

        var modified = audit with { Comment = "Needs review" };

        modified.Comment.Should().Be("Needs review");
        audit.Comment.Should().BeNull();
    }

    [Fact]
    public void FieldStatus_CanConvertToInt()
    {
        ((int)FieldStatus.OK).Should().Be(0);
        ((int)FieldStatus.INDIRECT).Should().Be(1);
        ((int)FieldStatus.UNCLEAR).Should().Be(2);
        ((int)FieldStatus.MISSING).Should().Be(3);
        ((int)FieldStatus.INCONSISTENT).Should().Be(4);
    }

    [Fact]
    public void FormalStatus_CanConvertToInt()
    {
        ((int)FormalStatus.Complete).Should().Be(0);
        ((int)FormalStatus.Incomplete).Should().Be(1);
        ((int)FormalStatus.NotAuditable).Should().Be(2);
    }

    [Fact]
    public void Draft_GetHashCode_EqualRecords_HaveSameHash()
    {
        var a = new Draft("content");
        var b = new Draft("content");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Thinking_InheritsFromReasoningState()
    {
        var thinking = new Thinking("analyzing");
        thinking.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void DocumentRevision_InheritsFromReasoningState()
    {
        var revision = new DocumentRevision("/path", "text", 1, null);
        revision.Should().BeAssignableTo<ReasoningState>();
        revision.Text.Should().Be("text");
    }
}
