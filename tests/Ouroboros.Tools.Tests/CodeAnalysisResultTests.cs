namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class CodeAnalysisResultTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var result = new CodeAnalysisResult(
            "TestCode",
            new[] { "Found issue 1" },
            new[] { "Use var instead" },
            true);

        // Assert
        result.Code.Should().Be("TestCode");
        result.Issues.Should().ContainSingle();
        result.Suggestions.Should().ContainSingle();
        result.IsValid.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class DslSuggestionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var suggestion = new DslSuggestion("Add error handling", "try-catch");

        // Assert
        suggestion.Description.Should().Be("Add error handling");
        suggestion.Code.Should().Be("try-catch");
    }
}
