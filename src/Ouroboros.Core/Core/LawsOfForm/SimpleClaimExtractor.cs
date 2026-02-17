namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Simple claim extractor that splits text into sentences.
/// In production, this would use NLP for proper claim extraction.
/// </summary>
public sealed class SimpleClaimExtractor : IClaimExtractor
{
    /// <inheritdoc/>
    public IReadOnlyList<Claim> ExtractClaims(string text, string source)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<Claim>();
        }

        // Simple sentence splitting (production would use NLP)
        var sentences = text
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 10) // Filter out very short fragments
            .Select(s => new Claim(s, 0.8, source)) // Default confidence
            .ToList();

        return sentences;
    }
}