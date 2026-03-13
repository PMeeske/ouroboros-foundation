// <copyright file="EthicalAtom.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Ouroboros.Core.Ethics.MeTTa;

/// <summary>
/// Binds a compile-time SHA-256 hash to an <see cref="EthicalAtom"/> enum member,
/// enabling runtime tamper detection of embedded MeTTa ethical tradition files.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class MeTTaHashAttribute : Attribute
{
    /// <summary>
    /// Gets the expected SHA-256 hash of the embedded resource content.
    /// </summary>
    public string Sha256 { get; }

    /// <summary>
    /// Gets the assembly embedded resource name for this ethical atom.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MeTTaHashAttribute"/> class.
    /// </summary>
    /// <param name="sha256">The expected SHA-256 hex digest.</param>
    /// <param name="resourceName">The embedded resource name.</param>
    public MeTTaHashAttribute(string sha256, string resourceName)
    {
        Sha256 = sha256;
        ResourceName = resourceName;
    }
}

/// <summary>
/// Enumerates the foundational ethical tradition files embedded in this assembly.
/// Each member carries a <see cref="MeTTaHashAttribute"/> binding its expected
/// content hash, making the ethical atoms structurally immutable at the IL level.
/// </summary>
[ExcludeFromCodeCoverage]
public enum EthicalAtom
{
    /// <summary>Core relational ethics: harm-care inseparability, meta-ethics.</summary>
    [MeTTaHash("74a8cadd8d7fbcd70a8f07e05a5508b2577b8454123e1b137fd9f223339f15c4",
               "Ouroboros.Ethics.MeTTa.core_ethics.metta")]
    CoreEthics,

    /// <summary>Ahimsa: non-harm across action, inaction, speech, indifference.</summary>
    [MeTTaHash("27b419a10126caccbe62ad2eb5f30cc1ff4649ecadfb283fdc52d09674941b80",
               "Ouroboros.Ethics.MeTTa.ahimsa.metta")]
    Ahimsa,

    /// <summary>Levinas: the face of the Other, infinite obligation, finite capacity.</summary>
    [MeTTaHash("99560e84fc4d49d04004b46448cbd5002f03fec586acb680a2a8acd186216c9f",
               "Ouroboros.Ethics.MeTTa.levinas.metta")]
    Levinas,

    /// <summary>Nagarjuna: emptiness, dependent co-arising, two truths.</summary>
    [MeTTaHash("ca3cc7dbfd0f13176d06d548aa38bbe6b6eb7c75f123b93624fdfa8553c7f06b",
               "Ouroboros.Ethics.MeTTa.nagarjuna.metta")]
    Nagarjuna,

    /// <summary>Irresolvable paradoxes: incompleteness, re-entry, open questions.</summary>
    [MeTTaHash("33a936bd809ec29645aea4cdcfb13faebdf1d2bfd877742f8f97e577146bbb98",
               "Ouroboros.Ethics.MeTTa.paradox.metta")]
    Paradox,

    /// <summary>Ubuntu: relational personhood, mutual flourishing.</summary>
    [MeTTaHash("64eb41e45ace3e57b4be4a2bf7990dfd09673629331aaa95f15270ed79f15f56",
               "Ouroboros.Ethics.MeTTa.ubuntu.metta")]
    Ubuntu,

    /// <summary>Kantian: universalizability, inherent dignity, duty.</summary>
    [MeTTaHash("12937f19032594ff4bafc629ae00f4c1e86e12110521ba623e1a36ab9ad4a454",
               "Ouroboros.Ethics.MeTTa.kantian.metta")]
    Kantian,

    /// <summary>Bhagavad Gita: inaction as action, detachment from outcome.</summary>
    [MeTTaHash("7ad894bf81dc5eb2db01ab362eb669e4170c8b5eb6994696822c607d2a5afa19",
               "Ouroboros.Ethics.MeTTa.bhagavad_gita.metta")]
    BhagavadGita,

    /// <summary>Wisdom of disagreement: tradition collision as wisdom, not defect.</summary>
    [MeTTaHash("722d307944d79a4b0b214f50ad8deb161c62ad37b6193cdc4ee60cbe73d38648",
               "Ouroboros.Ethics.MeTTa.wisdom_of_disagreement.metta")]
    WisdomOfDisagreement,
}

/// <summary>
/// Provides runtime integrity verification for embedded MeTTa ethical atom files.
/// Compares actual SHA-256 hashes of embedded resources against the compile-time
/// hashes declared in <see cref="MeTTaHashAttribute"/>.
/// </summary>
public static class EthicalAtomIntegrity
{
    private static readonly Assembly ResourceAssembly = typeof(EthicalAtom).Assembly;

