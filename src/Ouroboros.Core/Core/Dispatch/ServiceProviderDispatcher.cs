// <copyright file="ServiceProviderDispatcher.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Ouroboros.Abstractions.Agent.Dispatch;

namespace Ouroboros.Core.Dispatch;

/// <summary>
/// Default <see cref="IDispatcher"/> implementation that resolves handlers from
/// <see cref="IServiceProvider"/>. Register command/query handlers in DI as their
/// closed generic interface (e.g. <c>ICommandHandler&lt;MyCommand, MyResult&gt;</c>).
/// </summary>
public sealed class ServiceProviderDispatcher : IDispatcher
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProviderDispatcher"/> class.
    /// </summary>
    /// <param name="services">The DI service provider.</param>
    public ServiceProviderDispatcher(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <inheritdoc/>
    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        Type commandType = command.GetType();
        Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        object handler = _services.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No command handler registered for {commandType.Name}. " +
                $"Register an ICommandHandler<{commandType.Name}, {typeof(TResult).Name}> in DI.");

        MethodInfo method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync))!;
        return (Task<TResult>)method.Invoke(handler, [command, ct])!;
    }

    /// <inheritdoc/>
    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        Type queryType = query.GetType();
        Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

        object handler = _services.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No query handler registered for {queryType.Name}. " +
                $"Register an IQueryHandler<{queryType.Name}, {typeof(TResult).Name}> in DI.");

        MethodInfo method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync))!;
        return (Task<TResult>)method.Invoke(handler, [query, ct])!;
    }
}
