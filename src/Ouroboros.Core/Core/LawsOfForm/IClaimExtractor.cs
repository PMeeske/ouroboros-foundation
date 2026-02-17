namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Interface for extracting claims from text.
/// Abstracts the claim extraction logic for testability and flexibility.
/// </summary>
public interface IClaimExtractor
{
    /// <summary>
    /// Extracts verifiable claims from text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="source">The source of the text (e.g., model name).</param>
    /// <returns>A list of extracted claims.</returns>
    IReadOnlyList<Claim> ExtractClaims(string text, string source);
}