using Microsoft.Extensions.AI;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Domain;

/// <summary>
/// Unified embedding generator aligned with MEAI
/// <see cref="IEmbeddingGenerator{String, Embedding}"/>.
/// Migration target for <see cref="IEmbeddingModel"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public interface IOuroborosEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
}
