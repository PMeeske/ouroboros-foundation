using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Result of a Git operation.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GitOperationResult(
    bool Success,
    string Message,
    string? CommitHash = null,
    string? BranchName = null,
    IReadOnlyList<string>? AffectedFiles = null);