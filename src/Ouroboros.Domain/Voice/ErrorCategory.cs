namespace Ouroboros.Domain.Voice;

/// <summary>
/// Categories of errors in the voice stream.
/// </summary>
public enum ErrorCategory
{
    /// <summary>Unknown error.</summary>
    Unknown,

    /// <summary>Speech recognition error.</summary>
    SpeechRecognition,

    /// <summary>Speech synthesis error.</summary>
    SpeechSynthesis,

    /// <summary>LLM/generation error.</summary>
    Generation,

    /// <summary>Audio hardware error.</summary>
    AudioHardware,

    /// <summary>Network/connectivity error.</summary>
    Network,
}