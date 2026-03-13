using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Information about a file in the repository.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record RepoFileInfo(
    string RelativePath,
    string FullPath,
    long SizeBytes,
    DateTime LastModified,
    int LineCount,
    string Language);