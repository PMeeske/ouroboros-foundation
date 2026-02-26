using FluentAssertions;
using Ouroboros.Domain.States;
using Xunit;

namespace Ouroboros.Tests.Domain.States;

[Trait("Category", "Unit")]
public class StatesEnumTests
{
    [Theory]
    [InlineData(FieldStatus.OK)]
    [InlineData(FieldStatus.INDIRECT)]
    [InlineData(FieldStatus.UNCLEAR)]
    [InlineData(FieldStatus.MISSING)]
    [InlineData(FieldStatus.INCONSISTENT)]
    public void FieldStatus_AllValues_AreDefined(FieldStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Fact]
    public void FieldStatus_HasFiveValues()
    {
        Enum.GetValues<FieldStatus>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(FormalStatus.Complete)]
    [InlineData(FormalStatus.Incomplete)]
    [InlineData(FormalStatus.NotAuditable)]
    public void FormalStatus_AllValues_AreDefined(FormalStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Fact]
    public void FormalStatus_HasThreeValues()
    {
        Enum.GetValues<FormalStatus>().Should().HaveCount(3);
    }
}

[Trait("Category", "Unit")]
public class DraftTests
{
    [Fact]
    public void Create_ShouldSetText()
    {
        var draft = new Draft("Initial draft content");
        draft.DraftText.Should().Be("Initial draft content");
    }

    [Fact]
    public void Create_ShouldInheritFromReasoningState()
    {
        var draft = new Draft("Content");
        draft.Should().BeAssignableTo<ReasoningState>();
    }

    [Fact]
    public void Kind_ShouldBeDraft()
    {
        var draft = new Draft("Content");
        draft.Kind.Should().Be("Draft");
    }

    [Fact]
    public void Text_ShouldMatchDraftText()
    {
        var draft = new Draft("Content");
        draft.Text.Should().Be("Content");
    }
}

[Trait("Category", "Unit")]
public class CritiqueTests
{
    [Fact]
    public void Create_ShouldSetCritiqueText()
    {
        var critique = new Critique("This needs improvement in section 2");
        critique.CritiqueText.Should().Be("This needs improvement in section 2");
    }

    [Fact]
    public void Kind_ShouldBeCritique()
    {
        var critique = new Critique("Feedback");
        critique.Kind.Should().Be("Critique");
    }

    [Fact]
    public void Text_ShouldMatchCritiqueText()
    {
        var critique = new Critique("Feedback");
        critique.Text.Should().Be("Feedback");
    }
}

[Trait("Category", "Unit")]
public class FinalSpecTests
{
    [Fact]
    public void Create_ShouldSetFinalText()
    {
        var spec = new FinalSpec("Final polished content");
        spec.FinalText.Should().Be("Final polished content");
    }

    [Fact]
    public void Kind_ShouldBeFinal()
    {
        var spec = new FinalSpec("Content");
        spec.Kind.Should().Be("Final");
    }

    [Fact]
    public void Text_ShouldMatchFinalText()
    {
        var spec = new FinalSpec("Content");
        spec.Text.Should().Be("Content");
    }
}

[Trait("Category", "Unit")]
public class ThinkingTests
{
    [Fact]
    public void Create_ShouldSetText()
    {
        var thinking = new Thinking("Analyzing the problem...");
        thinking.Text.Should().Be("Analyzing the problem...");
    }

    [Fact]
    public void Kind_ShouldBeThinking()
    {
        var thinking = new Thinking("Content");
        thinking.Kind.Should().Be("Thinking");
    }
}

[Trait("Category", "Unit")]
public class DocumentRevisionTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var revision = new DocumentRevision(
            FilePath: "/docs/readme.md",
            RevisionText: "Updated content",
            Iteration: 2,
            Goal: "Improve clarity");

        revision.FilePath.Should().Be("/docs/readme.md");
        revision.RevisionText.Should().Be("Updated content");
        revision.Iteration.Should().Be(2);
        revision.Goal.Should().Be("Improve clarity");
    }

    [Fact]
    public void Kind_ShouldBeDocumentRevision()
    {
        var revision = new DocumentRevision("/path", "text", 1, null);
        revision.Kind.Should().Be("DocumentRevision");
    }

    [Fact]
    public void Goal_CanBeNull()
    {
        var revision = new DocumentRevision("/path", "text", 1, null);
        revision.Goal.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class CostCategoryAuditTests
{
    [Fact]
    public void Create_ShouldSetAllFields()
    {
        var audit = new CostCategoryAudit(
            Category: "Heating",
            TotalCosts: FieldStatus.OK,
            ReferenceMetric: FieldStatus.OK,
            TotalReferenceValue: FieldStatus.INDIRECT,
            TenantShare: FieldStatus.OK,
            TenantCost: FieldStatus.OK,
            Balance: FieldStatus.MISSING,
            Comment: "Balance not provided");

        audit.Category.Should().Be("Heating");
        audit.TotalCosts.Should().Be(FieldStatus.OK);
        audit.ReferenceMetric.Should().Be(FieldStatus.OK);
        audit.TotalReferenceValue.Should().Be(FieldStatus.INDIRECT);
        audit.TenantShare.Should().Be(FieldStatus.OK);
        audit.TenantCost.Should().Be(FieldStatus.OK);
        audit.Balance.Should().Be(FieldStatus.MISSING);
        audit.Comment.Should().Be("Balance not provided");
    }

    [Fact]
    public void Comment_CanBeNull()
    {
        var audit = new CostCategoryAudit("Water", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);
        audit.Comment.Should().BeNull();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CostCategoryAudit("Heat", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);
        var b = new CostCategoryAudit("Heat", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);
        a.Should().Be(b);
    }
}
