namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Interface for Speech-to-Text models.
/// </summary>
public interface ISttModel
{
    /// <summary>
    /// Gets the model name/identifier.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Transcribes an audio file.
    /// </summary>
    Task<Result<TranscriptionResult, string>> TranscribeAsync(
        string audioFilePath,
        string? language = null,
        CancellationToken ct = default);

    /// <summary>
    /// Transcribes audio bytes.
    /// </summary>
    Task<Result<TranscriptionResult, string>> TranscribeAsync(
        byte[] audioData,
        string format,
        int sampleRate,
        string? language = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a streaming transcription session.
    /// </summary>
    IStreamingTranscription CreateStreamingSession(string? language = null);

    /// <summary>
    /// Gets whether this model supports streaming.
    /// </summary>
    bool SupportsStreaming { get; }
}