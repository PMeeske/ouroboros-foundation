namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Represents the current focus of attention.
/// </summary>
/// <param name="Modality">Primary modality of attention.</param>
/// <param name="Target">What the attention is focused on.</param>
/// <param name="Intensity">How intensely focused (0-1).</param>
/// <param name="StartedAt">When attention was directed here.</param>
public sealed record AttentionFocus(
    SensorModality Modality,
    string Target,
    double Intensity,
    DateTime StartedAt)
{
    /// <summary>
    /// Duration of current attention focus.
    /// </summary>
    public TimeSpan Duration => DateTime.UtcNow - StartedAt;
}