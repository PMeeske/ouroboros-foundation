// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.SelfModification;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

/// <summary>
/// Tests for <see cref="GitReflectionService"/>.
/// Focuses on: risk assessment, proposal lifecycle, immutable/safe path enforcement,
/// code analysis, file filtering, and the self-modification workflow.
/// </summary>
[Trait("Category", "Unit")]
public class GitReflectionServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly GitReflectionService _sut;

    public GitReflectionServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _sut = new GitReflectionService(_tempDir);
    }

    public void Dispose()
    {
        _sut.Dispose();
        try { Directory.Delete(_tempDir, true); }
        catch { /* best-effort cleanup */ }
    }

    // ----------------------------------------------------------------
    // Helper to invoke private AssessRisk via reflection
    // ----------------------------------------------------------------

    private static RiskLevel InvokeAssessRisk(GitReflectionService sut, string filePath, string oldCode, string newCode)
    {
        MethodInfo? method = typeof(GitReflectionService)
            .GetMethod("AssessRisk", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull("AssessRisk should exist as a private instance method");
        return (RiskLevel)method!.Invoke(sut, new object[] { filePath, oldCode, newCode })!;
    }

    // ----------------------------------------------------------------
    // AssessRisk - Documentation
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("docs/README.md")]
    [InlineData("notes.txt")]
    public void AssessRisk_DocumentationFiles_ReturnsLow(string filePath)
    {
        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, filePath, "old", "new");

        // Assert
        risk.Should().Be(RiskLevel.Low);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Visibility changes
    // ----------------------------------------------------------------

    [Fact]
    public void AssessRisk_PrivateToPublic_ReturnsHigh()
    {
        // Arrange
        string oldCode = "private void DoSomething()";
        string newCode = "public void DoSomething()";

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.High);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Removing async
    // ----------------------------------------------------------------

    [Fact]
    public void AssessRisk_RemovingAsync_ReturnsHigh()
    {
        // Arrange
        string oldCode = "async Task<string> GetDataAsync()";
        string newCode = "string GetData()";

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.High);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Removing error handling
    // ----------------------------------------------------------------

    [Fact]
    public void AssessRisk_RemovingTryCatch_ReturnsHigh()
    {
        // Arrange
        string oldCode = "try { DoWork(); } catch { }";
        string newCode = "DoWork();";

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.High);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Security-critical patterns
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("unsafe { ptr++; }")]
    [InlineData("Process.Start(\"cmd\")")]
    [InlineData("File.Delete(path)")]
    [InlineData("Directory.Delete(path)")]
    public void AssessRisk_SecurityPatterns_ReturnsCritical(string dangerousCode)
    {
        // Arrange
        string oldCode = "// safe code";
        string newCode = dangerousCode;

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.Critical);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Large size difference
    // ----------------------------------------------------------------

    [Fact]
    public void AssessRisk_LargeSizeDifference_ReturnsMedium()
    {
        // Arrange
        string oldCode = "x";
        string newCode = new string('a', 600);

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.Medium);
    }

    // ----------------------------------------------------------------
    // AssessRisk - Whitespace/comment only changes
    // ----------------------------------------------------------------

    [Fact]
    public void AssessRisk_WhitespaceOnlyChanges_ReturnsLow()
    {
        // Arrange
        string oldCode = "int x = 1;";
        string newCode = "int  x  =  1;";

        // Act
        RiskLevel risk = InvokeAssessRisk(_sut, "src/Service.cs", oldCode, newCode);

        // Assert
        risk.Should().Be(RiskLevel.Low);
    }

    // ----------------------------------------------------------------
    // Proposal lifecycle: Propose -> Approve -> Apply
    // ----------------------------------------------------------------

    [Fact]
    public void ProposeChange_CreatesProposalWithPendingStatus()
    {
        // Act
        CodeChangeProposal proposal = _sut.ProposeChange(
            "src/Service.cs", "Add logging", "Improve observability",
            "old code", "new code", ChangeCategory.Feature, RiskLevel.Low);

        // Assert
        proposal.Should().NotBeNull();
        proposal.Status.Should().Be(ProposalStatus.Pending);
        proposal.FilePath.Should().Be("src/Service.cs");
        proposal.Description.Should().Be("Add logging");
        proposal.Rationale.Should().Be("Improve observability");
        proposal.Category.Should().Be(ChangeCategory.Feature);
        proposal.Risk.Should().Be(RiskLevel.Low);
        _sut.Proposals.Should().HaveCount(1);
    }

    [Fact]
    public void ProposeChange_MultipleProposals_AllTracked()
    {
        // Act
        _sut.ProposeChange("a.cs", "d1", "r1", "old", "new", ChangeCategory.BugFix, RiskLevel.Low);
        _sut.ProposeChange("b.cs", "d2", "r2", "old", "new", ChangeCategory.Feature, RiskLevel.Medium);

        // Assert
        _sut.Proposals.Should().HaveCount(2);
    }

    [Fact]
    public void ApproveProposal_ValidId_SetsStatusToApproved()
    {
        // Arrange
        var proposal = _sut.ProposeChange("a.cs", "d", "r", "old", "new",
            ChangeCategory.BugFix, RiskLevel.Low);

        // Act
        bool result = _sut.ApproveProposal(proposal.Id, "Looks good");

        // Assert
        result.Should().BeTrue();
        _sut.Proposals.Single().Status.Should().Be(ProposalStatus.Approved);
        _sut.Proposals.Single().ReviewComment.Should().Be("Looks good");
    }

    [Fact]
    public void ApproveProposal_InvalidId_ReturnsFalse()
    {
        // Act
        bool result = _sut.ApproveProposal("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectProposal_ValidId_SetsStatusToRejected()
    {
        // Arrange
        var proposal = _sut.ProposeChange("a.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Low);

        // Act
        bool result = _sut.RejectProposal(proposal.Id, "Too risky");

        // Assert
        result.Should().BeTrue();
        _sut.Proposals.Single().Status.Should().Be(ProposalStatus.Rejected);
        _sut.Proposals.Single().ReviewComment.Should().Be("Too risky");
    }

    [Fact]
    public void RejectProposal_InvalidId_ReturnsFalse()
    {
        // Act
        bool result = _sut.RejectProposal("nonexistent", "reason");

        // Assert
        result.Should().BeFalse();
    }

    // ----------------------------------------------------------------
    // ApplyProposalAsync - Validation
    // ----------------------------------------------------------------

    [Fact]
    public async Task ApplyProposalAsync_ProposalNotFound_ReturnsFailure()
    {
        // Act
        var result = await _sut.ApplyProposalAsync("nonexistent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ApplyProposalAsync_NotApproved_ReturnsFailure()
    {
        // Arrange
        var proposal = _sut.ProposeChange("a.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Low);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not approved");
    }

    [Fact]
    public async Task ApplyProposalAsync_ImmutablePath_ReturnsFailure()
    {
        // Arrange
        var proposal = _sut.ProposeChange(
            "src/Ouroboros.CLI/Constitution/core.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("immutable");
    }

    [Fact]
    public async Task ApplyProposalAsync_ImmutableSovereigntyPath_ReturnsFailure()
    {
        // Arrange
        var proposal = _sut.ProposeChange(
            "src/Ouroboros.CLI/Sovereignty/sovereign.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("immutable");
    }

    [Fact]
    public async Task ApplyProposalAsync_ImmutableEthicsPath_ReturnsFailure()
    {
        // Arrange
        var proposal = _sut.ProposeChange(
            "src/Ouroboros.Core/Ethics/Framework.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("immutable");
    }

    [Fact]
    public async Task ApplyProposalAsync_ImmutableSelfModPath_BlocksSelfModification()
    {
        // Arrange - the service protects its own source file
        var proposal = _sut.ProposeChange(
            "src/Ouroboros.Domain/Domain/SelfModification/GitReflectionService.cs",
            "d", "r", "old", "new", ChangeCategory.Feature, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("immutable");
    }

    [Fact]
    public async Task ApplyProposalAsync_HighRiskOutsideSafePaths_ReturnsFailure()
    {
        // Arrange - file outside safe paths with high risk
        var proposal = _sut.ProposeChange(
            "infrastructure/deploy.cs", "d", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.High);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not in safe modification paths");
    }

    [Fact]
    public async Task ApplyProposalAsync_OldCodeNotFound_SetsStatusToFailed()
    {
        // Arrange - create a file with content that does NOT match the old code
        string filePath = "src/Ouroboros.Domain/TestFile.cs";
        string fullPath = Path.Combine(_tempDir, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "completely different content");

        var proposal = _sut.ProposeChange(filePath, "d", "r", "nonexistent old code", "new code",
            ChangeCategory.BugFix, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Old code not found");
        _sut.Proposals.Single().Status.Should().Be(ProposalStatus.Failed);
    }

    [Fact]
    public async Task ApplyProposalAsync_ValidChange_AppliesSuccessfully()
    {
        // Arrange
        string filePath = "src/Ouroboros.Domain/TargetFile.cs";
        string fullPath = Path.Combine(_tempDir, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "// before\nprivate int _count = 0;\n// after");

        var proposal = _sut.ProposeChange(filePath, "Fix visibility", "Public API needed",
            "private int _count = 0;", "public int Count { get; set; } = 0;",
            ChangeCategory.BugFix, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        var result = await _sut.ApplyProposalAsync(proposal.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Applied");
        _sut.Proposals.Single().Status.Should().Be(ProposalStatus.Applied);

        // Verify file was actually modified
        string content = await File.ReadAllTextAsync(fullPath);
        content.Should().Contain("public int Count { get; set; } = 0;");
        content.Should().NotContain("private int _count = 0;");
    }

    [Fact]
    public async Task ApplyProposalAsync_ValidChange_CreatesBackup()
    {
        // Arrange
        string filePath = "src/Ouroboros.Domain/BackupTest.cs";
        string fullPath = Path.Combine(_tempDir, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "original content here");

        var proposal = _sut.ProposeChange(filePath, "d", "r",
            "original content here", "modified content here",
            ChangeCategory.Refactoring, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        // Act
        await _sut.ApplyProposalAsync(proposal.Id);

        // Assert - a backup file should exist
        string dir = Path.GetDirectoryName(fullPath)!;
        var backupFiles = Directory.GetFiles(dir, "*.backup.*");
        backupFiles.Should().NotBeEmpty();
    }

    // ----------------------------------------------------------------
    // SelfModifyAsync - End-to-end workflow
    // ----------------------------------------------------------------

    [Fact]
    public async Task SelfModifyAsync_AutoApproveDisabled_ReturnsFailureWithProposalId()
    {
        // Act
        var result = await _sut.SelfModifyAsync(
            "src/Ouroboros.Domain/File.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Feature,
            autoApprove: false);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Proposal");
    }

    [Fact]
    public async Task SelfModifyAsync_HighRiskAutoApprove_RequiresManualApproval()
    {
        // Arrange - security critical code triggers Critical risk
        string oldCode = "// safe";
        string newCode = "unsafe { ptr++; }";

        // Act
        var result = await _sut.SelfModifyAsync(
            "src/Ouroboros.Domain/File.cs", "desc", "rationale",
            oldCode, newCode, ChangeCategory.Security,
            autoApprove: true);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("manual approval");
    }

    // ----------------------------------------------------------------
    // AnalyzeFileAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task AnalyzeFileAsync_FileNotFound_ThrowsFileNotFound()
    {
        // Act
        Func<Task> act = async () =>
            await _sut.AnalyzeFileAsync("nonexistent.cs");

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AnalyzeFileAsync_CSharpFile_ExtractsClassesAndMethods()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "TestService.cs");
        string code = @"using System;
using System.Collections.Generic;

namespace Test;

public sealed class TestService
{
    private readonly ILogger _logger;

    public void DoWork()
    {
        // TODO: implement this
    }

    public async Task<string> GetDataAsync()
    {
        return """";
    }

    private int Calculate(int x) => x * 2;
}";
        await File.WriteAllTextAsync(filePath, code);

        // Act
        CodeAnalysis analysis = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        analysis.Classes.Should().Contain("TestService");
        analysis.Methods.Should().Contain("DoWork");
        analysis.Methods.Should().Contain("GetDataAsync");
        analysis.Methods.Should().Contain("Calculate");
        analysis.Usings.Should().Contain("System");
        analysis.Usings.Should().Contain("System.Collections.Generic");
        analysis.Todos.Should().NotBeEmpty();
        analysis.TotalLines.Should().BeGreaterThan(0);
        analysis.CodeLines.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AnalyzeFileAsync_DetectsNotImplementedException()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "Incomplete.cs");
        await File.WriteAllTextAsync(filePath, @"
public class Foo
{
    public void Bar() { throw new NotImplementedException(); }
}");

        // Act
        CodeAnalysis analysis = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        analysis.PotentialIssues.Should().Contain(i => i.Contains("NotImplementedException"));
    }

    [Fact]
    public async Task AnalyzeFileAsync_DetectsHackComment()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "Hacky.cs");
        await File.WriteAllTextAsync(filePath, @"
public class Foo
{
    // HACK: This is a workaround
    public void Bar() { }
}");

        // Act
        CodeAnalysis analysis = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        analysis.PotentialIssues.Should().Contain(i => i.Contains("HACK"));
    }

    [Fact]
    public async Task AnalyzeFileAsync_DetectsLongLines()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "LongLines.cs");
        string longLine = "public class Foo { " + new string('x', 250) + " }";
        await File.WriteAllTextAsync(filePath, longLine);

        // Act
        CodeAnalysis analysis = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        analysis.PotentialIssues.Should().Contain(i => i.Contains("long lines"));
    }

    [Fact]
    public async Task AnalyzeFileAsync_CommentRatio_CalculatedCorrectly()
    {
        // Arrange
        string filePath = Path.Combine(_tempDir, "Commented.cs");
        await File.WriteAllTextAsync(filePath, @"// Comment 1
// Comment 2
int x = 1;
int y = 2;");

        // Act
        CodeAnalysis analysis = await _sut.AnalyzeFileAsync(filePath);

        // Assert
        analysis.CommentRatio.Should().BeGreaterThan(0);
        analysis.CommentLines.Should().BeGreaterThanOrEqualTo(2);
    }

    // ----------------------------------------------------------------
    // ListSourceFilesAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ListSourceFilesAsync_ReturnsFilesInRepo()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "test.cs"), "class Foo {}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "readme.md"), "# Readme");

        // Act
        var files = await _sut.ListSourceFilesAsync();

        // Assert
        files.Should().NotBeEmpty();
        files.Should().Contain(f => f.RelativePath.EndsWith(".cs"));
    }

    [Fact]
    public async Task ListSourceFilesAsync_WithFilter_FiltersResults()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "alpha.cs"), "class Alpha {}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "beta.cs"), "class Beta {}");

        // Act
        var files = await _sut.ListSourceFilesAsync(filter: "alpha");

        // Assert
        files.Should().HaveCount(1);
        files.Single().RelativePath.Should().Contain("alpha");
    }

    [Fact]
    public async Task ListSourceFilesAsync_ExcludesBinAndObj()
    {
        // Arrange
        string binDir = Path.Combine(_tempDir, "bin", "Debug");
        Directory.CreateDirectory(binDir);
        await File.WriteAllTextAsync(Path.Combine(binDir, "output.cs"), "// compiled");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "source.cs"), "class Real {}");

        // Act
        var files = await _sut.ListSourceFilesAsync();

        // Assert
        files.Should().NotContain(f => f.RelativePath.Contains("bin"));
    }

    // ----------------------------------------------------------------
    // SearchCodeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task SearchCodeAsync_FindsMatchingLines()
    {
        // Arrange
        await File.WriteAllTextAsync(
            Path.Combine(_tempDir, "search.cs"),
            "public class Foo\n{\n    int bar = 42;\n}");

        // Act
        var results = await _sut.SearchCodeAsync("bar");

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.Content.Contains("bar"));
    }

    [Fact]
    public async Task SearchCodeAsync_RegexMode_MatchesPattern()
    {
        // Arrange
        await File.WriteAllTextAsync(
            Path.Combine(_tempDir, "regex.cs"),
            "public class MyService123 { }");

        // Act
        var results = await _sut.SearchCodeAsync(@"\w+Service\d+", isRegex: true);

        // Assert
        results.Should().NotBeEmpty();
    }

    // ----------------------------------------------------------------
    // ImmutablePaths and SafeModificationPaths
    // ----------------------------------------------------------------

    [Fact]
    public void ImmutablePaths_ContainsConstitutionPath()
    {
        GitReflectionService.ImmutablePaths
            .Should().Contain(p => p.Contains("Constitution"));
    }

    [Fact]
    public void ImmutablePaths_ContainsSovereigntyPath()
    {
        GitReflectionService.ImmutablePaths
            .Should().Contain(p => p.Contains("Sovereignty"));
    }

    [Fact]
    public void ImmutablePaths_ContainsEthicsPath()
    {
        GitReflectionService.ImmutablePaths
            .Should().Contain(p => p.Contains("Ethics"));
    }

    [Fact]
    public void SafeModificationPaths_ContainsDomainPath()
    {
        GitReflectionService.SafeModificationPaths
            .Should().Contain(p => p.Contains("Ouroboros.Domain"));
    }

    [Fact]
    public void SafeModificationPaths_ContainsDocsPath()
    {
        GitReflectionService.SafeModificationPaths
            .Should().Contain("docs");
    }

    [Fact]
    public void AllowedExtensions_ContainsCSharp()
    {
        GitReflectionService.AllowedExtensions
            .Should().Contain(".cs");
    }

    // ----------------------------------------------------------------
    // GetModificationSummary
    // ----------------------------------------------------------------

    [Fact]
    public void GetModificationSummary_EmptyProposals_ReturnsValidSummary()
    {
        // Act
        string summary = _sut.GetModificationSummary();

        // Assert
        summary.Should().NotBeNullOrWhiteSpace();
        summary.Should().Contain("SELF-MODIFICATION LOG");
    }

    [Fact]
    public void GetModificationSummary_WithProposals_IncludesProposalDetails()
    {
        // Arrange
        _sut.ProposeChange("a.cs", "Fix bug", "r", "old", "new",
            ChangeCategory.BugFix, RiskLevel.Low);
        _sut.ProposeChange("b.cs", "Add feature", "r", "old", "new",
            ChangeCategory.Feature, RiskLevel.Medium);

        // Act
        string summary = _sut.GetModificationSummary();

        // Assert
        summary.Should().Contain("Pending");
    }
}
