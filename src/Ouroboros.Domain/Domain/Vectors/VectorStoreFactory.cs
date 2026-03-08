using Ouroboros.Core.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Factory for creating vector store instances based on configuration.
/// </summary>
public class VectorStoreFactory
{
    private readonly VectorStoreConfiguration _config;
    private readonly ILogger? _logger;
    private readonly QdrantClient? _qdrantClient;
    private readonly IQdrantCollectionRegistry? _collectionRegistry;

    /// <summary>
    /// Initializes a new vector store factory with DI-provided Qdrant infrastructure.
    /// </summary>
    public VectorStoreFactory(
        VectorStoreConfiguration config,
        QdrantClient? qdrantClient = null,
        IQdrantCollectionRegistry? collectionRegistry = null,
        ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _qdrantClient = qdrantClient;
        _collectionRegistry = collectionRegistry;
        _logger = logger;
    }

    // Old (config, logger) constructor removed — the primary constructor
    // with optional QdrantClient/IQdrantCollectionRegistry covers all cases.

    /// <summary>
    /// Creates a vector store based on the configured type.
    /// </summary>
    public IVectorStore Create()
    {
        _logger?.LogInformation("Creating vector store of type: {Type}", _config.Type);

        return _config.Type.ToLowerInvariant() switch
        {
            "inmemory" => CreateInMemoryStore(),
            "qdrant" => CreateQdrantStore(),
            "pinecone" => CreatePineconeStore(),
            _ => throw new NotSupportedException($"Vector store type '{_config.Type}' is not supported")
        };
    }

    private IVectorStore CreateInMemoryStore()
    {
        _logger?.LogInformation("Creating in-memory vector store");
        return new TrackedVectorStore();
    }

    private IVectorStore CreateQdrantStore()
    {
        // Prefer DI-provided client + registry
        if (_qdrantClient != null && _collectionRegistry != null)
        {
            _logger?.LogInformation("Creating Qdrant vector store using DI-provided client");
            return new QdrantVectorStore(_qdrantClient, _collectionRegistry, _logger);
        }

        // Fallback to connection-string-based creation
        if (string.IsNullOrEmpty(_config.ConnectionString))
        {
            throw new InvalidOperationException("Connection string is required for Qdrant vector store");
        }

        _logger?.LogInformation("Creating Qdrant vector store with connection: {Connection}",
            MaskConnectionString(_config.ConnectionString));

        // Create a QdrantClient from the connection string, then use the DI-based constructor
        var uri = new Uri(_config.ConnectionString);
        var client = new QdrantClient(uri.Host, uri.Port > 0 ? uri.Port : 6334, uri.Scheme == "https");
        var registry = new QdrantCollectionRegistry(client);
        return new QdrantVectorStore(client, registry, _logger);
    }

    private IVectorStore CreatePineconeStore()
    {
        if (string.IsNullOrEmpty(_config.ConnectionString))
        {
            throw new InvalidOperationException("Connection string is required for Pinecone vector store");
        }

        _logger?.LogInformation("Creating Pinecone vector store with connection: {Connection}",
            MaskConnectionString(_config.ConnectionString));

        // FUTURE: Implement PineconeVectorStore when Pinecone package is added
        // Steps to implement:
        // 1. Add NuGet package: dotnet add package Pinecone.Client
        // 2. Create PineconeVectorStore class implementing IVectorStore
        // 3. Replace this exception with: return new PineconeVectorStore(_config.ConnectionString, _logger);
        throw new NotImplementedException(
            "Pinecone vector store implementation requires Pinecone SDK package. " +
            "Add the package and implement PineconeVectorStore class. " +
            "See docs/VECTOR_STORES.md for implementation guide.");
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Mask sensitive parts of connection string for logging
        if (connectionString.Length <= 10)
            return "***";

        return $"{connectionString[..5]}***{connectionString[^3..]}";
    }
}