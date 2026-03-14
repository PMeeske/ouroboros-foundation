using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Tests for FixChainArrows covering fix chain creation and configuration.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixChainArrowsTests
{
    private static (Document Document, SyntaxNode Root, Diagnostic Diagnostic) CreateTestContext(
        string code = "class C { }")
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST001", "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return (document, root, diagnostic);
    }

    [Fact]
    public void CreateFixChain_ReturnsNonNullFunction()
    {
        // Arrange
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        // Act
        var result = FixChainArrows.CreateFixChain("Title", "Key", pipeline);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateFixConfiguration_ReturnsCorrectTitleAndKey()
    {
        // Arrange
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        // Act
        var (title, key, register) = FixChainArrows.CreateFixConfiguration(
            "My Title", "My.Key", pipeline);

        // Assert
        title.Should().Be("My Title");
        key.Should().Be("My.Key");
        register.Should().NotBeNull();
    }

    [Fact]
    public void CreateFixConfiguration_TitleAndEquivalenceKey_ArePreserved()
    {
        // Arrange
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        // Act
        var config = FixChainArrows.CreateFixConfiguration(
            "Fix Null Check", "Ouroboros.NullFix", pipeline);

        // Assert
        config.Title.Should().Be("Fix Null Check");
        config.EquivalenceKey.Should().Be("Ouroboros.NullFix");
    }
}
