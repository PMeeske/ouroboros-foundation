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
/// Supported model architectures for world model learning.
/// </summary>
public enum ModelArchitecture
{
    /// <summary>Multi-layer perceptron (simple feed-forward network).</summary>
    MLP,

    /// <summary>Transformer-based architecture with attention mechanisms.</summary>
    Transformer,

    /// <summary>Graph neural network for structured state spaces.</summary>
    GNN,

    /// <summary>Hybrid architecture combining multiple approaches.</summary>
    Hybrid,
}

/// <summary>
/// Represents quality metrics for a world model.
/// Used to evaluate model accuracy and calibration.
/// </summary>
/// <param name="PredictionAccuracy">Accuracy of state predictions (0-1).</param>
/// <param name="RewardCorrelation">Correlation of predicted vs actual rewards (0-1).</param>
/// <param name="TerminalAccuracy">Accuracy of terminal state predictions (0-1).</param>
/// <param name="CalibrationError">Mean calibration error for uncertainty estimates.</param>
/// <param name="TestSamples">Number of samples used in evaluation.</param>
public sealed record ModelQuality(
    double PredictionAccuracy,
    double RewardCorrelation,
    double TerminalAccuracy,
    double CalibrationError,
    int TestSamples);

/// <summary>
/// Represents a planned sequence of actions from model-based planning.
/// </summary>
public sealed record ActionPlan(
    List<AgentAction> Actions,
    double ExpectedReward,
    double Confidence,
    int LookaheadDepth);
