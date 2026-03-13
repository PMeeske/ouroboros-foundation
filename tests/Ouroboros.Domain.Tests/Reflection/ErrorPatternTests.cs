using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class ErrorPatternTests
{
    private static FailedEpisode CreateFailedEpisode(DateTime? timestamp = null)
    {
        return new FailedEpisode(
            Guid.NewGuid(),
            timestamp ?? DateTime.UtcNow,
            "goal",
            "reason",
            "trace",
            new Dictionary<string, object>());
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var examples = new List<FailedEpisode> { CreateFailedEpisode() };

        var pattern = new ErrorPattern("Null ref", 5, examples, "Add null check");

        pattern.Description.Should().Be("Null ref");
        pattern.Frequency.Should().Be(5);
        pattern.Examples.Should().HaveCount(1);
        pattern.SuggestedFix.Should().Be("Add null check");
    }

    [Fact]
    public void Constructor_NullSuggestedFix_ShouldBeNull()
    {
        var pattern = new ErrorPattern("Error", 1, new List<FailedEpisode>(), null);

        pattern.SuggestedFix.Should().BeNull();
    }

    [Fact]
    public void SeverityScore_NoExamples_ShouldBeZero()
    {
        var pattern = new ErrorPattern("Error", 5, new List<FailedEpisode>(), null);

        pattern.SeverityScore.Should().Be(0.0);
    }

    [Fact]
    public void SeverityScore_HighFrequencyRecentErrors_ShouldBeHigh()
    {
        var recentExamples = new List<FailedEpisode>
        {
            CreateFailedEpisode(DateTime.UtcNow),
            CreateFailedEpisode(DateTime.UtcNow.AddMinutes(-10)),
        };

        var pattern = new ErrorPattern("Critical", 10, recentExamples, null);

        pattern.SeverityScore.Should().BeGreaterThanOrEqualTo(0.5);
    }

    [Fact]
    public void SeverityScore_LowFrequencyOldErrors_ShouldBeLow()
    {
        var oldExamples = new List<FailedEpisode>
        {
            CreateFailedEpisode(DateTime.UtcNow.AddDays(-60)),
        };

        var pattern = new ErrorPattern("Old error", 1, oldExamples, null);

        pattern.SeverityScore.Should().BeLessThan(0.5);
    }

    [Fact]
    public void SeverityScore_FrequencyCappedAtTen_ShouldNotExceedOne()
    {
        var examples = new List<FailedEpisode> { CreateFailedEpisode() };

        var pattern = new ErrorPattern("Frequent", 20, examples, null);

        pattern.SeverityScore.Should().BeLessThanOrEqualTo(1.0);
    }
}
