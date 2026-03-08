using Ouroboros.Core.Randomness;

namespace Ouroboros.Tests.Randomness;

[Trait("Category", "Unit")]
public sealed class CryptoRandomProviderTests
{
    [Fact]
    public void Instance_IsNotNull()
    {
        CryptoRandomProvider.Instance.Should().NotBeNull();
    }

    [Fact]
    public void Next_ReturnsWithinRange()
    {
        var sut = CryptoRandomProvider.Instance;

        var value = sut.Next(10);

        value.Should().BeGreaterThanOrEqualTo(0);
        value.Should().BeLessThan(10);
    }

    [Fact]
    public void Next_ZeroMax_ThrowsArgument()
    {
        var sut = CryptoRandomProvider.Instance;

        Action act = () => sut.Next(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Next_NegativeMax_ThrowsArgument()
    {
        var sut = CryptoRandomProvider.Instance;

        Action act = () => sut.Next(-5);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Next_MinMax_ReturnsWithinRange()
    {
        var sut = CryptoRandomProvider.Instance;

        var value = sut.Next(5, 10);

        value.Should().BeGreaterThanOrEqualTo(5);
        value.Should().BeLessThan(10);
    }

    [Fact]
    public void Next_MinEqualsMax_ReturnsMin()
    {
        var sut = CryptoRandomProvider.Instance;

        var value = sut.Next(5, 5);

        value.Should().Be(5);
    }

    [Fact]
    public void Next_MaxLessThanMin_ThrowsArgument()
    {
        var sut = CryptoRandomProvider.Instance;

        Action act = () => sut.Next(10, 5);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NextDouble_ReturnsInUnitInterval()
    {
        var sut = CryptoRandomProvider.Instance;

        var value = sut.NextDouble();

        value.Should().BeGreaterThanOrEqualTo(0.0);
        value.Should().BeLessThan(1.0);
    }

    [Fact]
    public void NextDouble_ProducesVariation()
    {
        var sut = CryptoRandomProvider.Instance;
        var values = Enumerable.Range(0, 10).Select(_ => sut.NextDouble()).ToList();

        values.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void NextBytes_FillsBuffer()
    {
        var sut = CryptoRandomProvider.Instance;
        var buffer = new byte[16];

        sut.NextBytes(buffer);

        buffer.Should().Contain(b => b != 0);
    }

    [Fact]
    public void NextBytes_NullBuffer_ThrowsArgument()
    {
        var sut = CryptoRandomProvider.Instance;

        Action act = () => sut.NextBytes(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
