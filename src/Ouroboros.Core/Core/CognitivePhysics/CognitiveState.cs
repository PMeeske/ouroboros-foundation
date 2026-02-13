// <copyright file="CognitiveState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Represents the state of a cognitive reasoning system modeled as a trajectory
/// in a metric conceptual space under resource and ethical constraints.
/// </summary>
/// <param name="Focus">Current active conceptual domain.</param>
/// <param name="Resources">Cognitive energy budget remaining.</param>
/// <param name="Compression">Efficiency coefficient (0.1 to 1.0). Lower is more efficient.</param>
/// <param name="History">Context trajectory recording visited conceptual domains.</param>
/// <param name="Cooldown">Shift stabilization delay. Must reach zero before next shift.</param>
/// <param name="Entanglement">Active superposition contexts for multi-context reasoning.</param>
public sealed record CognitiveState(
    string Focus,
    double Resources,
    double Compression,
    ImmutableList<string> History,
    double Cooldown,
    ImmutableHashSet<string> Entanglement)
{
    /// <summary>
    /// Creates a new cognitive state with default initial values.
    /// </summary>
    /// <param name="initialFocus">The starting conceptual domain.</param>
    /// <param name="resources">Initial resource budget.</param>
    /// <returns>A fresh cognitive state.</returns>
    public static CognitiveState Create(string initialFocus, double resources = 100.0) =>
        new(
            Focus: initialFocus,
            Resources: resources,
            Compression: 1.0,
            History: ImmutableList.Create(initialFocus),
            Cooldown: 0.0,
            Entanglement: ImmutableHashSet<string>.Empty);

    /// <summary>
    /// Returns a new state with cooldown decremented by the specified amount, floored at zero.
    /// </summary>
    public CognitiveState Tick(double elapsed = 1.0) =>
        this with { Cooldown = Math.Max(0.0, Cooldown - elapsed) };
}
