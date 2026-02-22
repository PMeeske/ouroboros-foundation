// <copyright file="IRandomProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Random;

/// <summary>
/// Abstraction over a source of random numbers.
/// Implementations may use cryptographic or deterministic (seeded) generators,
/// allowing consumers to be decoupled from the concrete randomness source.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a non-negative random integer in [0, <paramref name="maxValue"/>).
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound. Must be greater than zero.</param>
    /// <returns>A random integer in [0, <paramref name="maxValue"/>).</returns>
    int Next(int maxValue);

    /// <summary>
    /// Returns a random integer in [<paramref name="minValue"/>, <paramref name="maxValue"/>).
    /// </summary>
    /// <param name="minValue">The inclusive lower bound.</param>
    /// <param name="maxValue">The exclusive upper bound. Must be &gt;= <paramref name="minValue"/>.</param>
    /// <returns>A random integer in the specified range.</returns>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// Returns a random double in [0.0, 1.0).
    /// </summary>
    /// <returns>A random double-precision floating-point number.</returns>
    double NextDouble();

    /// <summary>
    /// Fills the provided buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    void NextBytes(byte[] buffer);
}
