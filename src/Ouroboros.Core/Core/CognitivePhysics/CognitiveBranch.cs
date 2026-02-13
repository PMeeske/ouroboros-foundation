// <copyright file="CognitiveBranch.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Represents a weighted branch of cognition during superposition reasoning.
/// Each branch competes via coherence and ethical alignment scoring.
/// </summary>
/// <param name="State">The cognitive state for this branch.</param>
/// <param name="Weight">The coherence weight of this branch (0.0 to 1.0).</param>
public sealed record CognitiveBranch(
    CognitiveState State,
    double Weight);
