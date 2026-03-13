// <copyright file="AudioCaptureOptions.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Configures audio capture parameters for microphone recording.
/// </summary>
public sealed record AudioCaptureOptions
{
    /// <summary>Audio sample rate in Hz. Default is 16 000 (speech-optimised).</summary>
    public int SampleRate { get; init; } = 16_000;

    /// <summary>Frame size in milliseconds. Default is 32 ms (512 samples at 16 kHz).</summary>
    public int FrameSizeMs { get; init; } = 32;

    /// <summary>
    /// Bounded channel capacity for backpressure between the capture callback
    /// and the consumer. When the channel is full the oldest frame is dropped.
    /// </summary>
    public int ChannelCapacity { get; init; } = 64;
}
