using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ClaimTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var sut = new Claim("The sky is blue", 0.9, "gpt-4");

        sut.Statement.Should().Be("The sky is blue");
        sut.Confidence.Should().Be(0.9);
        sut.Source.Should().Be("gpt-4");
    }

    [Fact]
    public void Constructor_ClampsConfidenceAboveOne()
    {
        var sut = new Claim("test", 1.5, "source");

        sut.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_ClampsConfidenceBelowZero()
    {
        var sut = new Claim("test", -0.5, "source");

        sut.Confidence.Should().Be(0.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructor_ValidConfidence_NotClamped(double confidence)
    {
        var sut = new Claim("test", confidence, "source");

        sut.Confidence.Should().Be(confidence);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new Claim("test", 0.8, "source");
        var b = new Claim("test", 0.8, "source");

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new Claim("test", 0.8, "source");
        var b = new Claim("test", 0.9, "source");

        a.Should().NotBe(b);
    }
}
