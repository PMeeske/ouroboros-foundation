// <copyright file="EcVectorCrypto.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Encrypts and decrypts float[] vectors using elliptic-curve cryptography.
///
/// Two modes are provided:
///
/// <b>Envelope mode</b> (Encrypt / Decrypt):
///   ECIES with ephemeral ECDH P-256 + AES-256-GCM.
///   Output is an opaque byte blob (base64 for JSON storage).
///
/// <b>Per-index mode</b> (EncryptPerIndex / DecryptPerIndex):
///   ECDH P-256 derives a master secret → HKDF expands a keystream
///   of length = dimension × 4 bytes → XOR each float's IEEE 754 bytes.
///   The point ID is mixed into the HKDF info for per-vector uniqueness.
///   Output is a float[] of the <em>same dimension</em>, directly storable
///   as a Qdrant vector (shape-preserving encryption).
/// </summary>
public sealed class EcVectorCrypto : IDisposable
{
    private readonly ECDiffieHellman _privateKey;

    // ── Envelope mode constants ───────────────────────────────────────────
    private const int EphemeralPubKeySize = 65; // uncompressed P-256
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int HeaderSize = EphemeralPubKeySize + NonceSize + TagSize;

    // ── Per-index mode: pre-derived master keystream seed ─────────────────
    private readonly byte[] _perIndexMasterSecret;

    /// <summary>
    /// Creates an instance with a new random P-256 key pair.
    /// </summary>
    public EcVectorCrypto()
    {
        _privateKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        _perIndexMasterSecret = DeriveMasterSecret();
    }

    /// <summary>
    /// Creates an instance from an existing PKCS#8 private key.
    /// </summary>
    public EcVectorCrypto(byte[] pkcs8PrivateKey)
    {
        _privateKey = ECDiffieHellman.Create();
        _privateKey.ImportPkcs8PrivateKey(pkcs8PrivateKey, out _);
        _perIndexMasterSecret = DeriveMasterSecret();
    }

    /// <summary>
    /// Creates an instance from a base64-encoded PKCS#8 private key.
    /// </summary>
    public EcVectorCrypto(string base64PrivateKey)
        : this(Convert.FromBase64String(base64PrivateKey))
    {
    }

    /// <summary>
    /// Exports the public key as a base64 string (for config / cloud metadata).
    /// </summary>
    public string ExportPublicKeyBase64()
    {
        var param = _privateKey.ExportParameters(includePrivateParameters: false);
        var pub = new byte[65];
        pub[0] = 0x04;
        param.Q.X!.CopyTo(pub.AsSpan(1));
        param.Q.Y!.CopyTo(pub.AsSpan(33));
        return Convert.ToBase64String(pub);
    }

    /// <summary>
    /// Exports the private key as a base64-encoded PKCS#8 blob (keep secret!).
    /// </summary>
    public string ExportPrivateKeyBase64()
        => Convert.ToBase64String(_privateKey.ExportPkcs8PrivateKey());

    // ═══════════════════════════════════════════════════════════════════════
    //  PER-INDEX MODE — shape-preserving float[] → float[]
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Encrypts a vector per-index: each float dimension is XOR'd with an
    /// EC-derived keystream byte block. The output is a float[] of the same
    /// length, directly storable as a Qdrant vector.
    /// </summary>
    /// <param name="vector">The plaintext vector.</param>
    /// <param name="pointId">The Qdrant point ID — mixed into the key derivation
    /// so identical vectors at different IDs produce different ciphertexts.</param>
    public float[] EncryptPerIndex(float[] vector, string pointId)
    {
        ArgumentNullException.ThrowIfNull(vector);
        if (vector.Length == 0)
            throw new ArgumentException("Vector must not be empty.", nameof(vector));

        var keystream = DeriveKeystream(vector.Length, pointId);
        return XorVectorWithKeystream(vector, keystream);
    }

