// <copyright file="Draft.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.States;

/// <summary>
/// Represents an initial draft response in the reasoning pipeline.
/// The first phase of iterative refinement.
/// </summary>
/// <param name="DraftText">The initial draft text content</param>
[ExcludeFromCodeCoverage]
public sealed record Draft(string DraftText) : ReasoningState("Draft", DraftText);
