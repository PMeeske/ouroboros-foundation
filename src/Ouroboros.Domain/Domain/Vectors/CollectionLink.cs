namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Represents a link between two collections in Ouroboros's memory system.
/// </summary>
public sealed record CollectionLink(
    string SourceCollection,
    string TargetCollection,
    string RelationType,
    double Strength = 1.0,
    string? Description = null)
{
    /// <summary>Common link types for collection relationships.</summary>
    public static class Types
    {
        public const string DependsOn = "depends_on";
        public const string Indexes = "indexes";
        public const string Extends = "extends";
        public const string Mirrors = "mirrors";
        public const string Aggregates = "aggregates";
        public const string PartOf = "part_of";
        public const string RelatedTo = "related_to";
    }
}