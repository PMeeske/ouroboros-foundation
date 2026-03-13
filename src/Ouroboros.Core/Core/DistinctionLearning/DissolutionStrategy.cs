// <copyright file="DissolutionStrategy.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.DistinctionLearning;

/// <summary>
/// Strategy for dissolving low-value distinctions.
/// </summary>
[ExcludeFromCodeCoverage]
public enum DissolutionStrategy
{
    /// <summary>
    /// Dissolve distinctions below a fitness threshold.
    /// </summary>
    FitnessThreshold,

    /// <summary>
    /// Dissolve oldest distinctions first.
    /// </summary>
    OldestFirst,

    /// <summary>
    /// Dissolve least recently used distinctions.
    /// </summary>
    LeastRecentlyUsed,

    /// <summary>
    /// Dissolve all distinctions.
    /// </summary>
    All
}
