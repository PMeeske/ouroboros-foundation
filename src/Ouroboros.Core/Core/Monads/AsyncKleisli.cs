// <copyright file="AsyncKleisli.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Monads;

/// <summary>
/// Kleisli arrow for asynchronous streaming computations (IAsyncEnumerable).
/// Represents a function A → IAsyncEnumerable(B) with monadic composition.
/// Supports covariance for output and contravariance for input.
/// </summary>
/// <typeparam name="TIn">The input type (contravariant).</typeparam>
/// <typeparam name="TOut">The output type (covariant).</typeparam>
/// <param name="input">The input value.</param>
/// <returns>An asynchronous enumerable sequence of results.</returns>
public delegate IAsyncEnumerable<TOut> AsyncKleisli<in TIn, out TOut>(TIn input);