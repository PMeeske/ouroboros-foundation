namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Interface for Text-to-Speech models.
/// </summary>
public interface ITtsModel
{
    /// <summary>
    /// Gets the model name/identifier.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Gets available voices.
    /// </summary>
    Task<Result<IReadOnlyList<VoiceInfo>, string>> GetVoicesAsync(
        string? language = null,
        CancellationToken ct = default);

    /// <summary>
    /// Synthesizes speech from text.
    /// </summary>
    Task<Result<SynthesizedSpeech, string>> SynthesizeAsync(
        string text,
        VoiceConfig? config = null,
        CancellationToken ct = default);

    /// <summary>
    /// Synthesizes speech with streaming output.
    /// </summary>
    IObservable<byte[]> SynthesizeStreaming(
        string text,
        VoiceConfig? config = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets whether this model supports streaming output.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets whether this model supports emotional expression.
    /// </summary>
    bool SupportsEmotions { get; }
}