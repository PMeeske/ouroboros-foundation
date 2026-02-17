namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Interface for content filtering and safety checks.
/// </summary>
public interface IContentFilter
{
    /// <summary>
    /// Analyzes content for safety violations.
    /// </summary>
    /// <param name="content">The content to analyze.</param>
    /// <returns>The safety level of the content.</returns>
    SafetyLevel Analyze(string content);
}