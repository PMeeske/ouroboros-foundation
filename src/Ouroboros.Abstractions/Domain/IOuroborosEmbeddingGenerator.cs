using Microsoft.Extensions.AI;

namespace Ouroboros.Abstractions.Domain;

/// <summary>
/// Unified embedding generator aligned with MEAI
/// <see cref="IEmbeddingGenerator{String, Embedding}"/>.
/// Migration target for <see cref="IEmbeddingModel"/>.
/// </summary>
public interface IOuroborosEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
}
