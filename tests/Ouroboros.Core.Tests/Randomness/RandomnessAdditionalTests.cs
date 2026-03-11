using Ouroboros.Core.Randomness;
using Ouroboros.Providers.Random;

namespace Ouroboros.Core.Tests.Randomness;

/// <summary>
/// Additional tests for CryptoRandomProvider and SeededRandomProvider.
/// </summary>
[Trait("Category", "Unit")]
public class CryptoRandomProviderAdditionalTests
{
    [Fact]
    public void Instance_IsSingleton()
    {
        CryptoRandomProvider.Instance.Should().BeSameAs(CryptoRandomProvider.Instance);
    }

    [Fact]
    public void ImplementsIRandomProvider()
    {
        CryptoRandomProvider.Instance.Should().BeAssignableTo<IRandomProvider>();
    }

    [Fact]
    public void Next_MaxGreaterThanMin_ReturnsWithinRange()
    {
        var sut = CryptoRandomProvider.Instance;
        var value = sut.Next(10, 20);
        value.Should().BeGreaterThanOrEqualTo(10);
        value.Should().BeLessThan(20);
    }

    [Fact]
    public void Next_MaxLessThanMin_ThrowsArgumentOutOfRange()
    {
        var sut = CryptoRandomProvider.Instance;
        var act = () => sut.Next(20, 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Next_MinEqualsMax_ReturnsMin()
    {
        var sut = CryptoRandomProvider.Instance;
        var value = sut.Next(5, 5);
        value.Should().Be(5);
    }

    [Fact]
    public void NextDouble_ReturnsInZeroToOneRange()
    {
        var sut = CryptoRandomProvider.Instance;
        for (int i = 0; i < 100; i++)
        {
            var value = sut.NextDouble();
            value.Should().BeGreaterThanOrEqualTo(0.0);
            value.Should().BeLessThan(1.0);
        }
    }

    [Fact]
    public void NextBytes_FillsBuffer()
    {
        var sut = CryptoRandomProvider.Instance;
        var buffer = new byte[16];
        sut.NextBytes(buffer);

        // Very unlikely all bytes are zero
        buffer.Any(b => b != 0).Should().BeTrue();
    }

    [Fact]
    public void NextBytes_NullBuffer_ThrowsArgumentNullException()
    {
        var sut = CryptoRandomProvider.Instance;
        var act = () => sut.NextBytes(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

[Trait("Category", "Unit")]
public class SeededRandomProviderAdditionalTests
{
    [Fact]
    public void ImplementsIRandomProvider()
    {
        var sut = new SeededRandomProvider(42);
        sut.Should().BeAssignableTo<IRandomProvider>();
    }

    [Fact]
    public void DefaultConstructor_DoesNotThrow()
    {
        var act = () => new SeededRandomProvider();
        act.Should().NotThrow();
    }

    [Fact]
    public void NextDouble_ReturnsInUnitInterval()
    {
        var sut = new SeededRandomProvider(42);
        for (int i = 0; i < 100; i++)
        {
            var value = sut.NextDouble();
            value.Should().BeGreaterThanOrEqualTo(0.0);
            value.Should().BeLessThan(1.0);
        }
    }

    [Fact]
    public void NextBytes_FillsBuffer()
    {
        var sut = new SeededRandomProvider(42);
        var buffer = new byte[16];
        sut.NextBytes(buffer);
        buffer.Any(b => b != 0).Should().BeTrue();
    }

    [Fact]
    public void Next_WithMinMax_ReturnsWithinRange()
    {
        var sut = new SeededRandomProvider(42);
        for (int i = 0; i < 100; i++)
        {
            var value = sut.Next(5, 15);
            value.Should().BeGreaterThanOrEqualTo(5);
            value.Should().BeLessThan(15);
        }
    }
}
