// <copyright file="Dependency.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.MultiAgent;

/// <summary>
/// Represents a dependency relationship between two tasks.
/// </summary>
/// <param name="TaskA">The first task identifier.</param>
/// <param name="TaskB">The second task identifier.</param>
/// <param name="Type">The type of dependency relationship.</param>
[ExcludeFromCodeCoverage]
public sealed record Dependency(
    string TaskA,
    string TaskB,
    DependencyType Type);
