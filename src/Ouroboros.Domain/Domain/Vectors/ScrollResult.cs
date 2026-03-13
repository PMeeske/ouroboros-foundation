using LangChain.DocumentLoaders;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Result of a scroll operation.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ScrollResult(
    IReadOnlyList<Document> Documents,
    string? NextOffset);