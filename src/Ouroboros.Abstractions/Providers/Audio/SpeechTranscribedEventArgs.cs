// <copyright file="SpeechTranscribedEventArgs.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Audio;

/// <summary>
/// Event arguments raised when the speech-to-chat bridge has transcribed speech to text.
/// </summary>
public sealed class SpeechTranscribedEventArgs : EventArgs
{
    /// <summary>Gets the transcribed text.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the STT confidence score (0.0 to 1.0).</summary>
    public float Confidence { get; init; }

    /// <summary>Gets the duration of the speech utterance.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets a value indicating whether this is a final (non-partial) transcription.</summary>
    public bool IsFinal { get; init; }
}
