using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

[Trait("Category", "Unit")]
public sealed class StandardStepsTests
{
    private static FixState CreateState(string code, string diagnosticId, int spanStart, int spanLength)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(spanStart, spanLength)));
        return new FixState(document, diagnostic, root);
    }

    [Fact]
    public async Task FormatCode_ValidState_ReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "TEST001", 0, 5);

        // Act
        var result = await StandardSteps.FormatCode(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task TryResolve_UnrecognizedDiagnostic_ReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "CS9999", 0, 5);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8019_RemovesUnnecessaryUsing()
    {
        // Arrange
        string code = "using System.Linq;\nclass C { }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        // Find the using directive span
        var usingDirective = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>().First();
        var span = usingDirective.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS8019", "Test", "Unnecessary using", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8019 (Remove unnecessary using)");
    }

    [Fact]
    public async Task TryResolve_CS0168_RemovesUnusedVariable()
    {
        // Arrange
        string code = "class C { void M() { int x = 0; } }";
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;

        // Find the local declaration
        var localDecl = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax>().First();
        var span = localDecl.Span;

        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS0168", "Test", "Unused var", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        var state = new FixState(document, diagnostic, root);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Remove unused variable");
    }
}
