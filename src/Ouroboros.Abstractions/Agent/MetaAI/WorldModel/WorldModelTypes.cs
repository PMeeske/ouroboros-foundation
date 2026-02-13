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

/// <summary>
/// Represents a state in the world model.
/// </summary>
public sealed record WorldState(
    Guid Id,
    Dictionary<string, object> Features,
    DateTime Timestamp);

/// <summary>
/// Represents a state transition observed in the environment.
/// </summary>
public sealed record WorldTransition(
    WorldState FromState,
    AgentAction Action,
    WorldState ToState,
    double Reward);

/// <summary>
/// Represents an action that can be taken in the world model.
/// </summary>
public sealed record AgentAction(
    string Name,
    Dictionary<string, object>? Parameters = null);

/// <summary>
/// Architecture specification for a world model.
/// </summary>
public sealed record ModelArchitecture(
    string Name,
    Dictionary<string, object>? Hyperparameters = null);

/// <summary>
/// Quality metrics for a world model evaluation.
/// </summary>
public sealed record ModelQuality(
    double PredictionAccuracy,
    double MeanSquaredError,
    int TestSamples,
    DateTime EvaluatedAt,
    double? RewardCorrelation = null,
    double? TerminalAccuracy = null,
    double? CalibrationError = null);

/// <summary>
/// Represents a planned sequence of actions from model-based planning.
/// </summary>
public sealed record ActionPlan(
    List<AgentAction> Actions,
    double ExpectedReward,
    double Confidence,
    int LookaheadDepth);
