// <copyright file="EnvironmentAction.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Environment;

/// <summary>
/// Represents an action taken in an environment.
/// Immutable record capturing the action type and parameters.
/// </summary>
/// <param name="ActionType">The type/name of the action</param>
/// <param name="Parameters">Optional parameters for the action</param>
[ExcludeFromCodeCoverage]
public sealed record EnvironmentAction(
    string ActionType,
    IReadOnlyDictionary<string, object>? Parameters = null);
