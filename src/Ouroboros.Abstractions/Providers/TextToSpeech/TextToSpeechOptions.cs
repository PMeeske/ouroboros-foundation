namespace Ouroboros.Providers.TextToSpeech;

/// <summary>
/// Configuration options for text-to-speech synthesis.
/// </summary>
/// <param name="Voice">The voice to use for synthesis.</param>
/// <param name="Speed">Speech speed (0.25 to 4.0, default 1.0).</param>
/// <param name="Format">Output format: "mp3", "opus", "aac", "flac", "wav", "pcm".</param>
/// <param name="Model">TTS model to use (e.g., "tts-1", "tts-1-hd").</param>
/// <param name="IsWhisper">If true, uses a soft whispering style for inner thoughts.</param>
public sealed record TextToSpeechOptions(
    TtsVoice Voice = TtsVoice.Alloy,
    double Speed = 1.0,
    string Format = "mp3",
    string? Model = null,
    bool IsWhisper = false);