// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

/// <summary>
/// Tests for <see cref="ErrorPattern"/>.
/// </summary>
[Trait("Category", "Unit")]
public class ErrorPatternTests
{
    // ----------------------------------------------------------------
    // Record properties
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var examples = new List<FailedEpisode>();

        // Act
        var pattern = new ErrorPattern("NullRef pattern", 5, examples, "Add null check");

        // Assert
        pattern.Description.Should().Be("NullRef pattern");
        pattern.Frequency.Should().Be(5);
        pattern.Examples.Should().BeSameAs(examples);
        pattern.SuggestedFix.Should().Be("Add null check");
    }

    [Fact]
    public void Constructor_NullSuggestedFix_IsAllowed()
    {
        // Act
        var pattern = new ErrorPattern("Error", 1, new List<FailedEpisode>(), null);

        // Assert
        pattern.SuggestedFix.Should().BeNull();
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var examples = new List<FailedEpisode>();

        // Act
        var pattern1 = new ErrorPattern("Error", 3, examples, "Fix it");
        var pattern2 = new ErrorPattern("Error", 3, examples, "Fix it");

        // Assert
        pattern1.Should().Be(pattern2);
    }

    // ----------------------------------------------------------------
    // SeverityScore
    // ----------------------------------------------------------------

    [Fact]
    public void SeverityScore_NoExamples_ReturnsZero()
    {
        // Arrange
        var pattern = new ErrorPattern("Empty", 5, new List<FailedEpisode>(), null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        score.Should().Be(0.0);
    }

    [Fact]
    public void SeverityScore_RecentExamples_HigherRecencyComponent()
    {
        // Arrange - very recent example
        var recentExamples = new List<FailedEpisode>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "Trace"),
        };
        var pattern = new ErrorPattern("Recent", 5, recentExamples, null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        score.Should().BeGreaterThan(0.0);
        score.Should().BeLessOrEqualTo(1.0);
    }

    [Fact]
    public void SeverityScore_OldExamples_LowerRecencyComponent()
    {
        // Arrange - 60 days ago (beyond 30-day recency window)
        var oldExamples = new List<FailedEpisode>
        {
            new(Guid.NewGuid(), DateTime.UtcNow.AddDays(-60), "Goal", "Reason", "Trace"),
        };
        var pattern = new ErrorPattern("Old", 5, oldExamples, null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        // Recency score should be 0 (clamped), so only frequency contributes
        // frequency = min(1, 5/10) = 0.5, recency = max(0, 1 - 60/30) = 0
        // score = 0.5 * 0.7 + 0 * 0.3 = 0.35
        score.Should().BeApproximately(0.35, 0.01);
    }

    [Fact]
    public void SeverityScore_HighFrequency_CapsAtOne()
    {
        // Arrange - frequency >= 10 caps frequencyScore at 1.0
        var examples = new List<FailedEpisode>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "Trace"),
        };
        var pattern = new ErrorPattern("Frequent", 20, examples, null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        // frequencyScore = min(1, 20/10) = 1.0
        // recencyScore close to 1.0 (very recent)
        // score ~ 1.0 * 0.7 + 1.0 * 0.3 = ~1.0
        score.Should().BeGreaterThan(0.9);
    }

    [Fact]
    public void SeverityScore_LowFrequency_ProportionalScore()
    {
        // Arrange
        var examples = new List<FailedEpisode>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "Trace"),
        };
        var pattern = new ErrorPattern("Low", 1, examples, null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        // frequencyScore = min(1, 1/10) = 0.1
        // score should be lower than high frequency
        score.Should().BeLessThan(0.5);
    }

    [Fact]
    public void SeverityScore_MultipleExamples_AveragesRecency()
    {
        // Arrange - mix of old and new
        var examples = new List<FailedEpisode>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, "Goal", "Reason", "Trace"),
            new(Guid.NewGuid(), DateTime.UtcNow.AddDays(-30), "Goal2", "Reason2", "Trace2"),
        };
        var pattern = new ErrorPattern("Mixed", 5, examples, null);

        // Act
        double score = pattern.SeverityScore;

        // Assert
        score.Should().BeGreaterThan(0.0);
        score.Should().BeLessOrEqualTo(1.0);
    }
}
