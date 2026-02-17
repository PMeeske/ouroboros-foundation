// <copyright file="IStreamingTtsService.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.TextToSpeech;

/// <summary>
/// Extended TTS interface that supports reactive streaming synthesis.
/// Enables real-time voice output as LLM generates text.
/// </summary>
public interface IStreamingTtsService : ITextToSpeechService
{
    /// <summary>
    /// Creates a reactive stream that consumes text chunks and produces audio chunks.
    /// Perfect for piping LLM streaming output directly to voice synthesis.
    /// </summary>
    /// <remarks>
    /// This method buffers incoming text into sentence-sized chunks for natural
    /// TTS prosody, then synthesizes each sentence and emits the audio.
    /// </remarks>
    /// <param name="textStream">Observable of text chunks to synthesize.</param>
    /// <param name="options">TTS options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Observable of audio chunks.</returns>
    IObservable<SpeechChunk> StreamSynthesis(
        IObservable<string> textStream,
        TextToSpeechOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Synthesizes text incrementally, emitting audio as each sentence is ready.
    /// Uses sentence-boundary detection for natural pauses between chunks.
    /// </summary>
    /// <param name="text">Complete text to synthesize.</param>
    /// <param name="options">TTS options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Observable of audio chunks with sentence boundaries marked.</returns>
    IObservable<SpeechChunk> StreamSynthesisIncremental(
        string text,
        TextToSpeechOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Synthesizes a single sentence/chunk immediately and returns the audio.
    /// Lower latency than full synthesis, suitable for streaming pipelines.
    /// </summary>
    /// <param name="text">The text chunk to synthesize (should be sentence-sized).</param>
    /// <param name="options">TTS options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The speech chunk result.</returns>
    Task<Result<SpeechChunk, string>> SynthesizeChunkAsync(
        string text,
        TextToSpeechOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Stops any ongoing synthesis immediately (for barge-in support).
    /// </summary>
    void InterruptSynthesis();

    /// <summary>
    /// Gets whether synthesis is currently in progress.
    /// </summary>
    bool IsSynthesizing { get; }

    /// <summary>
    /// Gets whether streaming synthesis is supported by this provider.
    /// </summary>
    bool SupportsStreaming { get; }
}