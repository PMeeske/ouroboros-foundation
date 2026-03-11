// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.SelfModification;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

/// <summary>
/// Tests for GitReflectionService.Proposals.cs — change proposals, risk assessment,
/// approval/rejection lifecycle, and modification summary.
/// </summary>
[Trait("Category", "Unit")]
public class GitReflectionServiceProposalsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly GitReflectionService _sut;

    public GitReflectionServiceProposalsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-proposals-" + Guid.NewGuid().ToString("N")[..8]);
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
    // ProposeChange
    // ----------------------------------------------------------------

    [Fact]
    public void ProposeChange_CreatesProposalWithCorrectFields()
    {
        var proposal = _sut.ProposeChange(
            "src/Test.cs",
            "Add logging",
            "Better observability",
            "old code",
            "new code",
            ChangeCategory.Enhancement,
            RiskLevel.Low);

        proposal.Should().NotBeNull();
        proposal.FilePath.Should().Be("src/Test.cs");
        proposal.Description.Should().Be("Add logging");
        proposal.Rationale.Should().Be("Better observability");
        proposal.OldCode.Should().Be("old code");
        proposal.NewCode.Should().Be("new code");
        proposal.Category.Should().Be(ChangeCategory.Enhancement);
        proposal.Risk.Should().Be(RiskLevel.Low);
        proposal.Status.Should().Be(ProposalStatus.Pending);
        proposal.Id.Should().HaveLength(8);
    }

    [Fact]
    public void ProposeChange_AddsToProposalsList()
    {
        _sut.ProposeChange("file.cs", "desc", "rationale", "old", "new",
            ChangeCategory.BugFix, RiskLevel.Low);

        _sut.Proposals.Should().HaveCount(1);
    }

    [Fact]
    public void ProposeChange_MultipleProposals_AllTracked()
    {
        for (int i = 0; i < 5; i++)
        {
            _sut.ProposeChange($"file{i}.cs", $"desc{i}", "rationale", "old", "new",
                ChangeCategory.Enhancement, RiskLevel.Low);
        }

        _sut.Proposals.Should().HaveCount(5);
    }

    // ----------------------------------------------------------------
    // ApproveProposal
    // ----------------------------------------------------------------

    [Fact]
    public void ApproveProposal_ValidId_ReturnsTrue()
    {
        var proposal = _sut.ProposeChange("file.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Enhancement, RiskLevel.Low);

        bool result = _sut.ApproveProposal(proposal.Id, "Looks good");

        result.Should().BeTrue();
        _sut.Proposals.First(p => p.Id == proposal.Id).Status.Should().Be(ProposalStatus.Approved);
    }

    [Fact]
    public void ApproveProposal_InvalidId_ReturnsFalse()
    {
        bool result = _sut.ApproveProposal("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveProposal_WithComment_SetsReviewComment()
    {
        var proposal = _sut.ProposeChange("file.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Enhancement, RiskLevel.Low);

        _sut.ApproveProposal(proposal.Id, "Great work!");

        _sut.Proposals.First(p => p.Id == proposal.Id).ReviewComment.Should().Be("Great work!");
    }

    // ----------------------------------------------------------------
    // RejectProposal
    // ----------------------------------------------------------------

    [Fact]
    public void RejectProposal_ValidId_ReturnsTrue()
    {
        var proposal = _sut.ProposeChange("file.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Enhancement, RiskLevel.Low);

        bool result = _sut.RejectProposal(proposal.Id, "Too risky");

        result.Should().BeTrue();
        _sut.Proposals.First(p => p.Id == proposal.Id).Status.Should().Be(ProposalStatus.Rejected);
    }

    [Fact]
    public void RejectProposal_InvalidId_ReturnsFalse()
    {
        bool result = _sut.RejectProposal("nonexistent", "reason");

        result.Should().BeFalse();
    }

    [Fact]
    public void RejectProposal_SetsReviewComment()
    {
        var proposal = _sut.ProposeChange("file.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Enhancement, RiskLevel.Low);

        _sut.RejectProposal(proposal.Id, "Not aligned with goals");

        _sut.Proposals.First(p => p.Id == proposal.Id).ReviewComment.Should().Be("Not aligned with goals");
    }

    // ----------------------------------------------------------------
    // ApplyProposalAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ApplyProposalAsync_NotFound_ReturnsFailure()
    {
        var result = await _sut.ApplyProposalAsync("nonexistent");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ApplyProposalAsync_NotApproved_ReturnsFailure()
    {
        var proposal = _sut.ProposeChange("file.cs", "desc", "rationale",
            "old", "new", ChangeCategory.Enhancement, RiskLevel.Low);

        var result = await _sut.ApplyProposalAsync(proposal.Id);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not approved");
    }

    [Fact]
    public async Task ApplyProposalAsync_ImmutablePath_ReturnsFailure()
    {
        var proposal = _sut.ProposeChange(
            "src/Ouroboros.Core/Ethics/EthicsFramework.cs",
            "desc", "rationale", "old", "new",
            ChangeCategory.Enhancement, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        var result = await _sut.ApplyProposalAsync(proposal.Id);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("constitutionally immutable");
    }

    [Fact]
    public async Task ApplyProposalAsync_Approved_WithValidFile_AppliesChange()
    {
        // Create a file with known content
        string filePath = Path.Combine(_tempDir, "src", "Ouroboros.Domain", "Target.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "old code here");

        var proposal = _sut.ProposeChange(
            "src/Ouroboros.Domain/Target.cs",
            "Replace code", "Better code",
            "old code", "new code",
            ChangeCategory.Enhancement, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        var result = await _sut.ApplyProposalAsync(proposal.Id);

        result.Success.Should().BeTrue();
        string newContent = await File.ReadAllTextAsync(filePath);
        newContent.Should().Contain("new code");
    }

    [Fact]
    public async Task ApplyProposalAsync_OldCodeNotFound_MarksFailed()
    {
        string filePath = Path.Combine(_tempDir, "src", "Ouroboros.Domain", "NoMatch.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "different content");

        var proposal = _sut.ProposeChange(
            "src/Ouroboros.Domain/NoMatch.cs",
            "Replace code", "rationale",
            "nonexistent code", "new code",
            ChangeCategory.Enhancement, RiskLevel.Low);
        _sut.ApproveProposal(proposal.Id);

        var result = await _sut.ApplyProposalAsync(proposal.Id);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // ----------------------------------------------------------------
    // AssessRisk via reflection
    // ----------------------------------------------------------------

    private static RiskLevel InvokeAssessRisk(string filePath, string oldCode, string newCode)
    {
        MethodInfo? method = typeof(GitReflectionService)
            .GetMethod("AssessRisk", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();
        return (RiskLevel)method!.Invoke(null, new object[] { filePath, oldCode, newCode })!;
    }

    [Fact]
    public void AssessRisk_MarkdownFile_ReturnsLow()
    {
        RiskLevel risk = InvokeAssessRisk("docs/README.md", "old", "new");
        risk.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public void AssessRisk_TextFile_ReturnsLow()
    {
        RiskLevel risk = InvokeAssessRisk("notes.txt", "old", "new");
        risk.Should().Be(RiskLevel.Low);
    }

    [Fact]
    public void AssessRisk_VisibilityChange_ReturnsHigh()
    {
        RiskLevel risk = InvokeAssessRisk("file.cs", "private void Method()", "public void Method()");
        risk.Should().Be(RiskLevel.High);
    }

    [Fact]
    public void AssessRisk_RemovingAsync_ReturnsHigh()
    {
        RiskLevel risk = InvokeAssessRisk("file.cs", "async Task DoWork()", "void DoWork()");
        risk.Should().Be(RiskLevel.High);
    }

    [Fact]
    public void AssessRisk_RemovingErrorHandling_ReturnsHigh()
    {
        RiskLevel risk = InvokeAssessRisk("file.cs", "try { } catch { }", "DoSomething();");
        risk.Should().Be(RiskLevel.High);
    }

    [Fact]
    public void AssessRisk_UnsafeCode_ReturnsCritical()
    {
        RiskLevel risk = InvokeAssessRisk("file.cs", "old code", "unsafe { }");
        risk.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void AssessRisk_ProcessStart_ReturnsCritical()
    {
        RiskLevel risk = InvokeAssessRisk("file.cs", "old code", "Process.Start(\"cmd\")");
        risk.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void AssessRisk_LargeChange_ReturnsMedium()
    {
        string oldCode = "small";
        string newCode = new string('x', 600);
        RiskLevel risk = InvokeAssessRisk("file.cs", oldCode, newCode);
        risk.Should().Be(RiskLevel.Medium);
    }

    // ----------------------------------------------------------------
    // GetModificationSummary
    // ----------------------------------------------------------------

    [Fact]
    public void GetModificationSummary_EmptyProposals_ReturnsFormattedOutput()
    {
        string summary = _sut.GetModificationSummary();

        summary.Should().Contain("SELF-MODIFICATION LOG");
    }

    [Fact]
    public void GetModificationSummary_WithProposals_IncludesProposals()
    {
        _sut.ProposeChange("file.cs", "Test change", "rationale", "old", "new",
            ChangeCategory.Enhancement, RiskLevel.Low);

        string summary = _sut.GetModificationSummary();

        summary.Should().Contain("Test change");
    }

    // ----------------------------------------------------------------
    // Dispose
    // ----------------------------------------------------------------

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var service = new GitReflectionService(_tempDir);
        service.Dispose();

        Action act = () => service.Dispose();
        act.Should().NotThrow();
    }
}
