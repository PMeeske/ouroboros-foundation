// <copyright file="ContradictionDetectorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for ContradictionDetector which uses Laws of Form re-entry pattern
/// to detect contradictions in LLM responses.
/// </summary>
[Trait("Category", "Unit")]
public class ContradictionDetectorTests
{
    private readonly ContradictionDetector _detector;
    private readonly Mock<IClaimExtractor> _mockExtractor;

    public ContradictionDetectorTests()
    {
        _mockExtractor = new Mock<IClaimExtractor>();
        _detector = new ContradictionDetector(_mockExtractor.Object, similarityThreshold: 0.5);
    }

    // --- Analyze single response ---

    [Fact]
    public void Analyze_NullResponse_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _detector.Analyze(null!));
    }

    [Fact]
    public void Analyze_SingleClaim_ReturnsVoid()
    {
        // Arrange
        var response = new LlmResponse("The sky is blue.", 0.9, modelName: "test");
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim> { new("The sky is blue", 0.9, "test") });

        // Act
        var result = _detector.Analyze(response);

        // Assert
        result.IsVoid().Should().BeTrue("not enough claims to detect contradictions");
    }

    [Fact]
    public void Analyze_NoClaims_ReturnsVoid()
    {
        // Arrange
        var response = new LlmResponse("Short.", 0.9, modelName: "test");
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim>());

        // Act
        var result = _detector.Analyze(response);

        // Assert
        result.IsVoid().Should().BeTrue();
    }

    [Fact]
    public void Analyze_ConsistentClaims_ReturnsMark()
    {
        // Arrange: two similar, non-contradictory claims
        var response = new LlmResponse("Test text.", 0.9, modelName: "test");
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim>
            {
                new("The weather is sunny and warm today", 0.9, "test"),
                new("Today the weather is warm and sunny", 0.9, "test")
            });

        // Act
        var result = _detector.Analyze(response);

        // Assert
        result.IsMark().Should().BeTrue("consistent claims yield Mark");
    }

    [Fact]
    public void Analyze_ContradictoryClaims_ReturnsImaginary()
    {
        // Arrange: two contradictory claims about the same topic
        var response = new LlmResponse("Contradictory text.", 0.9, modelName: "test");
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim>
            {
                new("The system is running normally and working", 0.9, "test"),
                new("The system is not running normally and working", 0.9, "test")
            });

        // Act
        var result = _detector.Analyze(response);

        // Assert
        result.IsImaginary().Should().BeTrue("contradictory claims yield Imaginary (re-entry pattern)");
    }

    [Fact]
    public void Analyze_UnrelatedClaims_ReturnsMark()
    {
        // Arrange: claims about different topics
        var response = new LlmResponse("Multi-topic text.", 0.9, modelName: "test");
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim>
            {
                new("Python is a programming language", 0.9, "test"),
                new("The Pacific ocean is very deep", 0.9, "test")
            });

        // Act
        var result = _detector.Analyze(response);

        // Assert - low similarity -> Void from CheckPair, so overall Mark (no contradictions found)
        result.IsMark().Should().BeTrue();
    }

    // --- AnalyzeMultiple ---

    [Fact]
    public void AnalyzeMultiple_SingleResponse_ReturnsVoid()
    {
        var responses = new[] { new LlmResponse("Test.", 0.9, modelName: "test") };
        _mockExtractor
            .Setup(e => e.ExtractClaims(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<Claim>());

        var result = _detector.AnalyzeMultiple(responses);
        result.IsVoid().Should().BeTrue();
    }

    [Fact]
    public void AnalyzeMultiple_ConsistentResponses_ReturnsMark()
    {
        // Arrange
        var responses = new[]
        {
            new LlmResponse("Response A.", 0.9, modelName: "modelA"),
            new LlmResponse("Response B.", 0.9, modelName: "modelB")
        };

        _mockExtractor
            .Setup(e => e.ExtractClaims("Response A.", "modelA"))
            .Returns(new List<Claim> { new("The data shows growth in revenue", 0.9, "modelA") });
        _mockExtractor
            .Setup(e => e.ExtractClaims("Response B.", "modelB"))
            .Returns(new List<Claim> { new("Revenue data shows growth patterns", 0.9, "modelB") });

        // Act
        var result = _detector.AnalyzeMultiple(responses);

        // Assert
        result.IsMark().Should().BeTrue();
    }

    // --- CheckPair ---

    [Fact]
    public void CheckPair_NullClaim1_ThrowsArgumentNullException()
    {
        var claim = new Claim("test", 0.9, "src");
        Assert.Throws<ArgumentNullException>(() => _detector.CheckPair(null!, claim));
    }

    [Fact]
    public void CheckPair_NullClaim2_ThrowsArgumentNullException()
    {
        var claim = new Claim("test", 0.9, "src");
        Assert.Throws<ArgumentNullException>(() => _detector.CheckPair(claim, null!));
    }

    [Fact]
    public void CheckPair_UnrelatedClaims_ReturnsVoid()
    {
        var claim1 = new Claim("Cats are mammals", 0.9, "src");
        var claim2 = new Claim("Jupiter is a planet", 0.9, "src");

        var result = _detector.CheckPair(claim1, claim2);
        result.IsVoid().Should().BeTrue("unrelated claims have low similarity");
    }

    [Fact]
    public void CheckPair_LowConfidenceClaims_ReturnsVoid()
    {
        // Even if similar, low confidence means not enough certainty to detect contradiction
        var claim1 = new Claim("The system is running normally", 0.2, "src");
        var claim2 = new Claim("The system is not running normally", 0.2, "src");

        var result = _detector.CheckPair(claim1, claim2);
        result.IsVoid().Should().BeTrue("low confidence -> no firm contradiction");
    }

    // --- Constructor ---

    [Fact]
    public void Constructor_NullExtractor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ContradictionDetector(null!));
    }

    // --- SimpleClaimExtractor integration ---

    [Fact]
    public void SimpleClaimExtractor_ExtractsClaims()
    {
        var extractor = new SimpleClaimExtractor();
        var claims = extractor.ExtractClaims(
            "The system is healthy. Performance metrics are normal. All checks passed.",
            "test-model");

        claims.Should().HaveCountGreaterThanOrEqualTo(2);
        claims.Should().AllSatisfy(c => c.Source.Should().Be("test-model"));
    }

    [Fact]
    public void SimpleClaimExtractor_EmptyText_ReturnsEmpty()
    {
        var extractor = new SimpleClaimExtractor();
        var claims = extractor.ExtractClaims("", "source");
        claims.Should().BeEmpty();
    }

    [Fact]
    public void SimpleClaimExtractor_WhitespaceText_ReturnsEmpty()
    {
        var extractor = new SimpleClaimExtractor();
        var claims = extractor.ExtractClaims("   ", "source");
        claims.Should().BeEmpty();
    }
}
