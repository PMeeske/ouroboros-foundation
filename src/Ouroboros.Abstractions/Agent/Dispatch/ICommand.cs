// <copyright file="ICommand.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Agent.Dispatch;

/// <summary>
/// Marker interface for CQRS commands — operations that change state.
/// Commands carry intent and data for a single operation.
/// </summary>
/// <typeparam name="TResult">The type of result produced by handling this command.</typeparam>
[ExcludeFromCodeCoverage]
public interface ICommand<TResult>;
