namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public class ImprovementSuggestionTests
{
    [Fact]
    public void Priority_HighImpact_ReturnsHigh()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Memory", "Use caching", 0.8, "Add Redis cache");

        // Assert
        suggestion.Priority.Should().Be("High");
    }

    [Fact]
    public void Priority_ExactlyPointSeven_ReturnsHigh()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Memory", "Use caching", 0.7, "Add Redis cache");

        // Assert
        suggestion.Priority.Should().Be("High");
    }

    [Fact]
    public void Priority_MediumImpact_ReturnsMedium()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Performance", "Optimize queries", 0.5, "Add indexes");

        // Assert
        suggestion.Priority.Should().Be("Medium");
    }

    [Fact]
    public void Priority_ExactlyPointFour_ReturnsMedium()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Performance", "Optimize", 0.4, "Refactor");

        // Assert
        suggestion.Priority.Should().Be("Medium");
    }

    [Fact]
    public void Priority_LowImpact_ReturnsLow()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Naming", "Improve variable names", 0.2, "Rename variables");

        // Assert
        suggestion.Priority.Should().Be("Low");
    }

    [Fact]
    public void Priority_ZeroImpact_ReturnsLow()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Style", "Fix formatting", 0.0, "Apply formatter");

        // Assert
        suggestion.Priority.Should().Be("Low");
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var suggestion = new ImprovementSuggestion(
            "Security", "Add input validation", 0.85, "Validate at boundaries");

        // Assert
        suggestion.Area.Should().Be("Security");
        suggestion.Suggestion.Should().Be("Add input validation");
        suggestion.ExpectedImpact.Should().Be(0.85);
        suggestion.Implementation.Should().Be("Validate at boundaries");
    }
}
