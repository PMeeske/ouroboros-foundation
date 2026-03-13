using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.DistinctionLearning;

/// <summary>
/// Represents an active distinction being tracked.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ActiveDistinction(
    string Id,
    string Content,
    double Fitness,
    DateTime LearnedAt,
    string LearnedAtStage);
