namespace Ouroboros.Tools;

/// <summary>
/// Represents a DSL suggestion.
/// </summary>
public class DslSuggestion
{
    public string Step { get; }
    public string Explanation { get; }
    public double Confidence { get; }

    public DslSuggestion(string step, string explanation, double confidence)
    {
        Step = step;
        Explanation = explanation;
        Confidence = confidence;
    }
}