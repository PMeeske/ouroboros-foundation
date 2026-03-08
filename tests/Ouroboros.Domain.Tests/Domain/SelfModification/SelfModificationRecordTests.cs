using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

namespace Ouroboros.Tests.Domain.SelfModification;

[Trait("Category", "Unit")]
public class SelfModificationEnumTests
{
    [Theory]
    [InlineData(ChangeCategory.BugFix)]
    [InlineData(ChangeCategory.Performance)]
    [InlineData(ChangeCategory.Refactoring)]
    [InlineData(ChangeCategory.Feature)]
    [InlineData(ChangeCategory.Documentation)]
    [InlineData(ChangeCategory.Testing)]
    [InlineData(ChangeCategory.Security)]
    public void ChangeCategory_AllValues_AreDefined(ChangeCategory category)
    {
        Enum.IsDefined(category).Should().BeTrue();
    }

    [Fact]
    public void ChangeCategory_HasSevenValues()
    {
        Enum.GetValues<ChangeCategory>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(RiskLevel.Low)]
    [InlineData(RiskLevel.Medium)]
    [InlineData(RiskLevel.High)]
    [InlineData(RiskLevel.Critical)]
    public void RiskLevel_AllValues_AreDefined(RiskLevel risk)
    {
        Enum.IsDefined(risk).Should().BeTrue();
    }

    [Fact]
    public void RiskLevel_HasFourValues()
    {
        Enum.GetValues<RiskLevel>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ProposalStatus.Pending)]
    [InlineData(ProposalStatus.Approved)]
    [InlineData(ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.Applied)]
    [InlineData(ProposalStatus.Failed)]
    public void ProposalStatus_AllValues_AreDefined(ProposalStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Fact]
    public void ProposalStatus_HasFiveValues()
    {
        Enum.GetValues<ProposalStatus>().Should().HaveCount(5);
    }
}

[Trait("Category", "Unit")]
public class CodeChangeProposalTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var proposal = new CodeChangeProposal(
            Id: "prop-001",
            FilePath: "/src/Example.cs",
            Description: "Fix null check",
            Rationale: "Prevents NRE in production",
            OldCode: "if (x == null)",
            NewCode: "if (x is null)",
            Category: ChangeCategory.BugFix,
            Risk: RiskLevel.Low,
            ProposedAt: now);

        proposal.Id.Should().Be("prop-001");
        proposal.FilePath.Should().Be("/src/Example.cs");
        proposal.Description.Should().Be("Fix null check");
        proposal.Rationale.Should().Be("Prevents NRE in production");
        proposal.OldCode.Should().Be("if (x == null)");
        proposal.NewCode.Should().Be("if (x is null)");
        proposal.Category.Should().Be(ChangeCategory.BugFix);
        proposal.Risk.Should().Be(RiskLevel.Low);
        proposal.ProposedAt.Should().Be(now);
        proposal.Status.Should().Be(ProposalStatus.Pending);
        proposal.ReviewComment.Should().BeNull();
    }

    [Fact]
    public void Create_WithReviewComment_ShouldSetIt()
    {
        var proposal = new CodeChangeProposal(
            Id: "prop-002",
            FilePath: "/src/Test.cs",
            Description: "Refactor",
            Rationale: "Simplify",
            OldCode: "old",
            NewCode: "new",
            Category: ChangeCategory.Refactoring,
            Risk: RiskLevel.Medium,
            ProposedAt: DateTime.UtcNow,
            ReviewComment: "Approved with minor changes",
            Status: ProposalStatus.Approved);

        proposal.ReviewComment.Should().Be("Approved with minor changes");
        proposal.Status.Should().Be(ProposalStatus.Approved);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var now = DateTime.UtcNow;
        var a = new CodeChangeProposal("1", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);
        var b = new CodeChangeProposal("1", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class CodeAnalysisTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var analysis = new CodeAnalysis(
            FilePath: "/src/Test.cs",
            Classes: new[] { "TestClass" },
            Methods: new[] { "Method1", "Method2" },
            Usings: new[] { "System" },
            TotalLines: 100,
            CodeLines: 80,
            CommentLines: 20,
            CommentRatio: 0.2,
            Todos: new[] { "TODO: fix this" },
            PotentialIssues: Array.Empty<string>());

        analysis.FilePath.Should().Be("/src/Test.cs");
        analysis.Classes.Should().HaveCount(1);
        analysis.Methods.Should().HaveCount(2);
        analysis.Usings.Should().ContainSingle();
        analysis.TotalLines.Should().Be(100);
        analysis.CodeLines.Should().Be(80);
        analysis.CommentLines.Should().Be(20);
        analysis.CommentRatio.Should().Be(0.2);
        analysis.Todos.Should().ContainSingle();
        analysis.PotentialIssues.Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class GitOperationResultTests
{
    [Fact]
    public void Create_Success_ShouldSetProperties()
    {
        var result = new GitOperationResult(
            Success: true,
            Message: "Committed",
            CommitHash: "abc123",
            BranchName: "feature/test",
            AffectedFiles: new[] { "file1.cs", "file2.cs" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Committed");
        result.CommitHash.Should().Be("abc123");
        result.BranchName.Should().Be("feature/test");
        result.AffectedFiles.Should().HaveCount(2);
    }

    [Fact]
    public void Create_Failure_ShouldHaveNullOptionalFields()
    {
        var result = new GitOperationResult(false, "Failed to commit");

        result.Success.Should().BeFalse();
        result.CommitHash.Should().BeNull();
        result.BranchName.Should().BeNull();
        result.AffectedFiles.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
public class RepoFileInfoTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var modified = DateTime.UtcNow;
        var info = new RepoFileInfo(
            RelativePath: "src/Test.cs",
            FullPath: "/repo/src/Test.cs",
            SizeBytes: 1024,
            LastModified: modified,
            LineCount: 50,
            Language: "C#");

        info.RelativePath.Should().Be("src/Test.cs");
        info.FullPath.Should().Be("/repo/src/Test.cs");
        info.SizeBytes.Should().Be(1024);
        info.LastModified.Should().Be(modified);
        info.LineCount.Should().Be(50);
        info.Language.Should().Be("C#");
    }
}
