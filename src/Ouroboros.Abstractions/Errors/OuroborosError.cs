// <copyright file="OuroborosError.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Errors;

/// <summary>
/// Structured error type for the Ouroboros pipeline system.
/// Replaces string-typed errors with categorized, debuggable error information.
/// Use with <see cref="Monads.Result{TValue, TError}"/> as <c>Result&lt;T, OuroborosError&gt;</c>
/// for rich error handling that preserves codes, details, and exception context.
/// </summary>
public sealed record OuroborosError
{
    /// <summary>
    /// Gets the machine-readable error code (e.g. "LLM_001", "ETHICS_001").
    /// See <see cref="ErrorCodes"/> for well-known codes.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets optional additional detail for debugging or logging.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Gets the original exception, if one caused this error.
    /// Preserves the stack trace that string-typed errors lose.
    /// </summary>
    public Exception? InnerException { get; init; }

    /// <summary>
    /// Gets the severity level of this error.
    /// Defaults to <see cref="ErrorSeverity.Error"/>.
    /// </summary>
    public ErrorSeverity Severity { get; init; } = ErrorSeverity.Error;

    /// <summary>
    /// Gets the timestamp when this error was created.
    /// Defaults to <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an <see cref="OuroborosError"/> from a code and message.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <returns>A new <see cref="OuroborosError"/> instance.</returns>
    public static OuroborosError From(string code, string message) =>
        new() { Code = code, Message = message };

    /// <summary>
    /// Creates an <see cref="OuroborosError"/> from a code and exception.
    /// The exception's message becomes the error message, and the exception
    /// is stored for stack trace preservation.
    /// </summary>
    /// <param name="code">The machine-readable error code.</param>
    /// <param name="ex">The exception that caused the error.</param>
    /// <returns>A new <see cref="OuroborosError"/> instance.</returns>
    public static OuroborosError From(string code, Exception ex) =>
        new() { Code = code, Message = ex.Message, InnerException = ex };

    /// <summary>
    /// Implicit conversion from <see cref="string"/> for backward compatibility
    /// during the incremental migration from <c>Result&lt;T, string&gt;</c>.
    /// The resulting error will have code <c>"UNKNOWN"</c>.
    /// </summary>
    /// <param name="message">The error message string.</param>
    public static implicit operator OuroborosError(string message) =>
        new() { Code = "UNKNOWN", Message = message };

    /// <summary>
    /// Returns a formatted string representation: <c>[CODE] Message</c>.
    /// </summary>
    /// <returns>A string in the format <c>[CODE] Message</c>.</returns>
    public override string ToString() => $"[{Code}] {Message}";
}
