using LangChain.DocumentLoaders;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Result of a scroll operation.
/// </summary>
public sealed record ScrollResult(
    IReadOnlyList<Document> Documents,
    string? NextOffset);