using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Maps memory layers to their underlying collections.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record MemoryLayerMapping(
    MemoryLayer Layer,
    IReadOnlyList<string> Collections,
    string Description,
    double RetentionPriority);