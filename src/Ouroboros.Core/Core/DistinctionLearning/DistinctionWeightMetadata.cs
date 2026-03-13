// <copyright file="DistinctionWeightMetadata.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.DistinctionLearning;

/// <summary>
/// Metadata for stored distinction weights.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DistinctionWeightMetadata(
    string Id,
    string Path,
    double Fitness,
    string LearnedAtStage,
    DateTime CreatedAt,
    bool IsDissolved,
    long SizeBytes);
