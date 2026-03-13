// <copyright file="MergeStrategy.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Learning;

/// <summary>
/// Strategies for merging multiple LoRA/PEFT adapters.
/// </summary>
[ExcludeFromCodeCoverage]
public enum MergeStrategy
{
    /// <summary>
    /// Simple averaging of adapter weights.
    /// </summary>
    Average,

    /// <summary>
    /// Weighted averaging based on adapter performance.
    /// </summary>
    Weighted,

    /// <summary>
    /// Task arithmetic: combining adapters using vector arithmetic.
    /// </summary>
    TaskArithmetic,

    /// <summary>
    /// TIES-Merging: Trim, Elect, and Merge strategy for better conflict resolution.
    /// </summary>
    TIES,
}
