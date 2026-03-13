// <copyright file="IMicrophoneManager.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Threading.Channels;

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Manages microphone device enumeration, selection, and audio capture.
/// Implementations wrap platform-specific backends (NAudio, FFmpeg, ALSA, etc.)
/// behind a unified interface so that higher-level audio pipelines remain
/// platform-agnostic.
/// </summary>
public interface IMicrophoneManager : IDisposable
{
    /// <summary>Available microphone devices discovered on this system.</summary>
    IReadOnlyList<MicrophoneDevice> AvailableDevices { get; }

    /// <summary>Currently selected device, or <c>null</c> if none has been selected.</summary>
    MicrophoneDevice? ActiveDevice { get; }

    /// <summary>Whether audio capture is currently running.</summary>
    bool IsCapturing { get; }

    /// <summary>
    /// Re-enumerates hardware devices (hot-plug detection).
    /// Raises <see cref="DevicesChanged"/> when the device list differs from
    /// the previous snapshot.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RefreshDevicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Selects a specific device by its <see cref="MicrophoneDevice.Id"/>.
    /// </summary>
    /// <param name="deviceId">The platform-specific device identifier.</param>
    /// <returns>The selected device on success, or an error string on failure.</returns>
    Result<MicrophoneDevice, string> SelectDevice(string deviceId);

    /// <summary>
    /// Selects the system default recording device.
    /// </summary>
    /// <returns>The selected device on success, or an error string on failure.</returns>
    Result<MicrophoneDevice, string> SelectDefaultDevice();

    /// <summary>
    /// Starts capturing audio frames from the <see cref="ActiveDevice"/>.
    /// Each element written to the returned <see cref="ChannelReader{T}"/> is a
    /// mono float-32 PCM frame normalised to [-1.0, 1.0].
    /// </summary>
    /// <param name="options">Capture parameters (sample rate, frame size, channel capacity).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="ChannelReader{T}"/> of float arrays on success, or an error string.
    /// </returns>
    Task<Result<ChannelReader<float[]>, string>> StartCaptureAsync(
        AudioCaptureOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Stops any active capture and completes the channel.
    /// </summary>
    Task StopCaptureAsync();

    /// <summary>
    /// Fired when the device list changes (hot-plug / unplug).
    /// </summary>
    event EventHandler<DeviceChangeEventArgs>? DevicesChanged;
}
