namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents a factual claim extracted from text.
/// </summary>
public sealed record Claim
{
    /// <summary>
    /// Gets the statement text.
    /// </summary>
    public string Statement { get; init; }

    /// <summary>
    /// Gets the confidence in this claim (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets the source of this claim (e.g., model name, document reference).
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Claim"/> class.
    /// </summary>
    /// <param name="statement">The claim statement.</param>
    /// <param name="confidence">The confidence score.</param>
    /// <param name="source">The source of the claim.</param>
    public Claim(string statement, double confidence, string source)
    {
        this.Statement = statement;
        this.Confidence = Math.Clamp(confidence, 0.0, 1.0);
        this.Source = source;
    }
}