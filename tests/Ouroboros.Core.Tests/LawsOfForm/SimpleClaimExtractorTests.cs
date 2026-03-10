// <copyright file="SimpleClaimExtractorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="SimpleClaimExtractor"/> which extracts claims from text
/// by splitting into sentences.
/// </summary>
[Trait("Category", "Unit")]
public class SimpleClaimExtractorTests
{
    private readonly SimpleClaimExtractor extractor = new();

    // ──────────── Null/Empty input ────────────

    [Fact]
    public void ExtractClaims_NullText_ReturnsEmpty()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(null!, "source");

        claims.Should().BeEmpty();
    }

    [Fact]
    public void ExtractClaims_EmptyText_ReturnsEmpty()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(string.Empty, "source");

        claims.Should().BeEmpty();
    }

    [Fact]
    public void ExtractClaims_WhitespaceText_ReturnsEmpty()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims("   ", "source");

        claims.Should().BeEmpty();
    }

    // ──────────── Basic sentence splitting ────────────

    [Fact]
    public void ExtractClaims_SingleSentence_ReturnsOneClaim()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "The quick brown fox jumps over the lazy dog.",
            "test-source");

        claims.Should().HaveCount(1);
        claims[0].Statement.Should().Be("The quick brown fox jumps over the lazy dog");
        claims[0].Source.Should().Be("test-source");
    }

    [Fact]
    public void ExtractClaims_MultipleSentences_ReturnsMultipleClaims()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "The Earth revolves around the Sun. Water boils at 100 degrees Celsius.",
            "science");

        claims.Should().HaveCount(2);
    }

    [Fact]
    public void ExtractClaims_SplitsOnPeriods()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "First sentence here. Second sentence here.",
            "source");

        claims.Should().HaveCount(2);
    }

    [Fact]
    public void ExtractClaims_SplitsOnExclamationMarks()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "This is amazing! What a wonderful discovery!",
            "source");

        claims.Should().HaveCount(2);
    }

    [Fact]
    public void ExtractClaims_SplitsOnQuestionMarks()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "Did you know about this? The answer is surprising.",
            "source");

        claims.Should().HaveCount(2);
    }

    // ──────────── Short fragment filtering ────────────

    [Fact]
    public void ExtractClaims_ShortFragments_AreFiltered()
    {
        // Fragments 10 chars or less are filtered out
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "Short. But this one is long enough to keep.",
            "source");

        claims.Should().HaveCount(1);
        claims[0].Statement.Should().Contain("long enough to keep");
    }

    [Fact]
    public void ExtractClaims_AllShortFragments_ReturnsEmpty()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "Hi. Bye. OK.",
            "source");

        claims.Should().BeEmpty();
    }

    // ──────────── Default confidence ────────────

    [Fact]
    public void ExtractClaims_DefaultConfidence_Is0Point8()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "This sentence has a default confidence score.",
            "source");

        claims.Should().HaveCount(1);
        claims[0].Confidence.Should().Be(0.8);
    }

    // ──────────── Source attribution ────────────

    [Fact]
    public void ExtractClaims_PreservesSourceAttribution()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "A claim from a specific model source.",
            "gpt-4");

        claims[0].Source.Should().Be("gpt-4");
    }

    // ──────────── Whitespace trimming ────────────

    [Fact]
    public void ExtractClaims_TrimsSentenceWhitespace()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "  This has leading spaces.   And trailing spaces too  .",
            "source");

        foreach (var claim in claims)
        {
            claim.Statement.Should().NotStartWith(" ");
            claim.Statement.Should().NotEndWith(" ");
        }
    }

    // ──────────── Interface implementation ────────────

    [Fact]
    public void ImplementsIClaimExtractor()
    {
        IClaimExtractor iface = this.extractor;

        iface.Should().NotBeNull();
    }

    // ──────────── Mixed delimiters ────────────

    [Fact]
    public void ExtractClaims_MixedDelimiters_SplitsCorrectly()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "Statement one is here. Is this a question? Wow this is exciting!",
            "source");

        claims.Should().HaveCount(3);
    }

    // ──────────── Consecutive delimiters ────────────

    [Fact]
    public void ExtractClaims_ConsecutiveDelimiters_HandledGracefully()
    {
        IReadOnlyList<Claim> claims = this.extractor.ExtractClaims(
            "A real sentence here... Another one follows.",
            "source");

        // Empty entries from "..." are removed by RemoveEmptyEntries
        claims.Should().AllSatisfy(c => c.Statement.Length.Should().BeGreaterThan(0));
    }
}
