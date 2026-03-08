namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Domain.Vectors;

[Trait("Category", "Unit")]
public class EcVectorCryptoTests : IDisposable
{
    private readonly EcVectorCrypto _sut = new();

    [Fact]
    public void Encrypt_Decrypt_RoundTrip_ProducesOriginalVector()
    {
        // Arrange
        float[] original = { 1.0f, 2.5f, -3.7f, 0.0f, 100.0f };

        // Act
        byte[] encrypted = _sut.Encrypt(original);
        float[] decrypted = _sut.Decrypt(encrypted);

        // Assert
        decrypted.Should().Equal(original);
    }

    [Fact]
    public void EncryptPerIndex_DecryptPerIndex_RoundTrip_ProducesOriginalVector()
    {
        // Arrange
        float[] original = { 0.1f, 0.2f, 0.3f, 0.4f };
        string pointId = "test-point-1";

        // Act
        float[] encrypted = _sut.EncryptPerIndex(original, pointId);
        float[] decrypted = _sut.DecryptPerIndex(encrypted, pointId);

        // Assert
        decrypted.Should().Equal(original);
    }

    [Fact]
    public void EncryptPerIndex_PreservesVectorLength()
    {
        // Arrange
        float[] vector = new float[128];
        for (int i = 0; i < vector.Length; i++)
            vector[i] = i * 0.01f;

        // Act
        float[] encrypted = _sut.EncryptPerIndex(vector, "pt-1");

        // Assert
        encrypted.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void EncryptPerIndex_DifferentPointIds_ProduceDifferentCiphertexts()
    {
        // Arrange
        float[] vector = { 1.0f, 2.0f, 3.0f };

        // Act
        float[] enc1 = _sut.EncryptPerIndex(vector, "point-A");
        float[] enc2 = _sut.EncryptPerIndex(vector, "point-B");

        // Assert
        enc1.Should().NotEqual(enc2);
    }

    [Fact]
    public void EncryptPerIndex_ProducesDifferentOutputFromInput()
    {
        // Arrange
        float[] vector = { 1.0f, 2.0f, 3.0f };

        // Act
        float[] encrypted = _sut.EncryptPerIndex(vector, "pt-1");

        // Assert
        encrypted.Should().NotEqual(vector);
    }

    [Fact]
    public void Encrypt_EmptyVector_ThrowsArgumentException()
    {
        // Arrange
        float[] empty = Array.Empty<float>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.Encrypt(empty));
    }

    [Fact]
    public void EncryptPerIndex_EmptyVector_ThrowsArgumentException()
    {
        // Arrange
        float[] empty = Array.Empty<float>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.EncryptPerIndex(empty, "pt-1"));
    }

    [Fact]
    public void Encrypt_NullVector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Encrypt(null!));
    }

    [Fact]
    public void EncryptPerIndex_NullVector_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.EncryptPerIndex(null!, "pt"));
    }

    [Fact]
    public void Decrypt_NullEnvelope_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Decrypt(null!));
    }

    [Fact]
    public void Decrypt_TooSmallEnvelope_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.Decrypt(new byte[10]));
    }

    [Fact]
    public void EncryptToBase64_DecryptFromBase64_RoundTrip()
    {
        // Arrange
        float[] original = { 42.0f, -7.5f, 0.123f };

        // Act
        string base64 = _sut.EncryptToBase64(original);
        float[] decrypted = _sut.DecryptFromBase64(base64);

        // Assert
        decrypted.Should().Equal(original);
    }

    [Fact]
    public void ComputeVectorHmac_ReturnsDeterministicResult()
    {
        // Arrange
        float[] vector = { 1.0f, 2.0f, 3.0f };

        // Act
        string hmac1 = _sut.ComputeVectorHmac(vector, "pt-1");
        string hmac2 = _sut.ComputeVectorHmac(vector, "pt-1");

        // Assert
        hmac1.Should().Be(hmac2);
    }

    [Fact]
    public void ComputeVectorHmac_DifferentPointIds_ProduceDifferentHmacs()
    {
        // Arrange
        float[] vector = { 1.0f, 2.0f, 3.0f };

        // Act
        string hmac1 = _sut.ComputeVectorHmac(vector, "pt-1");
        string hmac2 = _sut.ComputeVectorHmac(vector, "pt-2");

        // Assert
        hmac1.Should().NotBe(hmac2);
    }

    [Fact]
    public void VerifyVectorHmac_ValidHmac_ReturnsTrue()
    {
        // Arrange
        float[] original = { 1.0f, 2.0f, 3.0f };
        string pointId = "pt-1";
        string hmac = _sut.ComputeVectorHmac(original, pointId);
        float[] encrypted = _sut.EncryptPerIndex(original, pointId);

        // Act
        bool valid = _sut.VerifyVectorHmac(encrypted, pointId, hmac);

        // Assert
        valid.Should().BeTrue();
    }

    [Fact]
    public void VerifyVectorHmac_InvalidHmac_ReturnsFalse()
    {
        // Arrange
        float[] original = { 1.0f, 2.0f, 3.0f };
        float[] encrypted = _sut.EncryptPerIndex(original, "pt-1");

        // Act
        bool valid = _sut.VerifyVectorHmac(encrypted, "pt-1", "invalid-hmac");

        // Assert
        valid.Should().BeFalse();
    }

    [Fact]
    public void ExportPublicKeyBase64_ReturnsNonEmptyString()
    {
        // Act
        string pubKey = _sut.ExportPublicKeyBase64();

        // Assert
        pubKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExportPrivateKeyBase64_ReturnsNonEmptyString()
    {
        // Act
        string privKey = _sut.ExportPrivateKeyBase64();

        // Assert
        privKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_FromPrivateKey_CanDecryptPreviouslyEncrypted()
    {
        // Arrange
        float[] original = { 1.0f, 2.0f, 3.0f };
        string privateKeyBase64 = _sut.ExportPrivateKeyBase64();
        byte[] encrypted = _sut.Encrypt(original);

        // Act
        using var restored = new EcVectorCrypto(privateKeyBase64);
        float[] decrypted = restored.Decrypt(encrypted);

        // Assert
        decrypted.Should().Equal(original);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
