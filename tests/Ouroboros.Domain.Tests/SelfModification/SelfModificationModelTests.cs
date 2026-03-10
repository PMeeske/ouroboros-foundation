using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

namespace Ouroboros.Tests.SelfModification;

#region ChangeCategory Tests

[Trait("Category", "Unit")]
public class ChangeCategoryModelTests
{
    [Theory]
    [InlineData(ChangeCategory.BugFix, 0)]
    [InlineData(ChangeCategory.Performance, 1)]
    [InlineData(ChangeCategory.Refactoring, 2)]
    [InlineData(ChangeCategory.Feature, 3)]
    [InlineData(ChangeCategory.Documentation, 4)]
    [InlineData(ChangeCategory.Testing, 5)]
    [InlineData(ChangeCategory.Security, 6)]
    public void EnumValues_ShouldHaveExpectedOrdinals(ChangeCategory category, int expected)
    {
        ((int)category).Should().Be(expected);
    }

    [Fact]
    public void EnumValues_ShouldHaveSevenMembers()
    {
        Enum.GetValues<ChangeCategory>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(ChangeCategory.BugFix)]
    [InlineData(ChangeCategory.Performance)]
    [InlineData(ChangeCategory.Refactoring)]
    [InlineData(ChangeCategory.Feature)]
    [InlineData(ChangeCategory.Documentation)]
    [InlineData(ChangeCategory.Testing)]
    [InlineData(ChangeCategory.Security)]
    public void AllValues_ShouldBeDefined(ChangeCategory category)
    {
        Enum.IsDefined(category).Should().BeTrue();
    }

    [Fact]
    public void UndefinedValue_ShouldNotBeDefined()
    {
        Enum.IsDefined((ChangeCategory)99).Should().BeFalse();
    }
}

#endregion

#region RiskLevel Tests

[Trait("Category", "Unit")]
public class RiskLevelModelTests
{
    [Theory]
    [InlineData(RiskLevel.Low, 0)]
    [InlineData(RiskLevel.Medium, 1)]
    [InlineData(RiskLevel.High, 2)]
    [InlineData(RiskLevel.Critical, 3)]
    public void EnumValues_ShouldHaveExpectedOrdinals(RiskLevel risk, int expected)
    {
        ((int)risk).Should().Be(expected);
    }

    [Fact]
    public void EnumValues_ShouldHaveFourMembers()
    {
        Enum.GetValues<RiskLevel>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(RiskLevel.Low)]
    [InlineData(RiskLevel.Medium)]
    [InlineData(RiskLevel.High)]
    [InlineData(RiskLevel.Critical)]
    public void AllValues_ShouldBeDefined(RiskLevel risk)
    {
        Enum.IsDefined(risk).Should().BeTrue();
    }

    [Fact]
    public void UndefinedValue_ShouldNotBeDefined()
    {
        Enum.IsDefined((RiskLevel)99).Should().BeFalse();
    }
}

#endregion

#region ProposalStatus Tests

[Trait("Category", "Unit")]
public class ProposalStatusModelTests
{
    [Theory]
    [InlineData(ProposalStatus.Pending, 0)]
    [InlineData(ProposalStatus.Approved, 1)]
    [InlineData(ProposalStatus.Rejected, 2)]
    [InlineData(ProposalStatus.Applied, 3)]
    [InlineData(ProposalStatus.Failed, 4)]
    public void EnumValues_ShouldHaveExpectedOrdinals(ProposalStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void EnumValues_ShouldHaveFiveMembers()
    {
        Enum.GetValues<ProposalStatus>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(ProposalStatus.Pending)]
    [InlineData(ProposalStatus.Approved)]
    [InlineData(ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.Applied)]
    [InlineData(ProposalStatus.Failed)]
    public void AllValues_ShouldBeDefined(ProposalStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Fact]
    public void UndefinedValue_ShouldNotBeDefined()
    {
        Enum.IsDefined((ProposalStatus)99).Should().BeFalse();
    }
}

#endregion

#region CodeAnalysis Tests

[Trait("Category", "Unit")]
public class CodeAnalysisModelTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var analysis = new CodeAnalysis(
            FilePath: "/src/MyClass.cs",
            Classes: new[] { "MyClass", "Helper" },
            Methods: new[] { "DoWork", "Initialize", "Cleanup" },
            Usings: new[] { "System", "System.Linq" },
            TotalLines: 200,
            CodeLines: 150,
            CommentLines: 30,
            CommentRatio: 0.15,
            Todos: new[] { "TODO: refactor", "TODO: add tests" },
            PotentialIssues: new[] { "Missing null check" });

