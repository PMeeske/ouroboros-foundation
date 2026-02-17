namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Synthesized speech output.
/// </summary>
/// <param name="Text">The text that was synthesized.</param>
/// <param name="AudioData">The audio data.</param>
/// <param name="Format">Audio format (wav, mp3, opus).</param>
/// <param name="SampleRate">Sample rate.</param>
/// <param name="Duration">Speech duration.</param>
/// <param name="Timestamp">When synthesized.</param>
public sealed record SynthesizedSpeech(
    string Text,
    byte[] AudioData,
    string Format,
    int SampleRate,
    TimeSpan Duration,
    DateTime Timestamp);