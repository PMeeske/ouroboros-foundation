namespace Ouroboros.Providers.TextToSpeech;

/// <summary>
/// Extension methods for streaming TTS services.
/// </summary>
public static class StreamingTtsExtensions
{
    /// <summary>
    /// Sentence boundary detection pattern for natural TTS chunking.
    /// </summary>
    private static readonly char[] SentenceEnders = ['.', '!', '?', '\n'];
    private const int MinChunkSize = 15;
    private const int MaxChunkSize = 200;

    /// <summary>
    /// Buffers a token stream into sentence-sized chunks for natural TTS.
    /// </summary>
    /// <param name="tokens">The incoming token stream.</param>
    /// <returns>Observable of sentence-sized text chunks.</returns>
    public static IObservable<string> BufferIntoSentences(this IObservable<string> tokens)
    {
        return System.Reactive.Linq.Observable.Create<string>(observer =>
        {
            var buffer = new System.Text.StringBuilder();

            return tokens.Subscribe(
                onNext: token =>
                {
                    buffer.Append(token);

                    var text = buffer.ToString();
                    var lastEnder = text.LastIndexOfAny(SentenceEnders);

                    // Emit if we have a sentence boundary and enough content
                    if (lastEnder >= MinChunkSize)
                    {
                        var sentence = text[..(lastEnder + 1)].Trim();
                        if (!string.IsNullOrWhiteSpace(sentence))
                        {
                            observer.OnNext(sentence);
                        }

                        // Keep remainder in buffer
                        buffer.Clear();
                        if (lastEnder + 1 < text.Length)
                        {
                            buffer.Append(text[(lastEnder + 1)..]);
                        }
                    }
                    else if (buffer.Length > MaxChunkSize)
                    {
                        // Force emit if buffer is too large (no sentence boundary found)
                        var chunk = text.Trim();
                        if (!string.IsNullOrWhiteSpace(chunk))
                        {
                            observer.OnNext(chunk);
                        }

                        buffer.Clear();
                    }
                },
                onError: observer.OnError,
                onCompleted: () =>
                {
                    // Flush remaining buffer
                    var remaining = buffer.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(remaining))
                    {
                        observer.OnNext(remaining);
                    }

                    observer.OnCompleted();
                });
        });
    }

    /// <summary>
    /// Splits text into sentences for incremental synthesis.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <returns>Enumerable of sentences.</returns>
    public static IEnumerable<string> SplitIntoSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var pattern = new System.Text.RegularExpressions.Regex(
            @"(?<=[.!?])\s+|(?<=\n)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        var sentences = pattern.Split(text);

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                yield return trimmed;
            }
        }
    }
}