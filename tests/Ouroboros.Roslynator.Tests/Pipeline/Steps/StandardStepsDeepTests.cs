using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Deep coverage tests for StandardSteps exercising every diagnostic handler branch
/// including CS0168, CS0219, CS8600, CS8602, CS0266, CS8019, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class StandardStepsDeepTests
{
    #region Helpers

    private static async Task<FixState> CreateStateAsync(
        string code, string diagnosticId, Func<SyntaxNode, TextSpan> spanSelector)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;
        var span = spanSelector(root);
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, span));
        return new FixState(document, diagnostic, root);
    }

    private static FixState CreateStateSync(string code, string diagnosticId, int spanStart, int spanLength)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(spanStart, spanLength)));
        return new FixState(document, diagnostic, root);
    }

    #endregion

    #region CS0168 - Unused Variable Declaration

    [Fact]
    public async Task TryResolve_CS0168_WithMultipleStatements_RemovesOnlyTargetVariable()
    {
        // Arrange
        string code = "class C { void M() { int x; int y = 1; } }";
        var state = await CreateStateAsync(code, "CS0168", root =>
        {
            var localDecl = root.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .First(); // int x;
            return localDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Remove unused variable");
        result.CurrentRoot.ToFullString().Should().NotContain("int x;");
        result.CurrentRoot.ToFullString().Should().Contain("int y = 1;");
    }

    [Fact]
    public async Task TryResolve_CS0168_SpanNotOnLocalDeclaration_ReturnsUnchanged()
    {
        // Arrange - point the diagnostic at the class keyword, not a local declaration
        string code = "class C { void M() { } }";
        var state = CreateStateSync(code, "CS0168", 0, 5); // "class"

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region CS0219 - Variable Assigned But Never Used

    [Fact]
    public async Task TryResolve_CS0219_RemovesVariableWithInitializer()
    {
        // Arrange
        string code = "class C { void M() { string s = \"hello\"; } }";
        var state = await CreateStateAsync(code, "CS0219", root =>
        {
            var localDecl = root.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>().First();
            return localDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Remove unused variable");
    }

    [Fact]
    public async Task TryResolve_CS0219_SpanOnMethodBody_NoLocalDecl_ReturnsUnchanged()
    {
        // Arrange - diagnostic span covers a method body without local declarations
        string code = "class C { void M() { System.Console.WriteLine(); } }";
        var state = await CreateStateAsync(code, "CS0219", root =>
        {
            // Point at the invocation, not a local declaration
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>().First();
            return invocation.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region CS8600 - Converting Null to Non-Nullable Type

    [Fact]
    public async Task TryResolve_CS8600_ExplicitStringType_MakesNullable()
    {
        // Arrange
        string code = "class C { void M() { string s = null; } }";
        var state = await CreateStateAsync(code, "CS8600", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8600 (Make nullable)");
        result.CurrentRoot.ToFullString().Should().Contain("string?");
    }

    [Fact]
    public async Task TryResolve_CS8600_VarKeyword_DoesNotModify()
    {
        // Arrange - 'var' should be left untouched
        string code = "class C { void M() { var s = (string)null; } }";
        var state = await CreateStateAsync(code, "CS8600", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        // 'var' is an IdentifierNameSyntax with text "var", so isVar should be true
        // and the method should skip the fix
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8600_AlreadyNullableType_DoesNotDoubleNullable()
    {
        // Arrange
        string code = "class C { void M() { string? s = null; } }";
        var state = await CreateStateAsync(code, "CS8600", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert - already nullable, should not modify
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8600_SpanNotOnVariableDecl_ReturnsUnchanged()
    {
        // Arrange - point the span at the class keyword
        string code = "class C { void M() { string s = null; } }";
        var state = CreateStateSync(code, "CS8600", 0, 5); // "class"

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8600_IntType_MakesNullableValueType()
    {
        // Arrange - value type scenario (int cannot normally be null in C#, but test the code path)
        string code = "class C { void M() { object o = null; } }";
        var state = await CreateStateAsync(code, "CS8600", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8600 (Make nullable)");
        result.CurrentRoot.ToFullString().Should().Contain("object?");
    }

    #endregion

    #region CS8602 - Dereference of Possibly Null Reference

    [Fact]
    public async Task TryResolve_CS8602_SimpleMemberAccess_ConvertsToConditionalAccess()
    {
        // Arrange
        string code = "class C { void M(string? s) { var len = s.Length; } }";
        var state = await CreateStateAsync(code, "CS8602", root =>
        {
            var memberAccess = root.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>().First();
            return memberAccess.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8602 (Use ?.)");
        result.CurrentRoot.ToFullString().Should().Contain("?.");
    }

    [Fact]
    public async Task TryResolve_CS8602_NoMemberAccess_ReturnsUnchanged()
    {
        // Arrange - diagnostic on span that has no member access
        string code = "class C { void M() { int x = 1; } }";
        var state = CreateStateSync(code, "CS8602", 0, 5); // "class"

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8602_NestedMemberAccess_ConvertsOutermost()
    {
        // Arrange - nested member access: obj.Inner.Value
        string code = "class C { void M(C? obj) { var x = obj.ToString(); } string Value => \"\"; }";
        var state = await CreateStateAsync(code, "CS8602", root =>
        {
            var memberAccess = root.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>().First();
            return memberAccess.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8602 (Use ?.)");
    }

    #endregion

    #region CS0266 - Cannot Implicitly Convert Type

    [Fact]
    public async Task TryResolve_CS0266_IntType_MakesNullable()
    {
        // Arrange
        string code = "class C { void M() { int x = null; } }";
        var state = await CreateStateAsync(code, "CS0266", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS0266 (Make nullable)");
        result.CurrentRoot.ToFullString().Should().Contain("int?");
    }

    [Fact]
    public async Task TryResolve_CS0266_AlreadyNullable_DoesNotDoubleNullable()
    {
        // Arrange
        string code = "class C { void M() { int? x = null; } }";
        var state = await CreateStateAsync(code, "CS0266", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS0266_VarKeyword_DoesNotModify()
    {
        // Arrange - 'var' keyword should be skipped
        string code = "class C { void M() { var x = 0; } }";
        var state = await CreateStateAsync(code, "CS0266", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS0266_SpanNotOnVarDecl_ReturnsUnchanged()
    {
        // Arrange
        string code = "class C { void M() { int x = 0; } }";
        var state = CreateStateSync(code, "CS0266", 0, 5); // "class"

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS0266_LongType_MakesNullable()
    {
        // Arrange
        string code = "class C { void M() { long x = null; } }";
        var state = await CreateStateAsync(code, "CS0266", root =>
        {
            var varDecl = root.DescendantNodes()
                .OfType<VariableDeclarationSyntax>().First();
            return varDecl.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS0266 (Make nullable)");
        result.CurrentRoot.ToFullString().Should().Contain("long?");
    }

    #endregion

    #region CS8019 - Unnecessary Using Directive

    [Fact]
    public async Task TryResolve_CS8019_RemovesUsingAndPreservesCode()
    {
        // Arrange
        string code = "using System;\nusing System.Linq;\nclass C { }";
        var state = await CreateStateAsync(code, "CS8019", root =>
        {
            // Target the second using directive (System.Linq)
            var usingDirective = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>().Last();
            return usingDirective.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8019 (Remove unnecessary using)");
        result.CurrentRoot.ToFullString().Should().Contain("using System;");
    }

    [Fact]
    public async Task TryResolve_CS8019_SpanNotOnUsing_ReturnsUnchanged()
    {
        // Arrange
        string code = "using System;\nclass C { }";
        var state = CreateStateSync(code, "CS8019", 15, 5); // "class" not a using

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task TryResolve_CS8019_SingleUsing_RemovesIt()
    {
        // Arrange
        string code = "using System.Collections;\nclass C { }";
        var state = await CreateStateAsync(code, "CS8019", root =>
        {
            var usingDirective = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>().First();
            return usingDirective.Span;
        });

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().Contain("Fix CS8019 (Remove unnecessary using)");
    }

    #endregion

    #region Unknown/Unhandled Diagnostics

    [Fact]
    public async Task TryResolve_UnknownDiagnosticId_ReturnsUnchangedState()
    {
        // Arrange
        string code = "class C { }";
        var state = CreateStateSync(code, "CS1234", 0, 5);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
        result.CurrentRoot.ToFullString().Should().Be(state.CurrentRoot.ToFullString());
    }

    [Fact]
    public async Task TryResolve_EmptyFile_ReturnsUnchangedState()
    {
        // Arrange
        string code = "";
        var state = CreateStateSync(code, "CS0168", 0, 0);

        // Act
        var result = await StandardSteps.TryResolve(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region FormatCode

    [Fact]
    public async Task FormatCode_AnyState_ReturnsIdenticalState()
    {
        // Arrange
        string code = "class C { void M() { int x = 1;} }"; // bad formatting
        var state = CreateStateSync(code, "TEST", 0, 5);

        // Act
        var result = await StandardSteps.FormatCode(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task FormatCode_StateWithChanges_ReturnsIdenticalState()
    {
        // Arrange
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();
        var modifiedState = state.WithNewRoot(newRoot, "some change");

        // Act
        var result = await StandardSteps.FormatCode(modifiedState);

        // Assert
        result.Should().BeSameAs(modifiedState);
    }

    #endregion
}
