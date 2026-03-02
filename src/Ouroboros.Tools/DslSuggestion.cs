namespace Ouroboros.Tools;

/// <summary>
/// Represents a DSL suggestion.
/// </summary>
public sealed class DslSuggestion
{
    /// <summary>
    /// Gets the DSL token or step being suggested (e.g., "UseDraft", "UseCritique").
    /// </summary>
    public string Step { get; }

    /// <summary>
    /// Gets a human-readable explanation of why this step is suggested.
    /// </summary>
    public string Explanation { get; }

    /// <summary>
    /// Gets the confidence score for this suggestion, in the range [0.0, 1.0].
    /// </summary>
    public double Confidence { get; }

    public DslSuggestion(string step, string explanation, double confidence)
    {
        Step = step;
        Explanation = explanation;
        Confidence = confidence;
    }
}