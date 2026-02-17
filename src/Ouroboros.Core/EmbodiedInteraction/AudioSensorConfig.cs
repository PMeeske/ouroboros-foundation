namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Configuration for the audio sensor (microphone).
/// </summary>
/// <param name="SampleRate">Audio sample rate in Hz.</param>
/// <param name="Channels">Number of audio channels.</param>
/// <param name="Language">Language hint for STT.</param>
/// <param name="EnableVAD">Enable voice activity detection.</param>
/// <param name="SilenceThresholdMs">Silence threshold for VAD in ms.</param>
/// <param name="MaxRecordingDurationMs">Maximum recording duration in ms.</param>
/// <param name="EnableInterimResults">Enable streaming interim results.</param>
public sealed record AudioSensorConfig(
    int SampleRate = 16000,
    int Channels = 1,
    string? Language = null,
    bool EnableVAD = true,
    int SilenceThresholdMs = 1500,
    int MaxRecordingDurationMs = 30000,
    bool EnableInterimResults = true);