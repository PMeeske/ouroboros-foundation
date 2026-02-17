// <copyright file="KleisliSet.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Monads;

/// <summary>
/// Kleisli arrow for computations that produce multiple results (IEnumerable).
/// Represents a function A → IEnumerable(B) with monadic composition.
/// Supports covariance for output and contravariance for input.
/// </summary>
/// <typeparam name="TIn">The input type (contravariant).</typeparam>
/// <typeparam name="TOut">The output type (covariant).</typeparam>
/// <param name="input">The input value.</param>
/// <returns>An enumerable sequence of results.</returns>
public delegate IEnumerable<TOut> KleisliSet<in TIn, out TOut>(TIn input);