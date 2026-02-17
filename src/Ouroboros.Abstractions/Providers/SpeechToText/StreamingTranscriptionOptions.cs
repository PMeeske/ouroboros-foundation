namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Configuration for streaming transcription.
/// </summary>
/// <param name="Language">Language hint (ISO 639-1 code).</param>
/// <param name="EnableInterimResults">Whether to emit partial/interim results.</param>
/// <param name="PunctuationMode">Punctuation mode: "auto", "none", "explicit".</param>
/// <param name="ProfanityFilter">Whether to filter profanity.</param>
/// <param name="SpeakerDiarization">Whether to identify different speakers.</param>
/// <param name="MaxSpeakers">Maximum number of speakers to identify.</param>
/// <param name="ModelSize">Model size hint: "tiny", "base", "small", "medium", "large".</param>
public sealed record StreamingTranscriptionOptions(
    string? Language = null,
    bool EnableInterimResults = true,
    string PunctuationMode = "auto",
    bool ProfanityFilter = false,
    bool SpeakerDiarization = false,
    int MaxSpeakers = 2,
    string ModelSize = "base");