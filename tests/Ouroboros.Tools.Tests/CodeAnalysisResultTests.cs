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
            new[] { "MyClass" },
            new[] { "MyMethod" },
            Array.Empty<Microsoft.CodeAnalysis.Diagnostic>(),
            new[] { "Finding 1" });

        // Assert
        result.Classes.Should().ContainSingle().Which.Should().Be("MyClass");
        result.Methods.Should().ContainSingle().Which.Should().Be("MyMethod");
        result.Diagnostics.Should().BeEmpty();
        result.Findings.Should().ContainSingle();
    }

    [Fact]
    public void IsValid_NoDiagnosticErrors_ReturnsTrue()
    {
        // Act
        var result = new CodeAnalysisResult(
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<Microsoft.CodeAnalysis.Diagnostic>());

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NullFindings_DefaultsToEmpty()
    {
        // Act
        var result = new CodeAnalysisResult(
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<Microsoft.CodeAnalysis.Diagnostic>());

        // Assert
        result.Findings.Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class DslSuggestionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var suggestion = new DslSuggestion("UseDraft", "Good starting point", 0.85);

        // Assert
        suggestion.Step.Should().Be("UseDraft");
        suggestion.Explanation.Should().Be("Good starting point");
        suggestion.Confidence.Should().Be(0.85);
    }

    [Fact]
    public void Constructor_ZeroConfidence()
    {
        // Act
        var suggestion = new DslSuggestion("Step", "Reason", 0.0);

        // Assert
        suggestion.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void Constructor_FullConfidence()
    {
        // Act
        var suggestion = new DslSuggestion("Step", "Reason", 1.0);

        // Assert
        suggestion.Confidence.Should().Be(1.0);
    }
}
