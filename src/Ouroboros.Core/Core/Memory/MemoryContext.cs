namespace Ouroboros.Core.Memory;

/// <summary>
/// Memory-aware pipeline context that carries both data and memory state
/// </summary>
/// <typeparam name="T">The data type being processed</typeparam>
public record MemoryContext<T>(
    T Data,
    ConversationMemory Memory,
    Dictionary<string, object>? Properties = null)
{
    public Dictionary<string, object> Properties { get; } = Properties ?? new();

    /// <summary>
    /// Create a new context with updated data
    /// </summary>
    public MemoryContext<TNew> WithData<TNew>(TNew newData)
        => new(newData, Memory, Properties);

    /// <summary>
    /// Set a property value
    /// </summary>
    public MemoryContext<T> SetProperty(string key, object value)
    {
        Dictionary<string, object> newProperties = new Dictionary<string, object>(Properties)
        {
            [key] = value
        };
        return new MemoryContext<T>(Data, Memory, newProperties);
    }

    /// <summary>
    /// Get a property value
    /// </summary>
    public TValue? GetProperty<TValue>(string key)
        => Properties.TryGetValue(key, out object? value) && value is TValue typed
            ? typed
            : default;
}