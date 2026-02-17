namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Fused multimodal perception combining multiple sensor inputs.
/// </summary>
/// <param name="Id">Unique ID.</param>
/// <param name="Timestamp">When fusion occurred.</param>
/// <param name="AudioPerceptions">Audio inputs in this window.</param>
/// <param name="VisualPerceptions">Visual inputs in this window.</param>
/// <param name="TextPerceptions">Text inputs in this window.</param>
/// <param name="IntegratedUnderstanding">Combined understanding from all modalities.</param>
/// <param name="Confidence">Overall confidence.</param>
public sealed record FusedPerception(
    Guid Id,
    DateTime Timestamp,
    IReadOnlyList<AudioPerception> AudioPerceptions,
    IReadOnlyList<VisualPerception> VisualPerceptions,
    IReadOnlyList<TextPerception> TextPerceptions,
    string IntegratedUnderstanding,
    double Confidence)
{
    /// <summary>
    /// Gets whether this perception includes audio.
    /// </summary>
    public bool HasAudio => AudioPerceptions.Count > 0;

    /// <summary>
    /// Gets whether this perception includes visual.
    /// </summary>
    public bool HasVisual => VisualPerceptions.Count > 0;

    /// <summary>
    /// Gets the combined transcript from audio.
    /// </summary>
    public string CombinedTranscript =>
        string.Join(" ", AudioPerceptions.Where(a => a.IsFinal).Select(a => a.TranscribedText));

    /// <summary>
    /// Gets dominant modality by count.
    /// </summary>
    public SensorModality DominantModality
    {
        get
        {
            var counts = new[]
            {
                (SensorModality.Audio, AudioPerceptions.Count),
                (SensorModality.Visual, VisualPerceptions.Count),
                (SensorModality.Text, TextPerceptions.Count),
            };
            return counts.OrderByDescending(c => c.Item2).First().Item1;
        }
    }
}