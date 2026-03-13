// <copyright file="ICommandHandler.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Agent.Dispatch;

/// <summary>
/// Handles a single <see cref="ICommand{TResult}"/> and produces a result.
/// Register implementations in DI to enable dispatch via <see cref="IDispatcher"/>.
/// </summary>
/// <typeparam name="TCommand">The command type to handle.</typeparam>
/// <typeparam name="TResult">The result type produced.</typeparam>
[ExcludeFromCodeCoverage]
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of handling the command.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
