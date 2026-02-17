// <copyright file="SelfImprovementModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a single step in a plan.
/// </summary>
public sealed record PlanStep(
    string Action,
    Dictionary<string, object> Parameters,
    string ExpectedOutcome,
    double ConfidenceScore);