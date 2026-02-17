namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Represents a symbolic relation between thoughts in the neuro-symbolic map.
/// </summary>
public sealed record ThoughtRelation(
    Guid Id,
    Guid SourceThoughtId,
    Guid TargetThoughtId,
    string RelationType,
    double Strength,
    DateTime CreatedAt,
    Dictionary<string, object>? Metadata = null)
{
    /// <summary>Common relation types for the symbolic layer.</summary>
    public static class Types
    {
        public const string CausedBy = "caused_by";
        public const string LeadsTo = "leads_to";
        public const string Contradicts = "contradicts";
        public const string Supports = "supports";
        public const string Refines = "refines";
        public const string Abstracts = "abstracts";
        public const string Elaborates = "elaborates";
        public const string SimilarTo = "similar_to";
        public const string InstanceOf = "instance_of";
        public const string PartOf = "part_of";
        public const string Triggers = "triggers";
        public const string Resolves = "resolves";
    }
}