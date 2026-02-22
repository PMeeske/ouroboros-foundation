// <copyright file="CryptoRandomProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Randomness;

using System.Security.Cryptography;
using Ouroboros.Providers.Random;

/// <summary>
/// An <see cref="IRandomProvider"/> backed by <see cref="RandomNumberGenerator"/>.
/// All values are cryptographically strong and suitable for security-sensitive contexts.
/// This implementation is stateless and thread-safe.
/// </summary>
public sealed class CryptoRandomProvider : IRandomProvider
{
    /// <summary>
    /// Gets the default, shared singleton instance.
    /// </summary>
    public static readonly CryptoRandomProvider Instance = new();

    /// <inheritdoc/>
    public int Next(int maxValue)
    {
        if (maxValue <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than zero.");
        }

        return RandomNumberGenerator.GetInt32(maxValue);
    }

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue)
    {
        if (maxValue < minValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be >= minValue.");
        }

        if (maxValue == minValue)
        {
            return minValue;
        }

        return RandomNumberGenerator.GetInt32(minValue, maxValue);
    }

    /// <inheritdoc/>
    public double NextDouble()
    {
        // Fill 8 bytes and map to [0.0, 1.0) using the upper 53 bits (double mantissa precision).
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong value = System.Runtime.InteropServices.MemoryMarshal.Read<ulong>(bytes);
        // Keep 53 significant bits, then divide by 2^53 to get a uniform value in [0, 1).
        return (value >> 11) * (1.0 / (1UL << 53));
    }

    /// <inheritdoc/>
    public void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        RandomNumberGenerator.Fill(buffer);
    }
}
