// <copyright file="ToolRegistry.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Core;

/// <summary>
/// Represents a tool registry for managing available tools.
/// </summary>
public class ToolRegistry
{
    /// <summary>
    /// Gets the number of registered tools.
    /// </summary>
    public virtual int Count => 0;
}
