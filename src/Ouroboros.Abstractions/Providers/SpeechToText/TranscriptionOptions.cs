namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Configuration options for speech-to-text transcription.
/// </summary>
/// <param name="Language">Optional language hint (ISO 639-1 code, e.g., "en", "de", "fr").</param>
/// <param name="ResponseFormat">Response format: "text", "json", "verbose_json", "srt", "vtt".</param>
/// <param name="Temperature">Sampling temperature (0-1). Lower = more deterministic.</param>
/// <param name="TimestampGranularity">Granularity for timestamps: "word", "segment", or null.</param>
/// <param name="Prompt">Optional prompt to guide the transcription style.</param>
public sealed record TranscriptionOptions(
    string? Language = null,
    string ResponseFormat = "json",
    double? Temperature = null,
    string? TimestampGranularity = null,
    string? Prompt = null);