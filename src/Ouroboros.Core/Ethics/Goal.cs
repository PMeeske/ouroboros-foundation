// <copyright file="EthicsTypes.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Ethics;

/// <summary>
/// Minimal representation of a Goal for ethics evaluation.
/// This is a lightweight version to avoid circular dependencies.
/// </summary>
public sealed record Goal
{
    /// <summary>
    /// Gets the unique identifier for this goal.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the description of what this goal aims to achieve.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the type of goal.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the priority of this goal (0.0 to 1.0).
    /// </summary>
    public required double Priority { get; init; }
}