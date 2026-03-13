// <copyright file="IQuery.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Agent.Dispatch;

/// <summary>
/// Marker interface for CQRS queries — read-only operations that do not change state.
/// </summary>
/// <typeparam name="TResult">The type of result produced by handling this query.</typeparam>
[ExcludeFromCodeCoverage]
public interface IQuery<TResult>;
