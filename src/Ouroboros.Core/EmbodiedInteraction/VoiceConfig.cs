namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Voice/TTS configuration.
/// </summary>
/// <param name="Voice">Voice name/ID.</param>
/// <param name="Speed">Speech speed (0.5-2.0, 1.0 = normal).</param>
/// <param name="Pitch">Voice pitch adjustment.</param>
/// <param name="Volume">Output volume (0.0-1.0).</param>
/// <param name="Language">Language code.</param>
/// <param name="Style">Speech style (neutral, cheerful, sad, etc.).</param>
/// <param name="EnableSSML">Enable SSML markup support.</param>
public sealed record VoiceConfig(
    string Voice = "default",
    double Speed = 1.0,
    double Pitch = 1.0,
    double Volume = 1.0,
    string Language = "en-US",
    string Style = "neutral",
    bool EnableSSML = false);