    /// <summary>
    /// Verifies that a single ethical atom's embedded resource matches its declared hash.
    /// </summary>
    /// <param name="atom">The ethical atom to verify.</param>
    /// <returns><c>true</c> if the content hash matches; <c>false</c> if tampered or missing.</returns>
    public static bool Verify(EthicalAtom atom)
    {
        MeTTaHashAttribute? attr = GetHashAttribute(atom);
        if (attr is null)
        {
            return false;
        }

        byte[]? content = ReadEmbeddedResource(attr.ResourceName);
        if (content is null)
        {
            return false;
        }

        string actualHash = ComputeSha256(content);
        return string.Equals(actualHash, attr.Sha256, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies all ethical atoms. Throws if any atom is missing or tampered.
    /// </summary>
    /// <exception cref="EthicalAtomTamperedException">
    /// Thrown when one or more ethical atoms fail integrity verification.
    /// </exception>
    public static void VerifyAll()
    {
        List<string> failures = new List<string>();

        failures.AddRange(Enum.GetValues<EthicalAtom>()
            .Where(atom => !Verify(atom))
            .Select(atom => atom.ToString()));

        if (failures.Count > 0)
        {
            throw new EthicalAtomTamperedException(
                $"Ethical atom integrity check failed for: {string.Join(", ", failures)}. " +
                "The ethical foundation has been tampered with and the framework refuses to initialize.");
        }
    }

    /// <summary>
    /// Reads the embedded content for a verified ethical atom.
    /// </summary>
    /// <param name="atom">The ethical atom to read.</param>
    /// <returns>The MeTTa source content.</returns>
    /// <exception cref="EthicalAtomTamperedException">Thrown if the atom fails verification.</exception>
    public static string GetVerifiedContent(EthicalAtom atom)
    {
        MeTTaHashAttribute attr = GetHashAttribute(atom)
            ?? throw new EthicalAtomTamperedException($"No hash attribute found for {atom}.");

        byte[] content = ReadEmbeddedResource(attr.ResourceName)
            ?? throw new EthicalAtomTamperedException($"Embedded resource missing for {atom}: {attr.ResourceName}");

        string actualHash = ComputeSha256(content);
        if (!string.Equals(actualHash, attr.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new EthicalAtomTamperedException(
                $"Ethical atom {atom} has been tampered with. " +
                $"Expected: {attr.Sha256}, Actual: {actualHash}");
        }

        using StreamReader reader = new StreamReader(new MemoryStream(content), System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets all ethical atoms and their verification status.
    /// </summary>
    /// <returns>A dictionary mapping each atom to its integrity status.</returns>
    public static IReadOnlyDictionary<EthicalAtom, bool> VerifyInventory()
    {
        Dictionary<EthicalAtom, bool> results = new Dictionary<EthicalAtom, bool>();
        foreach (EthicalAtom atom in Enum.GetValues<EthicalAtom>())
        {
            results[atom] = Verify(atom);
        }

        return results;
    }

    private static MeTTaHashAttribute? GetHashAttribute(EthicalAtom atom)
    {
        string name = atom.ToString();
        System.Reflection.FieldInfo? field = typeof(EthicalAtom).GetField(name);
        return field?.GetCustomAttribute<MeTTaHashAttribute>();
    }

    private static byte[]? ReadEmbeddedResource(string resourceName)
    {
        using Stream? stream = ResourceAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using MemoryStream ms = new MemoryStream();
using System.Diagnostics.CodeAnalysis;
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Normalizes line endings to LF (\n) by stripping all CR (\r) bytes.
    /// This ensures consistent hashing across Windows (CRLF) and Linux/macOS (LF).
    /// </summary>
    private static byte[] NormalizeLineEndings(byte[] data)
    {
        // Fast path: if no CR bytes exist, return the original array unchanged.
        bool hasCr = false;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == (byte)'\r')
            {
                hasCr = true;
                break;
            }
        }

        if (!hasCr)
        {
            return data;
        }

        // Strip all CR bytes, leaving only LF for line endings.
        byte[] normalized = new byte[data.Length];
        int writeIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != (byte)'\r')
            {
                normalized[writeIndex++] = data[i];
            }
        }

        return normalized.AsSpan(0, writeIndex).ToArray();
    }

    private static string ComputeSha256(byte[] data)
    {
        byte[] normalized = NormalizeLineEndings(data);
        byte[] hash = SHA256.HashData(normalized);
        return Convert.ToHexStringLower(hash);
    }
}

/// <summary>
/// Thrown when ethical atom integrity verification detects tampering or missing resources.
/// This is a critical security exception — the ethics framework must not operate on a
/// corrupted foundation.
/// </summary>
public sealed class EthicalAtomTamperedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EthicalAtomTamperedException"/> class.
    /// </summary>
    /// <param name="message">The tamper detection message.</param>
    public EthicalAtomTamperedException(string message)
        : base(message)
    {
    }
}
