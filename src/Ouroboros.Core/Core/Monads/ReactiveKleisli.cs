// <copyright file="ReactiveKleisli.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Monads;

/// <summary>
/// Kleisli arrow for reactive computations (IObservable).
/// Represents a function A → IObservable(B) with monadic composition.
/// Supports covariance for output and contravariance for input.
/// </summary>
/// <typeparam name="TIn">The input type (contravariant).</typeparam>
/// <typeparam name="TOut">The output type (covariant).</typeparam>
/// <param name="input">The input value.</param>
/// <returns>An observable sequence of results.</returns>
public delegate IObservable<TOut> ReactiveKleisli<in TIn, out TOut>(TIn input);
