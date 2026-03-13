// <copyright file="IQueryHandler.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Ouroboros.Abstractions.Agent.Dispatch;

/// <summary>
/// Handles a single <see cref="IQuery{TResult}"/> and produces a result.
/// </summary>
/// <typeparam name="TQuery">The query type to handle.</typeparam>
/// <typeparam name="TResult">The result type produced.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the query asynchronously.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
