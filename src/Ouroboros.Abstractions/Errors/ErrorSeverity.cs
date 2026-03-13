// <copyright file="ErrorSeverity.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Errors;

/// <summary>
/// Indicates the severity level of an <see cref="OuroborosError"/>.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>A critical error that requires immediate attention and likely halts execution.</summary>
    Critical,

    /// <summary>A standard error that prevents the current operation from completing.</summary>
    Error,

    /// <summary>A warning that does not prevent completion but indicates a potential issue.</summary>
    Warning,

    /// <summary>An informational message that does not indicate a problem.</summary>
    Info,
}
