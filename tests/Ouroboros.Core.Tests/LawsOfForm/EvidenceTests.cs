// <copyright file="EvidenceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="Evidence"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class EvidenceTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var timestamp = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var evidence = new Evidence("safety_check", Form.Mark, "All safe", timestamp);

        evidence.CriterionName.Should().Be("safety_check");
        evidence.Evaluation.Should().Be(Form.Mark);
        evidence.Description.Should().Be("All safe");
        evidence.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Constructor_DefaultTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var evidence = new Evidence("test", Form.Void, "desc");
        var after = DateTime.UtcNow;

        evidence.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_WithImaginaryEvaluation_SetsCorrectly()
    {
        var evidence = new Evidence("uncertain_check", Form.Imaginary, "Needs review");

        evidence.Evaluation.Should().Be(Form.Imaginary);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = DateTime.UtcNow;
        var e1 = new Evidence("test", Form.Mark, "desc", ts);
        var e2 = new Evidence("test", Form.Mark, "desc", ts);

        e1.Should().Be(e2);
    }

    [Fact]
    public void RecordEquality_DifferentCriterionName_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var e1 = new Evidence("test1", Form.Mark, "desc", ts);
        var e2 = new Evidence("test2", Form.Mark, "desc", ts);

        e1.Should().NotBe(e2);
    }

    [Fact]
    public void RecordEquality_DifferentEvaluation_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var e1 = new Evidence("test", Form.Mark, "desc", ts);
        var e2 = new Evidence("test", Form.Void, "desc", ts);

        e1.Should().NotBe(e2);
    }
}
