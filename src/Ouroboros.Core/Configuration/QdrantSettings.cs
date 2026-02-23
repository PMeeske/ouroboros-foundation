namespace Ouroboros.Core.Configuration;

/// <summary>
/// Centralized Qdrant configuration bound from appsettings "Ouroboros:Qdrant".
/// Single source of truth for connection parameters across all layers.
/// </summary>
public sealed class QdrantSettings
{
    /// <summary>
    /// Configuration section path in appsettings.json.
    /// </summary>
    public const string SectionPath = "Ouroboros:Qdrant";

    /// <summary>
    /// Qdrant server host.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Qdrant gRPC port (used by IQdrantClient).
    /// </summary>
    public int GrpcPort { get; set; } = 6334;

    /// <summary>
    /// Qdrant HTTP/REST port (used by health checks).
    /// </summary>
    public int HttpPort { get; set; } = 6333;

    /// <summary>
    /// Whether to use HTTPS for connections.
    /// </summary>
    public bool UseHttps { get; set; }

    /// <summary>
    /// Optional API key for authenticated Qdrant instances.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Default vector dimension when creating collections.
    /// </summary>
    public int DefaultVectorSize { get; set; } = 768;

    /// <summary>
    /// Builds the gRPC endpoint URI string.
    /// </summary>
    public string GrpcEndpoint => $"{(UseHttps ? "https" : "http")}://{Host}:{GrpcPort}";

    /// <summary>
    /// Builds the HTTP/REST endpoint URI string.
    /// </summary>
    public string HttpEndpoint => $"{(UseHttps ? "https" : "http")}://{Host}:{HttpPort}";
}
