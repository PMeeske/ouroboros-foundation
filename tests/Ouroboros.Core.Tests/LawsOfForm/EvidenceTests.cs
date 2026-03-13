using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class EvidenceTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var evidence = new Evidence("safety_check", LoF.Mark, "All safety checks passed");

        // Assert
        evidence.CriterionName.Should().Be("safety_check");
        evidence.Evaluation.Should().Be(LoF.Mark);
        evidence.Description.Should().Be("All safety checks passed");
    }

    [Fact]
    public void Constructor_WithoutTimestamp_SetsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var evidence = new Evidence("test", LoF.Mark, "desc");

        // Assert
        evidence.Timestamp.Should().BeOnOrAfter(before);
        evidence.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithTimestamp_UsesProvided()
    {
        // Arrange
        var specificTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var evidence = new Evidence("test", LoF.Void, "desc", specificTime);

        // Assert
        evidence.Timestamp.Should().Be(specificTime);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var time = DateTime.UtcNow;
        var a = new Evidence("criterion", LoF.Mark, "desc", time);
        var b = new Evidence("criterion", LoF.Mark, "desc", time);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentEvaluation_AreNotEqual()
    {
        // Arrange
        var time = DateTime.UtcNow;
        var a = new Evidence("criterion", LoF.Mark, "desc", time);
        var b = new Evidence("criterion", LoF.Void, "desc", time);

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Evidence("criterion", LoF.Mark, "original desc");

        // Act
        var modified = original with { Description = "updated desc" };

        // Assert
        modified.CriterionName.Should().Be("criterion");
        modified.Description.Should().Be("updated desc");
        original.Description.Should().Be("original desc");
    }
}
