// <copyright file="SeededRandomProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Randomness;

using Ouroboros.Providers.Random;

/// <summary>
/// An <see cref="IRandomProvider"/> backed by <see cref="System.Random"/>.
/// Useful for reproducible scenarios (e.g., genetic algorithms, simulations) where a fixed seed
/// is required to replay the same sequence of random numbers.
/// </summary>
/// <remarks>
/// This implementation is <b>not</b> thread-safe. Each thread or component that requires
/// independent randomness should hold its own <see cref="SeededRandomProvider"/> instance.
/// For cryptographically strong randomness use <see cref="CryptoRandomProvider"/> instead.
/// </remarks>
public sealed class SeededRandomProvider : IRandomProvider
{
    private readonly Random random;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeededRandomProvider"/> class
    /// with a time-dependent seed (equivalent to <c>new Random()</c>).
    /// </summary>
    public SeededRandomProvider()
    {
        this.random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SeededRandomProvider"/> class
    /// with the specified seed for reproducible sequences.
    /// </summary>
    /// <param name="seed">The seed value.</param>
    public SeededRandomProvider(int seed)
    {
        this.random = new Random(seed);
    }

    /// <inheritdoc/>
    public int Next(int maxValue) => this.random.Next(maxValue);

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue) => this.random.Next(minValue, maxValue);

    /// <inheritdoc/>
    public double NextDouble() => this.random.NextDouble();

    /// <inheritdoc/>
    public void NextBytes(byte[] buffer) => this.random.NextBytes(buffer);
}