        analysis.FilePath.Should().Be("/src/MyClass.cs");
        analysis.Classes.Should().HaveCount(2);
        analysis.Methods.Should().HaveCount(3);
        analysis.Usings.Should().HaveCount(2);
        analysis.TotalLines.Should().Be(200);
        analysis.CodeLines.Should().Be(150);
        analysis.CommentLines.Should().Be(30);
        analysis.CommentRatio.Should().Be(0.15);
        analysis.Todos.Should().HaveCount(2);
        analysis.PotentialIssues.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithEmptyCollections_ShouldWork()
    {
        var analysis = new CodeAnalysis(
            "/empty.cs",
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            0, 0, 0, 0.0,
            Array.Empty<string>(),
            Array.Empty<string>());

        analysis.Classes.Should().BeEmpty();
        analysis.Methods.Should().BeEmpty();
        analysis.Usings.Should().BeEmpty();
        analysis.Todos.Should().BeEmpty();
        analysis.PotentialIssues.Should().BeEmpty();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var classes = new[] { "A" };
        var methods = new[] { "B" };
        var usings = new[] { "System" };
        var todos = Array.Empty<string>();
        var issues = Array.Empty<string>();

        var a = new CodeAnalysis("/a.cs", classes, methods, usings, 10, 8, 2, 0.2, todos, issues);
        var b = new CodeAnalysis("/a.cs", classes, methods, usings, 10, 8, 2, 0.2, todos, issues);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new CodeAnalysis(
            "/a.cs",
            new[] { "A" },
            new[] { "M" },
            new[] { "System" },
            50, 40, 10, 0.2,
            Array.Empty<string>(),
            Array.Empty<string>());

        var modified = original with { TotalLines = 100, CodeLines = 80 };

        modified.TotalLines.Should().Be(100);
        modified.CodeLines.Should().Be(80);
        original.TotalLines.Should().Be(50);
    }
}

#endregion

#region CodeChangeProposal Tests

