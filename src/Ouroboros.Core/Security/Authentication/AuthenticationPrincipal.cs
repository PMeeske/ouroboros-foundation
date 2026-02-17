// <copyright file="AuthenticationProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Security.Authentication;

/// <summary>
/// Represents an authenticated principal (user/service).
/// </summary>
public class AuthenticationPrincipal
{
    /// <summary>
    /// Gets unique identifier for the principal.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets username or service name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets email address (for users).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets roles assigned to the principal.
    /// </summary>
    public List<string> Roles { get; init; } = new();

    /// <summary>
    /// Gets claims/attributes of the principal.
    /// </summary>
    public Dictionary<string, string> Claims { get; init; } = new();

    /// <summary>
    /// Gets when the authentication expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets a value indicating whether checks if the principal is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > this.ExpiresAt;

    /// <summary>
    /// Checks if the principal has a specific role.
    /// </summary>
    /// <returns></returns>
    public bool HasRole(string role) => this.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if the principal has any of the specified roles.
    /// </summary>
    /// <returns></returns>
    public bool HasAnyRole(params string[] roles) =>
        roles.Any(r => this.HasRole(r));

    /// <summary>
    /// Checks if the principal has all of the specified roles.
    /// </summary>
    /// <returns></returns>
    public bool HasAllRoles(params string[] roles) =>
        roles.All(r => this.HasRole(r));

    /// <summary>
    /// Gets a claim value.
    /// </summary>
    /// <returns></returns>
    public string? GetClaim(string key) =>
        this.Claims.TryGetValue(key, out string? value) ? value : null;
}