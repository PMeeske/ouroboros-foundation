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
public enum EthicalAtom
{
    /// <summary>Core relational ethics: harm-care inseparability, meta-ethics.</summary>
    [MeTTaHash("b7f19e1a66b4e0459a4ad49fff7233215b25ac35d9a6d2dfe05c1ffd4c66e41c",
               "Ouroboros.Ethics.MeTTa.core_ethics.metta")]
    CoreEthics,

    /// <summary>Ahimsa: non-harm across action, inaction, speech, indifference.</summary>
    [MeTTaHash("2e275945ed960f556010447ec9dc98410ff8f8706de98aba7e512ebe726819e9",
               "Ouroboros.Ethics.MeTTa.ahimsa.metta")]
    Ahimsa,

    /// <summary>Levinas: the face of the Other, infinite obligation, finite capacity.</summary>
    [MeTTaHash("a7f023ed1366f2ddb9e539a001c0fb0a96b7cd48584d49e65f4f03323fcf8f3d",
               "Ouroboros.Ethics.MeTTa.levinas.metta")]
    Levinas,

    /// <summary>Nagarjuna: emptiness, dependent co-arising, two truths.</summary>
    [MeTTaHash("ab93b56ba2e8f0e02658333b6f33ff2aaa1fb06ad174ea2359096780221ab108",
               "Ouroboros.Ethics.MeTTa.nagarjuna.metta")]
    Nagarjuna,

    /// <summary>Irresolvable paradoxes: incompleteness, re-entry, open questions.</summary>
    [MeTTaHash("c2961f22105ef08fec6e02281108dbe0c67d339a7b5cdc2dbdacdf9d31b755b3",
               "Ouroboros.Ethics.MeTTa.paradox.metta")]
    Paradox,

    /// <summary>Ubuntu: relational personhood, mutual flourishing.</summary>
    [MeTTaHash("98c70b5b8e7c245ad1bdbd36e3f1cf607072ac9eac4d9e27fd19c1e7220d3f41",
               "Ouroboros.Ethics.MeTTa.ubuntu.metta")]
    Ubuntu,

    /// <summary>Kantian: universalizability, inherent dignity, duty.</summary>
    [MeTTaHash("e9d169f53b357f7fce3b116fd330e90213259b99c1f85537a80f7733a6c67c3c",
               "Ouroboros.Ethics.MeTTa.kantian.metta")]
    Kantian,

    /// <summary>Bhagavad Gita: inaction as action, detachment from outcome.</summary>
    [MeTTaHash("a6e14a0a0f3f47878ccc01b91871290286d9b78da8af55210c665ffbb88c30a0",
               "Ouroboros.Ethics.MeTTa.bhagavad_gita.metta")]
    BhagavadGita,

    /// <summary>Wisdom of disagreement: tradition collision as wisdom, not defect.</summary>
    [MeTTaHash("a4ba0a92b50dea81a9ce392174dbf880893861812263b92b2525f906e14333b1",
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

        foreach (EthicalAtom atom in Enum.GetValues<EthicalAtom>())
        {
            if (!Verify(atom))
            {
                failures.Add(atom.ToString());
            }
        }

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
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static string ComputeSha256(byte[] data)
    {
        byte[] hash = SHA256.HashData(data);
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
