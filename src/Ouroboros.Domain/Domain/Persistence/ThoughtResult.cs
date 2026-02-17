namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Represents the outcome/result of a thought or thought chain.
/// </summary>
public sealed record ThoughtResult(
    Guid Id,
    Guid ThoughtId,
    string ResultType,
    string Content,
    bool Success,
    double Confidence,
    DateTime CreatedAt,
    TimeSpan? ExecutionTime = null,
    Dictionary<string, object>? Metadata = null)
{
    /// <summary>Common result types.</summary>
    public static class Types
    {
        public const string Action = "action";
        public const string Response = "response";
        public const string Insight = "insight";
        public const string Decision = "decision";
        public const string SkillLearned = "skill_learned";
        public const string FactDiscovered = "fact_discovered";
        public const string Error = "error";
        public const string Deferred = "deferred";
    }
}