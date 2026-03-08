// <copyright file="SelfImprovementModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a single step in a plan.
/// </summary>
public sealed record PlanStep(
    string Action,
    Dictionary<string, object> Parameters,
    string ExpectedOutcome,
    double ConfidenceScore);
