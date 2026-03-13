// <copyright file="Draft.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.States;

/// <summary>
/// Represents an initial draft response in the reasoning pipeline.
/// The first phase of iterative refinement.
/// </summary>
/// <param name="DraftText">The initial draft text content</param>
public sealed record Draft(string DraftText) : ReasoningState("Draft", DraftText);
