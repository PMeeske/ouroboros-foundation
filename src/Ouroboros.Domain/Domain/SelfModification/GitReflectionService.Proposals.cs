// <copyright file="GitReflectionService.Proposals.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using System.Text.RegularExpressions;

namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Change proposal management and branch operations.
/// </summary>
public sealed partial class GitReflectionService
{
    /// <summary>
    /// Creates a new branch for self-modification.
    /// </summary>
    public async Task<GitOperationResult> CreateBranchAsync(
        string branchName,
        bool checkout = true,
        CancellationToken ct = default)
    {
        // Sanitize branch name
        string safeName = BranchNameSanitizerRegex().Replace(branchName, "-").ToLowerInvariant();
        string fullName = $"ouroboros/self-modify/{safeName}";

        string[] args = checkout
            ? ["checkout", "-b", fullName]
            : ["branch", fullName];
        (bool success, _, string error) = await ExecuteGitAsync(args, ct).ConfigureAwait(false);

        return new GitOperationResult(
            success,
            success ? $"Created branch: {fullName}" : $"Failed: {error}",
            BranchName: fullName);
    }

    /// <summary>
    /// Stages files for commit.
    /// </summary>
    public async Task<GitOperationResult> StageFilesAsync(
        IEnumerable<string> files,
        CancellationToken ct = default)
    {
        List<string> staged = new();
        foreach (string file in files)
        {
            (bool success, _, _) = await ExecuteGitAsync(["add", "--", file], ct).ConfigureAwait(false);
            if (success)
            {
                staged.Add(file);
            }
        }

        return new GitOperationResult(
            staged.Count > 0,
            $"Staged {staged.Count} files",
            AffectedFiles: staged);
    }

    /// <summary>
    /// Commits staged changes.
    /// </summary>
    public async Task<GitOperationResult> CommitAsync(
        string message,
        CancellationToken ct = default)
    {
        // Add Ouroboros signature to commit message
        string fullMessage = $"[Ouroboros Self-Modification] {message}";

        (bool success, string output, string error) = await ExecuteGitAsync(["commit", "-m", fullMessage], ct).ConfigureAwait(false);

        // Extract commit hash
        string? hash = null;
        if (success)
        {
            Match match = CommitHashRegex().Match(output);
            if (match.Success)
            {
                hash = match.Groups[1].Value;
            }
        }

        return new GitOperationResult(
            success,
            success ? $"Committed: {hash}" : $"Commit failed: {error}",
            CommitHash: hash);
    }

    /// <summary>
    /// Proposes a code change for review.
    /// </summary>
    public CodeChangeProposal ProposeChange(
        string filePath,
        string description,
        string rationale,
        string oldCode,
        string newCode,
        ChangeCategory category,
        RiskLevel risk)
    {
        CodeChangeProposal proposal = new(
            Id: Guid.NewGuid().ToString("N")[..8],
            FilePath: filePath,
            Description: description,
            Rationale: rationale,
            OldCode: oldCode,
            NewCode: newCode,
            Category: category,
            Risk: risk,
            ProposedAt: DateTime.UtcNow);

        _proposals.Add(proposal);
        return proposal;
    }

    /// <summary>
    /// Reviews and approves a change proposal.
    /// </summary>
    public bool ApproveProposal(string proposalId, string? comment = null)
    {
        int index = _proposals.FindIndex(p => p.Id == proposalId);
        if (index < 0) return false;

        _proposals[index] = _proposals[index] with
        {
            Status = ProposalStatus.Approved,
            ReviewComment = comment,
        };
        return true;
    }

    /// <summary>
    /// Rejects a change proposal.
    /// </summary>
    public bool RejectProposal(string proposalId, string reason)
    {
        int index = _proposals.FindIndex(p => p.Id == proposalId);
        if (index < 0) return false;

        _proposals[index] = _proposals[index] with
        {
            Status = ProposalStatus.Rejected,
            ReviewComment = reason,
        };
        return true;
    }

