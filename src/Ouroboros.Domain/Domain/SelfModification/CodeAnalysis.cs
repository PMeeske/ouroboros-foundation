namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Analysis of code for reflection purposes.
/// </summary>
public sealed record CodeAnalysis(
    string FilePath,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> Methods,
    IReadOnlyList<string> Usings,
    int TotalLines,
    int CodeLines,
    int CommentLines,
    double CommentRatio,
    IReadOnlyList<string> Todos,
    IReadOnlyList<string> PotentialIssues);