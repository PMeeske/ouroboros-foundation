namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Perception data from a sensor.
/// </summary>
/// <param name="SensorId">Source sensor identifier.</param>
/// <param name="Modality">Perception modality.</param>
/// <param name="Timestamp">When the perception occurred.</param>
/// <param name="Data">Raw perception data.</param>
/// <param name="Metadata">Additional metadata.</param>
public sealed record PerceptionData(
    string SensorId,
    SensorModality Modality,
    DateTime Timestamp,
    object Data,
    IReadOnlyDictionary<string, object>? Metadata = null)
{
    /// <summary>
    /// Gets the data as a specific type.
    /// </summary>
    public T? GetDataAs<T>() where T : class => Data as T;

    /// <summary>
    /// Gets the data as bytes (for raw frames/audio).
    /// </summary>
    public byte[]? GetBytes() => Data as byte[];
}