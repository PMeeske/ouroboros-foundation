// <copyright file="FinalSpec.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.States;

/// <summary>
/// Represents the final, refined specification after iterative improvements.
/// The culmination of the draft-critique-improve cycle.
/// </summary>
/// <param name="FinalText">The final refined text content</param>
[ExcludeFromCodeCoverage]
public sealed record FinalSpec(string FinalText) : ReasoningState("Final", FinalText);
