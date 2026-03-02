namespace Ouroboros.Core.Configuration;

/// <summary>Default service endpoint constants.</summary>
public static class DefaultEndpoints
{
    /// <summary>Default Ollama HTTP endpoint.</summary>
    public const string Ollama = "http://localhost:11434";

    /// <summary>Default Qdrant HTTP/REST endpoint.</summary>
    public const string Qdrant = "http://localhost:6333";

    /// <summary>Default Qdrant gRPC endpoint.</summary>
    public const string QdrantGrpc = "http://localhost:6334";
}
