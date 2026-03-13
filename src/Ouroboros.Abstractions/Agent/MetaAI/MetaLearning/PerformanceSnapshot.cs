// <copyright file="MetaLearningModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a snapshot of performance at a specific iteration.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record PerformanceSnapshot(
    int Iteration,
    double Performance,
    double Loss,
    DateTime Timestamp);
