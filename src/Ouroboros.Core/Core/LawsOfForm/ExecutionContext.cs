// <copyright file="ExecutionContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents the execution context for tool invocation.
/// Contains authorization, rate limiting, and other contextual information.
/// </summary>
public sealed record ExecutionContext
{
    /// <summary>
    /// Gets the user information for authorization checks.
    /// </summary>
    public UserInfo User { get; init; }

    /// <summary>
    /// Gets the rate limiter for controlling execution frequency.
    /// </summary>
    public IRateLimiter RateLimiter { get; init; }

    /// <summary>
    /// Gets the content filter for safety checks.
    /// </summary>
    public IContentFilter ContentFilter { get; init; }

    /// <summary>
    /// Gets optional session information.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets additional context metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
    /// </summary>
    /// <param name="user">The user information.</param>
    /// <param name="rateLimiter">The rate limiter.</param>
    /// <param name="contentFilter">The content filter.</param>
    /// <param name="sessionId">Optional session ID.</param>
    /// <param name="metadata">Optional metadata.</param>
    public ExecutionContext(
        UserInfo user,
        IRateLimiter rateLimiter,
        IContentFilter contentFilter,
        string? sessionId = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        this.User = user;
        this.RateLimiter = rateLimiter;
        this.ContentFilter = contentFilter;
        this.SessionId = sessionId;
        this.Metadata = metadata ?? new Dictionary<string, object>();
    }
}