namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// A streaming transcription session for interactive audio input.
/// </summary>
public interface IStreamingTranscriptionSession : IAsyncDisposable
{
    /// <summary>
    /// Gets the observable stream of transcription results.
    /// </summary>
    IObservable<TranscriptionEvent> Results { get; }

    /// <summary>
    /// Gets the observable stream of voice activity events.
    /// </summary>
    IObservable<VoiceActivityEvent> VoiceActivity { get; }

    /// <summary>
    /// Pushes an audio chunk to the transcription session.
    /// </summary>
    /// <param name="chunk">The audio chunk to process.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PushAudioAsync(AudioChunk chunk, CancellationToken ct = default);

    /// <summary>
    /// Signals that no more audio will be sent, triggering final processing.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task EndAudioAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the accumulated final transcription text.
    /// </summary>
    string AccumulatedText { get; }

    /// <summary>
    /// Gets whether the session is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Resets the session for a new transcription.
    /// </summary>
    void Reset();
}