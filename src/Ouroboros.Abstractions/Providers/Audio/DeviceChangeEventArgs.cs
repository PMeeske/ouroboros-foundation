// <copyright file="DeviceChangeEventArgs.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Event arguments raised when the set of available microphone devices changes
/// (hot-plug or unplug).
/// </summary>
public sealed class DeviceChangeEventArgs : EventArgs
{
    /// <summary>The complete list of currently available devices.</summary>
    public required IReadOnlyList<MicrophoneDevice> Devices { get; init; }

    /// <summary>The device that was added, or <c>null</c> if a device was removed.</summary>
    public MicrophoneDevice? Added { get; init; }

    /// <summary>The device that was removed, or <c>null</c> if a device was added.</summary>
    public MicrophoneDevice? Removed { get; init; }
}
