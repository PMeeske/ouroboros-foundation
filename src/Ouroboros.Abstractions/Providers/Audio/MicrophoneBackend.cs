// <copyright file="MicrophoneBackend.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Identifies the audio capture backend used by a microphone device.
/// </summary>
public enum MicrophoneBackend
{
    /// <summary>NAudio WaveInEvent (Windows).</summary>
    NAudio,

    /// <summary>FFmpeg process-based capture (cross-platform).</summary>
    FFmpeg,

    /// <summary>ALSA (Advanced Linux Sound Architecture).</summary>
    Alsa,

    /// <summary>PulseAudio (Linux).</summary>
    PulseAudio,

    /// <summary>Core Audio (macOS).</summary>
    CoreAudio,
}
