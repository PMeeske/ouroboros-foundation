namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Voice activity detection event.
/// </summary>
/// <param name="Activity">The type of voice activity detected.</param>
/// <param name="Timestamp">When the activity was detected.</param>
/// <param name="Confidence">Confidence in the detection (0.0-1.0).</param>
public sealed record VoiceActivityEvent(
    VoiceActivity Activity,
    DateTimeOffset Timestamp,
    double Confidence = 1.0);