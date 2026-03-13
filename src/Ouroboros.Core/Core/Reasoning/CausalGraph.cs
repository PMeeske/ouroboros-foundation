// <copyright file="CausalModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a causal graph with variables, edges, and structural equations.
/// Immutable record type for functional programming style.
/// </summary>
/// <param name="Variables">The variables in the causal graph.</param>
/// <param name="Edges">The causal edges between variables.</param>
/// <param name="Equations">Structural equations defining causal relationships.</param>
[ExcludeFromCodeCoverage]
public sealed record CausalGraph(
    List<Variable> Variables,
    List<CausalEdge> Edges,
    Dictionary<string, StructuralEquation> Equations);