[Trait("Category", "Unit")]
public class CodeChangeProposalModelTests
{
    private static CodeChangeProposal CreateSample(
        string id = "prop-001",
        ProposalStatus status = ProposalStatus.Pending,
        string? reviewComment = null)
    {
        return new CodeChangeProposal(
            Id: id,
            FilePath: "/src/Example.cs",
            Description: "Fix null reference",
            Rationale: "Prevents NRE in production",
            OldCode: "if (x == null)",
            NewCode: "if (x is null)",
            Category: ChangeCategory.BugFix,
            Risk: RiskLevel.Low,
            ProposedAt: new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc),
            ReviewComment: reviewComment,
            Status: status);
    }

    [Fact]
    public void Create_WithDefaults_ShouldSetPendingStatusAndNullComment()
    {
        var proposal = new CodeChangeProposal(
            "id", "/path", "desc", "rationale", "old", "new",
            ChangeCategory.Feature, RiskLevel.Medium,
            DateTime.UtcNow);

        proposal.Status.Should().Be(ProposalStatus.Pending);
        proposal.ReviewComment.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var proposal = CreateSample();

        proposal.Id.Should().Be("prop-001");
        proposal.FilePath.Should().Be("/src/Example.cs");
        proposal.Description.Should().Be("Fix null reference");
        proposal.Rationale.Should().Be("Prevents NRE in production");
        proposal.OldCode.Should().Be("if (x == null)");
        proposal.NewCode.Should().Be("if (x is null)");
        proposal.Category.Should().Be(ChangeCategory.BugFix);
        proposal.Risk.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public void Create_WithApprovedStatus_ShouldSetStatus()
    {
        var proposal = CreateSample(status: ProposalStatus.Approved, reviewComment: "LGTM");

        proposal.Status.Should().Be(ProposalStatus.Approved);
        proposal.ReviewComment.Should().Be("LGTM");
    }

    [Theory]
    [InlineData(ProposalStatus.Pending)]
    [InlineData(ProposalStatus.Approved)]
    [InlineData(ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.Applied)]
    [InlineData(ProposalStatus.Failed)]
    public void Create_WithAnyStatus_ShouldPersist(ProposalStatus status)
    {
        var proposal = CreateSample(status: status);

        proposal.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChangeCategory.BugFix, RiskLevel.Low)]
    [InlineData(ChangeCategory.Security, RiskLevel.Critical)]
    [InlineData(ChangeCategory.Refactoring, RiskLevel.Medium)]
    [InlineData(ChangeCategory.Feature, RiskLevel.High)]
    public void Create_WithVariousCategoryAndRisk_ShouldPersist(ChangeCategory category, RiskLevel risk)
    {
        var proposal = new CodeChangeProposal(
            "id", "/path", "desc", "rationale", "old", "new",
            category, risk, DateTime.UtcNow);

        proposal.Category.Should().Be(category);
        proposal.Risk.Should().Be(risk);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var now = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var a = new CodeChangeProposal("1", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);
        var b = new CodeChangeProposal("1", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentId()
    {
        var now = DateTime.UtcNow;
        var a = new CodeChangeProposal("1", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);
        var b = new CodeChangeProposal("2", "/a", "d", "r", "o", "n", ChangeCategory.BugFix, RiskLevel.Low, now);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = CreateSample();
        var modified = original with { Status = ProposalStatus.Approved, ReviewComment = "Approved" };

        modified.Status.Should().Be(ProposalStatus.Approved);
        modified.ReviewComment.Should().Be("Approved");
        original.Status.Should().Be(ProposalStatus.Pending);
        original.ReviewComment.Should().BeNull();
    }
}

#endregion

#region GitOperationResult Tests

[Trait("Category", "Unit")]
public class GitOperationResultModelTests
{
    [Fact]
    public void Create_Success_ShouldSetAllProperties()
    {
        var result = new GitOperationResult(
            Success: true,
            Message: "Committed successfully",
            CommitHash: "abc123def456",
            BranchName: "feature/new-feature",
            AffectedFiles: new[] { "file1.cs", "file2.cs", "file3.cs" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Committed successfully");
        result.CommitHash.Should().Be("abc123def456");
        result.BranchName.Should().Be("feature/new-feature");
        result.AffectedFiles.Should().HaveCount(3);
    }

    [Fact]
    public void Create_Failure_ShouldHaveNullOptionals()
    {
        var result = new GitOperationResult(false, "Merge conflict");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Merge conflict");
        result.CommitHash.Should().BeNull();
        result.BranchName.Should().BeNull();
        result.AffectedFiles.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyAffectedFiles_ShouldWork()
    {
        var result = new GitOperationResult(true, "No changes", AffectedFiles: Array.Empty<string>());

        result.AffectedFiles.Should().BeEmpty();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new GitOperationResult(true, "OK", "abc", "main");
        var b = new GitOperationResult(true, "OK", "abc", "main");

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentSuccess()
    {
        var a = new GitOperationResult(true, "OK");
        var b = new GitOperationResult(false, "OK");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new GitOperationResult(false, "Failed");
        var modified = original with { Success = true, CommitHash = "abc123" };

        modified.Success.Should().BeTrue();
        modified.CommitHash.Should().Be("abc123");
        original.Success.Should().BeFalse();
    }
}

#endregion

#region RepoFileInfo Tests

[Trait("Category", "Unit")]
public class RepoFileInfoModelTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var modified = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var info = new RepoFileInfo(
            RelativePath: "src/MyClass.cs",
            FullPath: "/repo/src/MyClass.cs",
            SizeBytes: 2048,
            LastModified: modified,
            LineCount: 100,
            Language: "C#");

        info.RelativePath.Should().Be("src/MyClass.cs");
        info.FullPath.Should().Be("/repo/src/MyClass.cs");
        info.SizeBytes.Should().Be(2048);
        info.LastModified.Should().Be(modified);
        info.LineCount.Should().Be(100);
        info.Language.Should().Be("C#");
    }

    [Fact]
    public void Create_WithDifferentLanguages_ShouldPersist()
    {
        var modified = DateTime.UtcNow;

        var csFile = new RepoFileInfo("a.cs", "/a.cs", 100, modified, 10, "C#");
        var pyFile = new RepoFileInfo("b.py", "/b.py", 200, modified, 20, "Python");
        var tsFile = new RepoFileInfo("c.ts", "/c.ts", 300, modified, 30, "TypeScript");

        csFile.Language.Should().Be("C#");
        pyFile.Language.Should().Be("Python");
        tsFile.Language.Should().Be("TypeScript");
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var modified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new RepoFileInfo("a.cs", "/a.cs", 100, modified, 10, "C#");
        var b = new RepoFileInfo("a.cs", "/a.cs", 100, modified, 10, "C#");

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentPath()
    {
        var modified = DateTime.UtcNow;
        var a = new RepoFileInfo("a.cs", "/a.cs", 100, modified, 10, "C#");
        var b = new RepoFileInfo("b.cs", "/b.cs", 100, modified, 10, "C#");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var modified = DateTime.UtcNow;
        var original = new RepoFileInfo("a.cs", "/a.cs", 100, modified, 10, "C#");
        var updated = original with { SizeBytes = 200, LineCount = 20 };

        updated.SizeBytes.Should().Be(200);
        updated.LineCount.Should().Be(20);
        original.SizeBytes.Should().Be(100);
    }
}

#endregion
