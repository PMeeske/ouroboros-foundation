// <copyright file="MetaLearningModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents a snapshot of performance at a specific iteration.
/// </summary>
public sealed record PerformanceSnapshot(
    int Iteration,
    double Performance,
    double Loss,
    DateTime Timestamp);