    /// <summary>
    /// Applies an approved change proposal.
    /// </summary>
    public async Task<GitOperationResult> ApplyProposalAsync(
        string proposalId,
        bool autoCommit = false,
        CancellationToken ct = default)
    {
        CodeChangeProposal? proposal = _proposals.FirstOrDefault(p => p.Id == proposalId);
        if (proposal == null)
        {
            return new GitOperationResult(false, $"Proposal {proposalId} not found");
        }

        if (proposal.Status != ProposalStatus.Approved)
        {
            return new GitOperationResult(false, $"Proposal {proposalId} is not approved (status: {proposal.Status})");
        }

        // Validate the file path is not constitutionally immutable
        string relativePath = proposal.FilePath.Replace('\\', '/');
        bool isImmutable = ImmutablePaths.Any(p => relativePath.Contains(p, StringComparison.OrdinalIgnoreCase));
        if (isImmutable)
        {
            return new GitOperationResult(false,
                $"Cannot modify {proposal.FilePath} — path is constitutionally immutable.");
        }

        // Validate the file path is in the safe modification list
        bool isSafe = SafeModificationPaths.Any(p => relativePath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (!isSafe && proposal.Risk >= RiskLevel.High)
        {
            return new GitOperationResult(false, $"Cannot modify {proposal.FilePath} - not in safe modification paths");
        }

        string fullPath = Path.IsPathRooted(proposal.FilePath)
            ? proposal.FilePath
            : Path.Combine(_repoRoot, proposal.FilePath);

        try
        {
            // Read current content
            string content = await File.ReadAllTextAsync(fullPath, ct).ConfigureAwait(false);

            // Verify the old code exists
            if (!content.Contains(proposal.OldCode))
            {
                int idx = _proposals.FindIndex(p => p.Id == proposalId);
                if (idx >= 0)
                {
                    _proposals[idx] = _proposals[idx] with { Status = ProposalStatus.Failed };
                }
                return new GitOperationResult(false, "Old code not found in file - may have already been modified");
            }

            // Create backup
            string backupPath = fullPath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
            await File.WriteAllTextAsync(backupPath, content, ct).ConfigureAwait(false);

            // Apply change
            string newContent = content.Replace(proposal.OldCode, proposal.NewCode);
            await File.WriteAllTextAsync(fullPath, newContent, ct).ConfigureAwait(false);

            // Update proposal status
            int index = _proposals.FindIndex(p => p.Id == proposalId);
            if (index >= 0)
            {
                _proposals[index] = _proposals[index] with { Status = ProposalStatus.Applied };
            }

            // Auto-commit if requested
            if (autoCommit)
            {
                await StageFilesAsync(new[] { proposal.FilePath }, ct).ConfigureAwait(false);
                await CommitAsync($"{proposal.Category}: {proposal.Description}", ct).ConfigureAwait(false);
            }

            return new GitOperationResult(
                true,
                $"Applied change to {proposal.FilePath}",
                AffectedFiles: new[] { proposal.FilePath });
        }
        catch (IOException ex)
        {
            int index = _proposals.FindIndex(p => p.Id == proposalId);
            if (index >= 0)
            {
                _proposals[index] = _proposals[index] with { Status = ProposalStatus.Failed };
            }
            return new GitOperationResult(false, $"Failed to apply change: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            int index = _proposals.FindIndex(p => p.Id == proposalId);
            if (index >= 0)
            {
                _proposals[index] = _proposals[index] with { Status = ProposalStatus.Failed };
            }
            return new GitOperationResult(false, $"Failed to apply change: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete workflow: propose, review, apply, commit a change.
    /// </summary>
    public async Task<GitOperationResult> SelfModifyAsync(
        string filePath,
        string description,
        string rationale,
        string oldCode,
        string newCode,
        ChangeCategory category,
        bool autoApprove = false,
        CancellationToken ct = default)
    {
        // Assess risk
        RiskLevel risk = AssessRisk(filePath, oldCode, newCode);

        // Create proposal
        CodeChangeProposal proposal = ProposeChange(filePath, description, rationale, oldCode, newCode, category, risk);

        // Auto-approve only low-risk changes
        if (autoApprove && risk <= RiskLevel.Low)
        {
            ApproveProposal(proposal.Id, "Auto-approved (low risk)");
        }
        else if (autoApprove && risk == RiskLevel.Medium)
        {
            ApproveProposal(proposal.Id, "Auto-approved (medium risk - review recommended)");
        }
        else if (!autoApprove)
        {
            return new GitOperationResult(
                false,
                $"Proposal {proposal.Id} created (risk: {risk}). Call ApproveProposal to approve.");
        }
        else
        {
            return new GitOperationResult(
                false,
                $"High-risk change requires manual approval. Proposal ID: {proposal.Id}");
        }

        // Create branch for the change
        string branchName = $"{category.ToString().ToLowerInvariant()}-{proposal.Id}";
        _ = await CreateBranchAsync(branchName, ct: ct).ConfigureAwait(false);

        // Apply the change
        GitOperationResult applyResult = await ApplyProposalAsync(proposal.Id, autoCommit: true, ct: ct).ConfigureAwait(false);

        return applyResult;
    }

    /// <summary>
    /// Assesses the risk level of a proposed change.
    /// </summary>
    private static RiskLevel AssessRisk(string filePath, string oldCode, string newCode)
    {
        // Documentation changes are low risk
        if (filePath.EndsWith(".md") || filePath.EndsWith(".txt"))
        {
            return RiskLevel.Low;
        }

        // Check for high-risk patterns
        if (oldCode.Contains("private") && newCode.Contains("public"))
        {
            return RiskLevel.High; // Visibility change
        }

        if (AsyncTaskRegex().IsMatch(oldCode) && !AsyncTaskRegex().IsMatch(newCode))
        {
            return RiskLevel.High; // Removing async
        }

        if (oldCode.Contains("try") && !newCode.Contains("try"))
        {
            return RiskLevel.High; // Removing error handling
        }

        // Security patterns
        if (newCode.Contains("unsafe") || newCode.Contains("Process.Start") ||
            newCode.Contains("File.Delete") || newCode.Contains("Directory.Delete"))
        {
            return RiskLevel.Critical;
        }

        // Size of change
        int sizeDiff = Math.Abs(newCode.Length - oldCode.Length);
        if (sizeDiff > 500)
        {
            return RiskLevel.Medium;
        }

        // Comments and whitespace only
        if (WhitespaceAndCommentsRegex().Replace(oldCode, "") == WhitespaceAndCommentsRegex().Replace(newCode, ""))
        {
            return RiskLevel.Low;
        }

        return RiskLevel.Medium;
    }

    /// <summary>
    /// Generates a summary of self-modification activity.
    /// </summary>
    public string GetModificationSummary()
    {
        StringBuilder sb = new();
        sb.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║           OUROBOROS SELF-MODIFICATION LOG                     ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");

        IEnumerable<IGrouping<ProposalStatus, CodeChangeProposal>> byStatus = _proposals.GroupBy(p => p.Status);
        foreach (IGrouping<ProposalStatus, CodeChangeProposal> group in byStatus)
        {
            sb.AppendLine($"║  {group.Key,-15}: {group.Count(),5} proposals                         ║");
        }

        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");

        foreach (CodeChangeProposal p in _proposals.OrderByDescending(p => p.ProposedAt).Take(5))
        {
            string status = p.Status switch
            {
                ProposalStatus.Applied => "✅",
                ProposalStatus.Approved => "🔵",
                ProposalStatus.Rejected => "❌",
                ProposalStatus.Failed => "⚠️",
                _ => "⏳",
            };
            sb.AppendLine($"║  {status} [{p.Id}] {p.Description.Truncate(40),-40}  ║");
        }

        sb.AppendLine("╚═══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    // GeneratedRegex patterns for proposal operations

    [GeneratedRegex(@"[^a-zA-Z0-9_-]")]
    private static partial Regex BranchNameSanitizerRegex();

    [GeneratedRegex(@"\[[\w/]+\s+([a-f0-9]+)\]")]
    private static partial Regex CommitHashRegex();

    [GeneratedRegex(@"async\s+Task")]
    private static partial Regex AsyncTaskRegex();

    [GeneratedRegex(@"[\s//*]+")]
    private static partial Regex WhitespaceAndCommentsRegex();
}
