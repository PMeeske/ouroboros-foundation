using Ouroboros.Core.Randomness;

namespace Ouroboros.Tests.Randomness;

[Trait("Category", "Unit")]
public sealed class SeededRandomProviderTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var sut1 = new SeededRandomProvider(42);
        var sut2 = new SeededRandomProvider(42);

        for (int i = 0; i < 10; i++)
        {
            sut1.Next(100).Should().Be(sut2.Next(100));
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var sut1 = new SeededRandomProvider(42);
        var sut2 = new SeededRandomProvider(99);

        var seq1 = Enumerable.Range(0, 10).Select(_ => sut1.Next(1000)).ToList();
        var seq2 = Enumerable.Range(0, 10).Select(_ => sut2.Next(1000)).ToList();

        seq1.Should().NotBeEquivalentTo(seq2);
    }

    [Fact]
    public void Next_ReturnsWithinRange()
    {
        var sut = new SeededRandomProvider(42);

        for (int i = 0; i < 100; i++)
        {
            var value = sut.Next(10);
            value.Should().BeGreaterThanOrEqualTo(0);
            value.Should().BeLessThan(10);
        }
    }

    [Fact]
    public void Next_MinMax_ReturnsWithinRange()
    {
        var sut = new SeededRandomProvider(42);

        for (int i = 0; i < 100; i++)
        {
            var value = sut.Next(5, 15);
            value.Should().BeGreaterThanOrEqualTo(5);
            value.Should().BeLessThan(15);
        }
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
    public void NextDouble_SameSeed_Deterministic()
    {
        var sut1 = new SeededRandomProvider(42);
        var sut2 = new SeededRandomProvider(42);

        for (int i = 0; i < 10; i++)
        {
            sut1.NextDouble().Should().Be(sut2.NextDouble());
        }
    }

    [Fact]
    public void NextBytes_FillsBuffer()
    {
        var sut = new SeededRandomProvider(42);
        var buffer = new byte[16];

        sut.NextBytes(buffer);

        buffer.Should().Contain(b => b != 0);
    }

    [Fact]
    public void NextBytes_SameSeed_Deterministic()
    {
        var sut1 = new SeededRandomProvider(42);
        var sut2 = new SeededRandomProvider(42);
        var buf1 = new byte[16];
        var buf2 = new byte[16];

        sut1.NextBytes(buf1);
        sut2.NextBytes(buf2);

        buf1.Should().Equal(buf2);
    }

    [Fact]
    public void DefaultConstructor_DoesNotThrow()
    {
        Action act = () => _ = new SeededRandomProvider();

        act.Should().NotThrow();
    }
}
