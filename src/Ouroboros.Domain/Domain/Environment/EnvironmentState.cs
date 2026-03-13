// <copyright file="EnvironmentState.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Environment;

/// <summary>
/// Represents the state of an environment at a specific point in time.
/// Immutable record that can be serialized for replay.
/// </summary>
/// <param name="StateData">Dictionary containing the state data</param>
/// <param name="IsTerminal">Whether this state is terminal (episode ended)</param>
[ExcludeFromCodeCoverage]
public sealed record EnvironmentState(
    IReadOnlyDictionary<string, object> StateData,
    bool IsTerminal = false);
