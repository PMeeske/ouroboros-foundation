namespace Ouroboros.Domain.Voice;

/// <summary>
/// Raw audio chunk for streaming speech-to-text.
/// Published as microphone captures audio in real-time.
/// </summary>
public sealed record AudioChunkEvent : InteractionEvent
{
    /// <summary>Gets the raw audio data.</summary>
    public required byte[] AudioData { get; init; }

    /// <summary>Gets the audio format (e.g., "pcm16", "wav").</summary>
    public required string Format { get; init; }

    /// <summary>Gets the sample rate in Hz.</summary>
    public int SampleRate { get; init; } = 16000;

    /// <summary>Gets the number of audio channels.</summary>
    public int Channels { get; init; } = 1;

    /// <summary>Gets whether this is the final chunk in a recording.</summary>
    public bool IsFinal { get; init; }
}