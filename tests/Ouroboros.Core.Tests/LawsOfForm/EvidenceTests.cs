using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class EvidenceTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var timestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var sut = new Evidence("safety-check", Form.Mark, "Passed safety check", timestamp);

        sut.CriterionName.Should().Be("safety-check");
        sut.Evaluation.Should().Be(Form.Mark);
        sut.Description.Should().Be("Passed safety check");
        sut.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Constructor_WithoutTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = new Evidence("criterion", Form.Void, "description");
        var after = DateTime.UtcNow;

        sut.Timestamp.Should().BeOnOrAfter(before);
        sut.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_WithVoidEvaluation_SetsCorrectly()
    {
        var sut = new Evidence("test", Form.Void, "failed check");

        sut.Evaluation.Should().Be(Form.Void);
    }

    [Fact]
    public void Constructor_WithImaginaryEvaluation_SetsCorrectly()
    {
        var sut = new Evidence("test", Form.Imaginary, "uncertain result");

        sut.Evaluation.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new Evidence("c", Form.Mark, "d", ts);
        var b = new Evidence("c", Form.Mark, "d", ts);

        a.Should().Be(b);
    }
}
