using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class ImprovementSuggestionTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var suggestion = new ImprovementSuggestion("Reasoning", "Add chain-of-thought", 0.8, "Implement CoT prompts");

        suggestion.Area.Should().Be("Reasoning");
        suggestion.Suggestion.Should().Be("Add chain-of-thought");
        suggestion.ExpectedImpact.Should().Be(0.8);
        suggestion.Implementation.Should().Be("Implement CoT prompts");
    }

    [Theory]
    [InlineData(0.7, "High")]
    [InlineData(0.8, "High")]
    [InlineData(1.0, "High")]
    public void Priority_HighImpact_ShouldReturnHigh(double impact, string expected)
    {
        var suggestion = new ImprovementSuggestion("Area", "Suggestion", impact, "Impl");

        suggestion.Priority.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.4, "Medium")]
    [InlineData(0.5, "Medium")]
    [InlineData(0.69, "Medium")]
    public void Priority_MediumImpact_ShouldReturnMedium(double impact, string expected)
    {
        var suggestion = new ImprovementSuggestion("Area", "Suggestion", impact, "Impl");

        suggestion.Priority.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.0, "Low")]
    [InlineData(0.1, "Low")]
    [InlineData(0.39, "Low")]
    public void Priority_LowImpact_ShouldReturnLow(double impact, string expected)
    {
        var suggestion = new ImprovementSuggestion("Area", "Suggestion", impact, "Impl");

        suggestion.Priority.Should().Be(expected);
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var s1 = new ImprovementSuggestion("A", "B", 0.5, "C");
        var s2 = new ImprovementSuggestion("A", "B", 0.5, "C");

        s1.Should().Be(s2);
    }
}
