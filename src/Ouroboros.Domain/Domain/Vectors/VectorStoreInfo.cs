using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Information about a vector store.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record VectorStoreInfo(
    string Name,
    ulong VectorCount,
    int VectorDimension,
    string Status,
    IDictionary<string, object>? AdditionalInfo = null);