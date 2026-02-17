// <copyright file="AffectModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Represents the current affective state of an agent.
/// </summary>
public sealed record AffectiveState(
    Guid Id,
    double Valence,
    double Stress,
    double Confidence,
    double Curiosity,
    double Arousal,
    DateTime Timestamp,
    Dictionary<string, object> Metadata);