namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Audio chunk for streaming speech-to-text.
/// </summary>
/// <param name="Data">Raw audio data.</param>
/// <param name="Format">Audio format (e.g., "pcm16", "wav").</param>
/// <param name="SampleRate">Sample rate in Hz.</param>
/// <param name="Channels">Number of audio channels (1 for mono, 2 for stereo).</param>
/// <param name="IsFinal">Whether this is the final chunk in a recording session.</param>
public sealed record AudioChunk(
    byte[] Data,
    string Format,
    int SampleRate,
    int Channels = 1,
    bool IsFinal = false);