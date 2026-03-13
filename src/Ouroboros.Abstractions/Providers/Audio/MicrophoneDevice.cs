// <copyright file="MicrophoneDevice.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Describes a single microphone input device.
/// </summary>
/// <param name="Id">Platform-specific unique device identifier.</param>
/// <param name="Name">Human-readable device name.</param>
/// <param name="Channels">Number of supported input channels.</param>
/// <param name="IsDefault">Whether this device is the system default.</param>
/// <param name="Backend">The audio capture backend that owns this device.</param>
public sealed record MicrophoneDevice(
    string Id,
    string Name,
    int Channels,
    bool IsDefault,
    MicrophoneBackend Backend);
