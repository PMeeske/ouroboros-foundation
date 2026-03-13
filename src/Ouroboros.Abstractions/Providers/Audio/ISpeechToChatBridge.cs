// <copyright file="ISpeechToChatBridge.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Bridges live microphone audio into the chat subsystem by coordinating
/// audio capture, voice activity detection, speech-to-text transcription,
/// and event-driven text delivery.
/// </summary>
public interface ISpeechToChatBridge : IAsyncDisposable
{
    /// <summary>Gets the current input mode (off, push-to-talk, wake-word, always-on).</summary>
    SpeechInputMode Mode { get; }

    /// <summary>Gets a value indicating whether the bridge is actively listening for speech.</summary>
    bool IsListening { get; }

    /// <summary>
    /// Sets the input mode. Changing mode while running may restart the pipeline.
    /// </summary>
    /// <param name="mode">The desired speech input mode.</param>
    void SetMode(SpeechInputMode mode);

    /// <summary>
    /// Starts the bridge pipeline. Audio frames flow through VAD and STT;
    /// transcribed text is emitted via <see cref="SpeechTranscribed"/>.
    /// </summary>
    /// <param name="ct">Cancellation token for cooperative shutdown.</param>
    /// <returns>A task that completes once the pipeline is running.</returns>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops the bridge pipeline and releases audio resources.
    /// </summary>
    /// <returns>A task that completes when shutdown is finished.</returns>
    Task StopAsync();

    /// <summary>
    /// Raised when transcribed text is ready to send to the chat subsystem.
    /// </summary>
    event EventHandler<SpeechTranscribedEventArgs>? SpeechTranscribed;

    /// <summary>
    /// Raised when the bridge transitions between speech-processing states.
    /// </summary>
    event EventHandler<SpeechStateChangedEventArgs>? StateChanged;
}
