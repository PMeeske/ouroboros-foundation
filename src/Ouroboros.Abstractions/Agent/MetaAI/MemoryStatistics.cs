namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Statistics about the memory store.
/// </summary>
/// <param name="TotalExperiences">Total number of stored experiences.</param>
/// <param name="SuccessfulExperiences">Number of successful experiences.</param>
/// <param name="FailedExperiences">Number of failed experiences.</param>
/// <param name="UniqueContexts">Number of unique contexts.</param>
/// <param name="UniqueTags">Number of unique tags.</param>
/// <param name="OldestExperience">Timestamp of oldest experience.</param>
/// <param name="NewestExperience">Timestamp of newest experience.</param>
/// <param name="AverageQualityScore">Average quality score across all experiences.</param>
public sealed record MemoryStatistics(
    int TotalExperiences,
    int SuccessfulExperiences,
    int FailedExperiences,
    int UniqueContexts,
    int UniqueTags,
    DateTime? OldestExperience = null,
    DateTime? NewestExperience = null,
    double AverageQualityScore = 0.0);