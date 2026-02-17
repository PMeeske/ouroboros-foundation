namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Interface for streaming transcription.
/// </summary>
public interface IStreamingTranscription : IAsyncDisposable
{
    /// <summary>
    /// Observable stream of transcription results.
    /// </summary>
    IObservable<TranscriptionResult> Results { get; }

    /// <summary>
    /// Observable stream of voice activity.
    /// </summary>
    IObservable<VoiceActivity> VoiceActivity { get; }

    /// <summary>
    /// Pushes audio data to the transcription stream.
    /// </summary>
    Task PushAudioAsync(byte[] audioData, CancellationToken ct = default);

    /// <summary>
    /// Signals end of audio stream.
    /// </summary>
    Task EndAudioAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets accumulated final transcript.
    /// </summary>
    string AccumulatedTranscript { get; }

    /// <summary>
    /// Gets whether the session is active.
    /// </summary>
    bool IsActive { get; }
}