    /// <summary>
    /// Decrypts a per-index encrypted vector back to plaintext.
    /// XOR is symmetric, so this is the same operation as encrypt.
    /// </summary>
    /// <param name="encryptedVector">The encrypted float[] from <see cref="EncryptPerIndex"/>.</param>
    /// <param name="pointId">The same point ID used during encryption.</param>
    public float[] DecryptPerIndex(float[] encryptedVector, string pointId)
    {
        // XOR is its own inverse
        return EncryptPerIndex(encryptedVector, pointId);
    }

    /// <summary>
    /// Computes an HMAC-SHA256 over the plaintext vector bytes, keyed by
    /// master-secret + point ID. Use this to detect tampering or corruption
    /// of encrypted vectors stored on cloud.
    /// </summary>
    /// <param name="plaintextVector">The original unencrypted vector.</param>
    /// <param name="pointId">The Qdrant point ID (same as used for encryption).</param>
    /// <returns>Base64-encoded HMAC (43 chars).</returns>
    public string ComputeVectorHmac(float[] plaintextVector, string pointId)
    {
        ArgumentNullException.ThrowIfNull(plaintextVector);

        // Derive a per-point HMAC key (separate from the encryption keystream)
        var hmacKey = HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm: _perIndexMasterSecret,
            outputLength: 32,
            salt: Array.Empty<byte>(),
            info: System.Text.Encoding.UTF8.GetBytes($"iaret-vec-hmac:{pointId}"));

