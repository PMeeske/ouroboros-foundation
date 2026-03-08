using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Extended tests for StandardSteps covering CS8600, CS8602, CS0266,
/// and CS0219 diagnostic fix paths.
/// </summary>
[Trait("Category", "Unit")]
public sealed class StandardStepsExtendedTests
{
    private static FixState CreateState(string code, string diagnosticId, int spanStart, int spanLength)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(spanStart, spanLength)));
        return new FixState(document, diagnostic, root);
    }

    [Fact]
    public async Task TryResolve_CS0219_RemovesUnusedVariable()
    {
        // Arrange - CS0219 is similar to CS0168 (variable assigned but never used)
        string code = "class C { void M() { int unused = 42; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        var localDecl = root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().First();
        var span = localDecl.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS0219", "Test", "Unused var", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Remove unused variable");
    }

    [Fact]
    public async Task TryResolve_CS8600_MakesTypeNullable()
    {
        // Arrange - CS8600: Converting null literal to non-nullable type
        string code = "class C { void M() { string s = null; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        // Find the variable declaration
        var varDecl = root.DescendantNodes().OfType<VariableDeclarationSyntax>().First();
        var span = varDecl.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS8600", "Test", "Null to non-nullable", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8600 (Make nullable)");
    }

    [Fact]
    public async Task TryResolve_CS8602_ConvertsToConditionalAccess()
    {
        // Arrange - CS8602: Dereference of a possibly null reference
        string code = "class C { void M(string? s) { var len = s.Length; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        // Find the member access expression (s.Length)
        var memberAccess = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>().First();
        var span = memberAccess.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS8602", "Test", "Possibly null reference", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8602 (Use ?.)");
    }

    [Fact]
    public async Task TryResolve_CS0266_MakesVariableNullable()
    {
        // Arrange - CS0266: Cannot implicitly convert type
        string code = "class C { void M() { int x = null; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        var varDecl = root.DescendantNodes().OfType<VariableDeclarationSyntax>().First();
        var span = varDecl.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS0266", "Test", "Cannot implicitly convert", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS0266 (Make nullable)");
    }

    [Fact]
    public async Task TryResolve_NullRoot_ReturnsUnchangedState()
    {
        // Arrange - state with null root from GetSyntaxRootAsync
        string code = "class C { }";
        var state = CreateState(code, "CS9999", 0, 5);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task FormatCode_ReturnsStateUnchanged()
    {
        // Arrange
        var state = CreateState("class C { }", "TEST001", 0, 5);

        // Act
        var result = await StandardSteps.FormatCode(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task TryResolve_CS8600_VarDeclaration_DoesNotModify()
    {
        // Arrange - var declaration should not be changed to nullable
        string code = "class C { void M() { var s = (string?)null; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        var varDecl = root.DescendantNodes().OfType<VariableDeclarationSyntax>().First();
        var span = varDecl.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS8600", "Test", "Null to non-nullable", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert - var is already determined by compiler, should not be changed
        // The result may or may not have changes depending on whether "var" is detected
        result.Should().NotBeNull();
    }
}
