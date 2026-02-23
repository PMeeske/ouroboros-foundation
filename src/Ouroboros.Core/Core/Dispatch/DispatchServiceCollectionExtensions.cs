// <copyright file="DispatchServiceCollectionExtensions.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouroboros.Abstractions.Agent.Dispatch;

namespace Ouroboros.Core.Dispatch;

/// <summary>
/// DI extensions for registering the CQRS dispatcher and handler discovery.
/// </summary>
public static class DispatchServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IDispatcher"/> and scans the given assemblies for
    /// <see cref="ICommandHandler{TCommand, TResult}"/> and
    /// <see cref="IQueryHandler{TQuery, TResult}"/> implementations.
    /// </summary>
    /// <param name="services">The DI container.</param>
    /// <param name="assemblies">Assemblies to scan for handlers. If empty, scans the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOuroborosDispatch(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.TryAddSingleton<IDispatcher, ServiceProviderDispatcher>();

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            RegisterHandlersFromAssembly(services, assembly);
        }

        return services;
    }

    private static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaces = new[]
        {
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
        };

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                {
                    continue;
                }

                var genericDef = iface.GetGenericTypeDefinition();
                if (Array.Exists(handlerInterfaces, h => h == genericDef))
                {
                    services.TryAddTransient(iface, type);
                }
            }
        }
    }
}
