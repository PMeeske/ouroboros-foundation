using Microsoft.Extensions.AI;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Core;

/// <summary>
/// Optional interface for providers that natively back an <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/>.
/// Providers implementing this return their underlying MEAI embedding generator directly.
/// </summary>
[ExcludeFromCodeCoverage]
public interface IEmbeddingGeneratorBridge
{
    /// <summary>
    /// Returns the native MEAI embedding generator backing this provider.
    /// </summary>
    IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingGenerator();
}
