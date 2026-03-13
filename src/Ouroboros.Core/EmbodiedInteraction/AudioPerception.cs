using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Audio perception from microphone.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record AudioPerception(
    Guid Id,
    DateTime Timestamp,
    double Confidence,
    string TranscribedText,
    string? DetectedLanguage,
    double? SpeakerEmbedding,
    TimeSpan Duration,
    bool IsFinal) : PerceptionEvent(Id, SensorModality.Audio, Timestamp, Confidence);
