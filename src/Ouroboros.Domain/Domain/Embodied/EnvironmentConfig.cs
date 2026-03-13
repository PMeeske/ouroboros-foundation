// <copyright file="EnvironmentConfig.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Embodied;

/// <summary>
/// Configuration for creating an environment instance.
/// </summary>
/// <param name="SceneName">Name of the scene/environment to load</param>
/// <param name="Parameters">Configuration parameters specific to the environment</param>
/// <param name="AvailableActions">List of action names available in this environment</param>
/// <param name="Type">Type of environment</param>
[ExcludeFromCodeCoverage]
public sealed record EnvironmentConfig(
    string SceneName,
    IReadOnlyDictionary<string, object> Parameters,
    IReadOnlyList<string> AvailableActions,
    EnvironmentType Type);
