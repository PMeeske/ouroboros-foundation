// <copyright file="AuthorizationProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Security.Authorization;

/// <summary>
/// Result of an authorization check.
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Gets a value indicating whether whether the action is authorized.
    /// </summary>
    public bool IsAuthorized { get; init; }

    /// <summary>
    /// Gets reason for denial (if not authorized).
    /// </summary>
    public string? DenialReason { get; init; }

    /// <summary>
    /// Creates an authorized result.
    /// </summary>
    /// <returns></returns>
    public static AuthorizationResult Allow() =>
        new() { IsAuthorized = true };

    /// <summary>
    /// Creates a denied result with a reason.
    /// </summary>
    /// <returns></returns>
    public static AuthorizationResult Deny(string reason) =>
        new() { IsAuthorized = false, DenialReason = reason };
}