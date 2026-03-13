// <copyright file="IDispatcher.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Agent.Dispatch;

/// <summary>
/// Unified dispatch interface for both commands and queries.
/// Resolves the appropriate handler from DI and invokes it.
/// Host layers (CLI, WebAPI, Android) register an implementation backed
/// by their preferred dispatch mechanism (e.g. MediatR, custom DI lookup).
/// </summary>
[ExcludeFromCodeCoverage]
public interface IDispatcher
{
    /// <summary>
    /// Dispatches a command to its handler.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result from the command handler.</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);

    /// <summary>
    /// Dispatches a query to its handler.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result from the query handler.</returns>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}
