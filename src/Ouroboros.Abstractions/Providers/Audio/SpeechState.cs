// <copyright file="SpeechState.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Represents the current state of the speech-to-chat bridge processing pipeline.
/// </summary>
public enum SpeechState
{
    /// <summary>No active processing; waiting for speech or activation.</summary>
    Idle,

    /// <summary>Microphone is active and monitoring for speech onset.</summary>
    Listening,

    /// <summary>Speech activity detected; accumulating audio frames.</summary>
    Detecting,

    /// <summary>Speech ended; audio buffer is being transcribed via STT.</summary>
    Transcribing,

    /// <summary>Transcription complete; text is ready for the chat subsystem.</summary>
    Ready
}
