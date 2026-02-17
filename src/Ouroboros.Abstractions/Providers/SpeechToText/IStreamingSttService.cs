// <copyright file="IStreamingSttService.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Extended STT interface that supports reactive streaming transcription.
/// Enables real-time voice input processing.
/// </summary>
public interface IStreamingSttService : ISpeechToTextService
{
    /// <summary>
    /// Creates a continuous transcription stream from audio chunks.
    /// Emits partial results as speech is recognized in real-time.
    /// </summary>
    /// <param name="audioStream">Observable of audio chunks.</param>
    /// <param name="options">Streaming transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Observable of transcription events (partial and final).</returns>
    IObservable<TranscriptionEvent> StreamTranscription(
        IObservable<AudioChunk> audioStream,
        StreamingTranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Voice activity detection stream - detects speech start/end events.
    /// Useful for determining when to start/stop recording.
    /// </summary>
    /// <param name="audioStream">Observable of audio chunks.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Observable of voice activity events.</returns>
    IObservable<VoiceActivityEvent> DetectVoiceActivity(
        IObservable<AudioChunk> audioStream,
        CancellationToken ct = default);

    /// <summary>
    /// Starts a streaming transcription session that can be fed audio chunks.
    /// Returns a session that can be used to push audio and receive results.
    /// </summary>
    /// <param name="options">Streaming transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A streaming session for interactive transcription.</returns>
    Task<IStreamingTranscriptionSession> StartStreamingSessionAsync(
        StreamingTranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets whether streaming transcription is supported by this provider.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets whether voice activity detection is supported.
    /// </summary>
    bool SupportsVoiceActivityDetection { get; }
}