        // Serialize vector to bytes
        var data = new byte[plaintextVector.Length * sizeof(float)];
        for (int i = 0; i < plaintextVector.Length; i++)
            BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(i * 4), plaintextVector[i]);

        var mac = HMACSHA256.HashData(hmacKey, data);
        return Convert.ToBase64String(mac);
    }

    /// <summary>
    /// Verifies an encrypted cloud vector by decrypting it and checking the HMAC.
    /// </summary>
    /// <param name="encryptedVector">The encrypted float[] from cloud.</param>
    /// <param name="pointId">The Qdrant point ID.</param>
    /// <param name="expectedHmac">The base64 HMAC stored alongside the vector.</param>
    /// <returns>True if the vector is intact; false if tampered or corrupted.</returns>
    public bool VerifyVectorHmac(float[] encryptedVector, string pointId, string expectedHmac)
    {
        var decrypted = DecryptPerIndex(encryptedVector, pointId);
        var actualHmac = ComputeVectorHmac(decrypted, pointId);
        return string.Equals(actualHmac, expectedHmac, StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ENVELOPE MODE — full ECIES (opaque blob)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Encrypts a float[] vector into a self-contained byte envelope.
    /// Uses an ephemeral ECDH key so each call produces a unique ciphertext.
    /// </summary>
    public byte[] Encrypt(float[] vector)
    {
        ArgumentNullException.ThrowIfNull(vector);
        if (vector.Length == 0)
            throw new ArgumentException("Vector must not be empty.", nameof(vector));

        using var ephemeral = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var sharedSecret = DeriveAesKey(ephemeral, _privateKey.PublicKey);

        var plaintext = new byte[vector.Length * sizeof(float)];
        for (int i = 0; i < vector.Length; i++)
            BinaryPrimitives.WriteSingleLittleEndian(plaintext.AsSpan(i * 4), vector[i]);

        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(sharedSecret, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var ephemeralPub = ExportUncompressedPublicKey(ephemeral);
        var envelope = new byte[HeaderSize + ciphertext.Length];
        ephemeralPub.CopyTo(envelope, 0);
        nonce.CopyTo(envelope, EphemeralPubKeySize);
        tag.CopyTo(envelope, EphemeralPubKeySize + NonceSize);
        ciphertext.CopyTo(envelope, HeaderSize);

        return envelope;
    }

    /// <summary>
    /// Decrypts a byte envelope back into a float[] vector.
    /// </summary>
    public float[] Decrypt(byte[] envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (envelope.Length <= HeaderSize)
            throw new ArgumentException("Envelope too small to contain encrypted vector.", nameof(envelope));

        var ephemeralPubBytes = envelope.AsSpan(0, EphemeralPubKeySize);
        var nonce = envelope.AsSpan(EphemeralPubKeySize, NonceSize);
        var tag = envelope.AsSpan(EphemeralPubKeySize + NonceSize, TagSize);
        var ciphertext = envelope.AsSpan(HeaderSize);

        using var ephemeralPub = ECDiffieHellman.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = ephemeralPubBytes.Slice(1, 32).ToArray(),
                Y = ephemeralPubBytes.Slice(33, 32).ToArray()
            }
        });

        var sharedSecret = DeriveAesKey(_privateKey, ephemeralPub.PublicKey);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(sharedSecret, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        int count = plaintext.Length / sizeof(float);
        var vector = new float[count];
        for (int i = 0; i < count; i++)
            vector[i] = BinaryPrimitives.ReadSingleLittleEndian(plaintext.AsSpan(i * 4));

        return vector;
    }

    /// <summary>
    /// Encrypts a vector and returns a base64 string (envelope mode).
    /// </summary>
    public string EncryptToBase64(float[] vector)
        => Convert.ToBase64String(Encrypt(vector));

    /// <summary>
    /// Decrypts a base64 envelope string back into a float[] vector (envelope mode).
    /// </summary>
    public float[] DecryptFromBase64(string base64Envelope)
        => Decrypt(Convert.FromBase64String(base64Envelope));

    // ═══════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Derives a deterministic master secret from the private key using
    /// ECDH self-agreement (private key × own public key) + HKDF.
    /// </summary>
    private byte[] DeriveMasterSecret()
    {
        var rawSecret = _privateKey.DeriveRawSecretAgreement(_privateKey.PublicKey);
        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm: rawSecret,
            outputLength: 32,
            salt: "ouroboros-iaret-perindex-master"u8.ToArray(),
            info: "master-keystream-seed"u8.ToArray());
    }

    /// <summary>
    /// Derives a per-point keystream of exactly <paramref name="dimension"/> × 4 bytes.
    /// The point ID is mixed into the HKDF info so each vector has a unique keystream.
    /// </summary>
    private byte[] DeriveKeystream(int dimension, string pointId)
    {
        var info = System.Text.Encoding.UTF8.GetBytes($"iaret-vec-idx:{pointId}");
        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm: _perIndexMasterSecret,
            outputLength: dimension * sizeof(float),
            salt: Array.Empty<byte>(),
            info: info);
    }

    /// <summary>
    /// XORs each float's 4 IEEE 754 bytes with the corresponding keystream bytes.
    /// </summary>
    private static float[] XorVectorWithKeystream(float[] vector, byte[] keystream)
    {
        var result = new float[vector.Length];
        Span<byte> floatBytes = stackalloc byte[4];

        for (int i = 0; i < vector.Length; i++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(floatBytes, vector[i]);

            int offset = i * 4;
            floatBytes[0] ^= keystream[offset];
            floatBytes[1] ^= keystream[offset + 1];
            floatBytes[2] ^= keystream[offset + 2];
            floatBytes[3] ^= keystream[offset + 3];

            result[i] = BinaryPrimitives.ReadSingleLittleEndian(floatBytes);
        }

        return result;
    }

    private static byte[] DeriveAesKey(ECDiffieHellman ourKey, ECDiffieHellmanPublicKey theirPubKey)
    {
        var rawSecret = ourKey.DeriveRawSecretAgreement(theirPubKey);
        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm: rawSecret,
            outputLength: 32,
            salt: "ouroboros-iaret-vector-sync"u8.ToArray(),
            info: "aes-256-gcm-key"u8.ToArray());
    }

    private static byte[] ExportUncompressedPublicKey(ECDiffieHellman key)
    {
        var param = key.ExportParameters(includePrivateParameters: false);
        var pub = new byte[65];
        pub[0] = 0x04;
        param.Q.X!.CopyTo(pub, 1);
        param.Q.Y!.CopyTo(pub, 33);
        return pub;
    }

    /// <inheritdoc/>
    public void Dispose() => _privateKey.Dispose();
}
