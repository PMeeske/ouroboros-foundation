// <copyright file="Kleisli.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Kleisli;

/// <summary>
/// Kleisli arrow for computations in a monadic context.
/// Since Step{T,U} and Kleisli{T,U} are identical, this primarily serves as an alias for conceptual clarity.
/// A Kleisli arrow from A to B in monad M is a function A → M(B).
/// </summary>
/// <typeparam name="TInput">The input type.</typeparam>
/// <typeparam name="TOutput">The output type.</typeparam>
/// <param name="input">The input value.</param>
/// <returns>A Task representing the monadic computation.</returns>
public delegate Task<TOutput> Kleisli<in TInput, TOutput>(TInput input);