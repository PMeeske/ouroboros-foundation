// <copyright file="WorldModelTypes.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents a learned world model for model-based reinforcement learning.
/// </summary>
public sealed record LearnedWorldModel(
    Guid Id,
    string Name,
    ModelArchitecture Architecture,
    double Accuracy,
    int TrainingSamples,
    DateTime TrainedAt);