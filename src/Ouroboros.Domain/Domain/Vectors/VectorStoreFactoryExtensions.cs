using Microsoft.Extensions.Logging;
using Ouroboros.Core.Configuration;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Extension methods for vector store factory.
/// </summary>
public static class VectorStoreFactoryExtensions
{
    /// <summary>
    /// Creates a vector store factory from pipeline configuration.
    /// </summary>
    public static VectorStoreFactory CreateVectorStoreFactory(
        this PipelineConfiguration config,
        ILogger? logger = null)
    {
        return new VectorStoreFactory(config.VectorStore, logger);
    }
}