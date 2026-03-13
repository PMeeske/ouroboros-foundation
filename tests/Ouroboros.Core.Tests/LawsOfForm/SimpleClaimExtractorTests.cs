using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class SimpleClaimExtractorTests
{
    private readonly SimpleClaimExtractor _sut = new();

    [Fact]
    public void ExtractClaims_NullText_ReturnsEmpty()
    {
        _sut.ExtractClaims(null!, "source").Should().BeEmpty();
    }

    [Fact]
    public void ExtractClaims_EmptyText_ReturnsEmpty()
    {
        _sut.ExtractClaims("", "source").Should().BeEmpty();
    }

    [Fact]
    public void ExtractClaims_WhitespaceText_ReturnsEmpty()
    {
        _sut.ExtractClaims("   ", "source").Should().BeEmpty();
    }

    [Fact]
    public void ExtractClaims_SingleLongSentence_ReturnsOneClaim()
    {
        var claims = _sut.ExtractClaims("This is a long enough sentence to be extracted.", "test-model");

        claims.Should().HaveCount(1);
        claims[0].Statement.Should().Be("This is a long enough sentence to be extracted");
        claims[0].Source.Should().Be("test-model");
        claims[0].Confidence.Should().Be(0.8);
    }

    [Fact]
    public void ExtractClaims_MultipleSentences_ReturnsMultipleClaims()
    {
        var text = "The Earth orbits the Sun. Water freezes at zero degrees.";
        var claims = _sut.ExtractClaims(text, "source");

        claims.Should().HaveCount(2);
    }

    [Fact]
    public void ExtractClaims_ShortFragments_AreFiltered()
    {
        var text = "Hi. Ok. This is a long enough sentence.";
        var claims = _sut.ExtractClaims(text, "source");

        // "Hi" and "Ok" are <= 10 chars, only the long sentence remains
        claims.Should().HaveCount(1);
    }

    [Fact]
    public void ExtractClaims_ExclamationAndQuestion_AreHandled()
    {
        var text = "The temperature is rising fast! Will it rain tomorrow? Forecasts say it probably will.";
        var claims = _sut.ExtractClaims(text, "source");

        claims.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
