using System.Reactive.Linq;

namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Extension methods for streaming STT services.
/// </summary>
public static class StreamingSttExtensions
{
    /// <summary>
    /// Filters a transcription stream to only final results.
    /// </summary>
    /// <param name="stream">The transcription event stream.</param>
    /// <returns>Observable of only final transcription events.</returns>
    public static IObservable<TranscriptionEvent> FinalResultsOnly(
        this IObservable<TranscriptionEvent> stream)
    {
        return System.Reactive.Linq.Observable
            .Where(stream, e => e.IsFinal);
    }

    /// <summary>
    /// Extracts just the text from transcription events.
    /// </summary>
    /// <param name="stream">The transcription event stream.</param>
    /// <returns>Observable of transcribed text strings.</returns>
    public static IObservable<string> TextOnly(
        this IObservable<TranscriptionEvent> stream)
    {
        return System.Reactive.Linq.Observable.Select(stream, e => e.Text)
            .Where(static text => !string.IsNullOrWhiteSpace(text));
    }

    /// <summary>
    /// Filters voice activity to speech boundaries (start and end only).
    /// </summary>
    /// <param name="stream">The voice activity stream.</param>
    /// <returns>Observable of speech start/end events only.</returns>
    public static IObservable<VoiceActivityEvent> SpeechBoundariesOnly(
        this IObservable<VoiceActivityEvent> stream)
    {
        return System.Reactive.Linq.Observable.Where(
            stream,
            static e => e.Activity is VoiceActivity.SpeechStart or VoiceActivity.SpeechEnd);
    }

    /// <summary>
    /// Detects speech segments between start and end events.
    /// </summary>
    /// <param name="stream">The voice activity stream.</param>
    /// <returns>Observable of speech segment durations.</returns>
    public static IObservable<TimeSpan> SpeechSegmentDurations(
        this IObservable<VoiceActivityEvent> stream)
    {
        return System.Reactive.Linq.Observable.Buffer(stream.SpeechBoundariesOnly(), 2)
            .Where(static buffer => buffer.Count == 2 &&
                                    buffer[0].Activity == VoiceActivity.SpeechStart &&
                                    buffer[1].Activity == VoiceActivity.SpeechEnd)
            .Select(static buffer => buffer[1].Timestamp - buffer[0].Timestamp);
    }
}