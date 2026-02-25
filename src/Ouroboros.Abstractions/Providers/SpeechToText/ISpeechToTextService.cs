// <copyright file="ISpeechToTextService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Defines the contract for speech-to-text transcription services.
/// Supports various audio formats and providers (OpenAI Whisper, Azure, local Whisper, etc.).
/// </summary>
public interface ISpeechToTextService
{
    /// <summary>
    /// Gets the name of the speech-to-text provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the supported audio formats.
    /// </summary>
    IReadOnlyList<string> SupportedFormats { get; }

    /// <summary>
    /// Gets the maximum audio file size in bytes.
    /// </summary>
    long MaxFileSizeBytes { get; }

    /// <summary>
    /// Transcribes audio from a file path.
    /// </summary>
    /// <param name="filePath">Path to the audio file.</param>
    /// <param name="options">Optional transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transcription result.</returns>
    Task<Result<TranscriptionResult, string>> TranscribeFileAsync(
        string filePath,
        TranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Transcribes audio from a stream.
    /// </summary>
    /// <param name="audioStream">The audio stream.</param>
    /// <param name="fileName">The file name (used to determine format).</param>
    /// <param name="options">Optional transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transcription result.</returns>
    Task<Result<TranscriptionResult, string>> TranscribeStreamAsync(
        Stream audioStream,
        string fileName,
        TranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Transcribes audio from a byte array.
    /// </summary>
    /// <param name="audioData">The audio data bytes.</param>
    /// <param name="fileName">The file name (used to determine format).</param>
    /// <param name="options">Optional transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The transcription result.</returns>
    Task<Result<TranscriptionResult, string>> TranscribeBytesAsync(
        byte[] audioData,
        string fileName,
        TranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Translates audio to English (if supported by the provider).
    /// </summary>
    /// <param name="filePath">Path to the audio file.</param>
    /// <param name="options">Optional transcription options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The translation result in English.</returns>
    Task<Result<TranscriptionResult, string>> TranslateToEnglishAsync(
        string filePath,
        TranscriptionOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if the service is available and properly configured.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the service is available.</returns>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
