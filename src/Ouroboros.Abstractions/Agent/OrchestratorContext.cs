namespace Ouroboros.Agent;

/// <summary>
/// Represents the execution context for an orchestration operation.
/// Provides a unified way to pass context information across all orchestrators.
/// </summary>
public sealed record OrchestratorContext(
    string OperationId,
    Dictionary<string, object> Metadata,
    CancellationToken CancellationToken = default)
{
    /// <summary>
    /// Creates a new orchestrator context with generated operation ID.
    /// </summary>
    public static OrchestratorContext Create(
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default) =>
        new OrchestratorContext(
            Guid.NewGuid().ToString(),
            metadata ?? new Dictionary<string, object>(),
            ct);

    /// <summary>
    /// Gets a value from metadata or returns default.
    /// </summary>
    public T? GetMetadata<T>(string key, T? defaultValue = default) =>
        Metadata.TryGetValue(key, out var value) && value is T typed
            ? typed
            : defaultValue;

    /// <summary>
    /// Creates a new context with additional metadata.
    /// </summary>
    public OrchestratorContext WithMetadata(string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(Metadata) { [key] = value };
        return this with { Metadata = newMetadata };
    }
}