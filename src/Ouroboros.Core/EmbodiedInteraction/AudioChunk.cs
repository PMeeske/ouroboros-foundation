namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Audio chunk from microphone.
/// </summary>
/// <param name="Data">Raw audio data.</param>
/// <param name="SampleRate">Sample rate.</param>
/// <param name="Channels">Number of channels.</param>
/// <param name="Timestamp">Capture timestamp.</param>
/// <param name="IsFinal">Is this the final chunk.</param>
public sealed record AudioChunk(
    byte[] Data,
    int SampleRate,
    int Channels,
    DateTime Timestamp,
    bool IsFinal);