// <copyright file="SpeechInputMode.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Defines how the speech-to-chat bridge listens for speech input.
/// </summary>
public enum SpeechInputMode
{
    /// <summary>Bridge is disabled; no audio is captured or processed.</summary>
    Off,

    /// <summary>Audio is captured only while a push-to-talk key or flag is held.</summary>
    PushToTalk,

    /// <summary>Audio capture activates only after a wake-word is detected.</summary>
    WakeWord,

    /// <summary>Audio is captured and transcribed continuously.</summary>
    AlwaysOn
}
