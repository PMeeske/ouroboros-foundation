// <copyright file="SpeechStateChangedEventArgs.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Event arguments raised when the speech-to-chat bridge transitions between states.
/// </summary>
public sealed class SpeechStateChangedEventArgs : EventArgs
{
    /// <summary>Gets the state the bridge was in before the transition.</summary>
    public required SpeechState PreviousState { get; init; }

    /// <summary>Gets the state the bridge transitioned to.</summary>
    public required SpeechState CurrentState { get; init; }
}
