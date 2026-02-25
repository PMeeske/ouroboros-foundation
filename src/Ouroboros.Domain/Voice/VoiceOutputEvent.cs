namespace Ouroboros.Domain.Voice;

/// <summary>
/// Voice synthesis output - streamed audio chunks.
/// </summary>
public sealed record VoiceOutputEvent : InteractionEvent
{
    /// <summary>Gets the synthesized audio chunk.</summary>
    public required byte[] AudioChunk { get; init; }

    /// <summary>Gets the audio format (e.g., "pcm16", "mp3", "wav").</summary>
    public required string Format { get; init; }

    /// <summary>Gets the sample rate in Hz.</summary>
    public int SampleRate { get; init; } = 24000;

    /// <summary>Gets the duration of this audio chunk in seconds.</summary>
    public double DurationSeconds { get; init; }

    /// <summary>Gets whether this is the final audio chunk.</summary>
    public bool IsComplete { get; init; }

    /// <summary>Gets the emotion/style applied (for expressive TTS).</summary>
    public string? Emotion { get; init; }

    /// <summary>Gets the text this audio represents (for debugging/display).</summary>
    public string? Text { get; init; }
}