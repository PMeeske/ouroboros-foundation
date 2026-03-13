using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class EcVectorCryptoTests
{
    private static float[] MakeVector(int length = 8)
    {
        var v = new float[length];
        for (int i = 0; i < length; i++) v[i] = (i + 1) * 1.5f;
        return v;
    }

    [Fact]
    public void PerIndex_EncryptDecrypt_ShouldRoundTrip()
    {
        using var crypto = new EcVectorCrypto();
        var original = MakeVector();

        var encrypted = crypto.EncryptPerIndex(original, "point-1");
        var decrypted = crypto.DecryptPerIndex(encrypted, "point-1");

        decrypted.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void PerIndex_DifferentPointIds_ShouldProduceDifferentCiphertexts()
    {
        using var crypto = new EcVectorCrypto();
        var vector = MakeVector();

        var enc1 = crypto.EncryptPerIndex(vector, "point-1");
        var enc2 = crypto.EncryptPerIndex(vector, "point-2");

        enc1.Should().NotBeEquivalentTo(enc2);
    }

    [Fact]
    public void PerIndex_EncryptedVector_ShouldHaveSameLength()
    {
        using var crypto = new EcVectorCrypto();
        var vector = MakeVector(16);

        var encrypted = crypto.EncryptPerIndex(vector, "point-1");

        encrypted.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void Envelope_EncryptDecrypt_ShouldRoundTrip()
    {
        using var crypto = new EcVectorCrypto();
        var original = MakeVector();

        var encrypted = crypto.Encrypt(original);
        var decrypted = crypto.Decrypt(encrypted);

        for (int i = 0; i < original.Length; i++)
            decrypted[i].Should().BeApproximately(original[i], 0.0001f);
    }

    [Fact]
    public void Envelope_EncryptToBase64_ShouldRoundTrip()
    {
        using var crypto = new EcVectorCrypto();
        var original = MakeVector();

        var b64 = crypto.EncryptToBase64(original);
        var decrypted = crypto.DecryptFromBase64(b64);

        for (int i = 0; i < original.Length; i++)
            decrypted[i].Should().BeApproximately(original[i], 0.0001f);
    }

    [Fact]
    public void ComputeVectorHmac_ShouldReturnConsistentHmac()
    {
        using var crypto = new EcVectorCrypto();
        var vector = MakeVector();

        var hmac1 = crypto.ComputeVectorHmac(vector, "point-1");
        var hmac2 = crypto.ComputeVectorHmac(vector, "point-1");

        hmac1.Should().Be(hmac2);
    }

    [Fact]
    public void VerifyVectorHmac_ValidHmac_ShouldReturnTrue()
    {
        using var crypto = new EcVectorCrypto();
        var vector = MakeVector();
        var hmac = crypto.ComputeVectorHmac(vector, "point-1");
        var encrypted = crypto.EncryptPerIndex(vector, "point-1");

        crypto.VerifyVectorHmac(encrypted, "point-1", hmac).Should().BeTrue();
    }

    [Fact]
    public void VerifyVectorHmac_InvalidHmac_ShouldReturnFalse()
    {
        using var crypto = new EcVectorCrypto();
        var vector = MakeVector();
        var encrypted = crypto.EncryptPerIndex(vector, "point-1");

        crypto.VerifyVectorHmac(encrypted, "point-1", "invalid-hmac").Should().BeFalse();
    }

    [Fact]
    public void ExportPublicKeyBase64_ShouldReturnNonEmptyString()
    {
        using var crypto = new EcVectorCrypto();
        var publicKey = crypto.ExportPublicKeyBase64();

        publicKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExportPrivateKeyBase64_ShouldRoundTrip()
    {
        using var crypto1 = new EcVectorCrypto();
        var privateKey = crypto1.ExportPrivateKeyBase64();

        using var crypto2 = new EcVectorCrypto(privateKey);
        var vector = MakeVector();
        var encrypted = crypto1.EncryptPerIndex(vector, "p1");
        var decrypted = crypto2.DecryptPerIndex(encrypted, "p1");

        decrypted.Should().BeEquivalentTo(vector);
    }

    [Fact]
    public void EncryptPerIndex_EmptyVector_ShouldThrow()
    {
        using var crypto = new EcVectorCrypto();
        var act = () => crypto.EncryptPerIndex(Array.Empty<float>(), "p1");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EncryptPerIndex_NullVector_ShouldThrow()
    {
        using var crypto = new EcVectorCrypto();
        var act = () => crypto.EncryptPerIndex(null!, "p1");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_NullVector_ShouldThrow()
    {
        using var crypto = new EcVectorCrypto();
        var act = () => crypto.Encrypt(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Decrypt_TooSmallEnvelope_ShouldThrow()
    {
        using var crypto = new EcVectorCrypto();
        var act = () => crypto.Decrypt(new byte[10]);
        act.Should().Throw<ArgumentException>();
    }
}
