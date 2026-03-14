using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Deep coverage tests for StandardFixes covering SimplifyLinq with actual
/// LINQ patterns, UseIndexOperator with array[len-1] patterns,
/// UseRangeOperator with Substring patterns, and all helper methods.
/// </summary>
[Trait("Category", "Unit")]
public sealed class StandardFixesDeepTests
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

    #region ShouldSkip Comprehensive Tests

    [Fact]
    public void ShouldSkip_ValidStateWithAllProperties_ReturnsFalse()
    {
        // Arrange
        var state = CreateStateSync("class C { }", "TEST", 0, 5);

        // Act & Assert
        StandardFixes.ShouldSkip(state).Should().BeFalse();
    }

    [Fact]
    public void ShouldSkip_Null_ReturnsTrue()
    {
        StandardFixes.ShouldSkip(null!).Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_NullCurrentRoot_ReturnsTrue()
    {
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };
        StandardFixes.ShouldSkip(nullRootState).Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_NullDiagnostic_ReturnsTrue()
    {
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var nullDiagState = state with { Diagnostic = null! };
        StandardFixes.ShouldSkip(nullDiagState).Should().BeTrue();
    }

    #endregion

    #region FindNodeSafe Comprehensive Tests

    [Fact]
    public void FindNodeSafe_ValidRootAndSpan_ReturnsCorrectNode()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { int x; }");
        var root = tree.GetRoot();
        var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().First();

        var found = StandardFixes.FindNodeSafe(root, field.Span);

        found.Should().NotBeNull();
    }

    [Fact]
    public void FindNodeSafe_NullRoot_ReturnsNull()
    {
        StandardFixes.FindNodeSafe(null!, new TextSpan(0, 5)).Should().BeNull();
    }

    [Fact]
    public void FindNodeSafe_SpanBeyondEndOfFile_ReturnsNull()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        StandardFixes.FindNodeSafe(root, new TextSpan(5000, 10)).Should().BeNull();
    }

    [Fact]
    public void FindNodeSafe_SpanAtStartOfFile_ReturnsNode()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        var node = StandardFixes.FindNodeSafe(root, new TextSpan(0, 1));
        node.Should().NotBeNull();
    }

    [Fact]
    public void FindNodeSafe_SpanCoveringEntireFile_ReturnsNode()
    {
        var code = "class C { }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var node = StandardFixes.FindNodeSafe(root, new TextSpan(0, code.Length));
        node.Should().NotBeNull();
    }

    #endregion

    #region ReplaceNode Tests

    [Fact]
    public async Task ReplaceNode_ReplacesIdentifierInClass()
    {
        // Arrange
        var code = "class OldName { }";
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = (await document.GetSyntaxRootAsync())!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST", "Test", "Test", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        var state = new FixState(document, diagnostic, root);

        var oldClassDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var newClassDecl = oldClassDecl.WithIdentifier(SyntaxFactory.Identifier("NewName"));

        // Act
        var result = await StandardFixes.ReplaceNode(state, oldClassDecl, newClassDecl, "Rename class");

        // Assert
        result.Changes.Should().Contain("Rename class");
        result.CurrentRoot.ToFullString().Should().Contain("NewName");
        result.CurrentRoot.ToFullString().Should().NotContain("OldName");
    }

    [Fact]
    public async Task ReplaceNode_NullState_Throws()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        var act = () => StandardFixes.ReplaceNode(null!, root, root, "test");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReplaceNode_NullOldNode_Throws()
    {
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var tree = CSharpSyntaxTree.ParseText("class D { }");
        var root = tree.GetRoot();

        var act = () => StandardFixes.ReplaceNode(state, null!, root, "test");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReplaceNode_NullNewNode_Throws()
    {
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var oldNode = state.CurrentRoot.DescendantNodes().First();

        var act = () => StandardFixes.ReplaceNode(state, oldNode, null!, "test");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReplaceNode_PreservesDescription()
    {
        var state = CreateStateSync("class C { }", "TEST", 0, 5);
        var classDecl = state.CurrentRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>().First();
        var newDecl = classDecl.WithIdentifier(SyntaxFactory.Identifier("X"));

        var result = await StandardFixes.ReplaceNode(state, classDecl, newDecl, "My Custom Description");

        result.Changes.Should().HaveCount(1);
        result.Changes[0].Should().Be("My Custom Description");
    }

    #endregion

    #region SimplifyLinq - Matching Pattern Tests

    [Fact]
    public async Task SimplifyLinq_WhereFollowedByFirst_SimplifiesCorrectly()
    {
        // Arrange - list.Where(x => x > 0).First()
        string code = @"
using System.Linq;
using System.Collections.Generic;
class C {
    void M() {
        var list = new List<int>();
        var result = list.Where(x => x > 0).First();
    }
}";
        var state = await CreateStateAsync(code, "IDE0001", root =>
        {
            // We need to find the .First() invocation -- it is the outermost InvocationExpression
            // whose MemberAccess name is "First" and whose inner expression is the Where() invocation.
            var firstInvocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(inv =>
                    inv.Expression is MemberAccessExpressionSyntax ma &&
                    ma.Name.Identifier.Text == "First" &&
                    ma.Expression is InvocationExpressionSyntax innerInv &&
                    innerInv.Expression is MemberAccessExpressionSyntax innerMa &&
                    innerMa.Name.Identifier.Text == "Where");

            // Point the diagnostic span at the "First" identifier name inside the member access
            // so that FindNodeSafe will find a node within the invocation
            if (firstInvocation?.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Span;

            return firstInvocation?.Span ?? new TextSpan(0, 5);
        });

        // Act
        var result = await StandardFixes.SimplifyLinq(state);

        // Assert
        result.Changes.Should().Contain("Simplify LINQ");
    }

    [Fact]
    public async Task SimplifyLinq_NoWhereFirst_ReturnsUnchanged()
    {
        // Arrange - no Where().First() pattern
        string code = @"
using System.Linq;
class C {
    void M() {
        var list = new int[] { 1, 2, 3 };
        var result = list.First();
    }
}";
        var state = await CreateStateAsync(code, "IDE0001", root =>
        {
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>().First();
            return invocation.Span;
        });

        // Act
        var result = await StandardFixes.SimplifyLinq(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task SimplifyLinq_ShouldSkipState_ReturnsUnchanged()
    {
        // Arrange - state with null root
        var state = CreateStateSync("class C { }", "IDE0001", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        // Act
        var result = await StandardFixes.SimplifyLinq(nullRootState);

        // Assert
        result.CurrentRoot.Should().BeNull();
    }

    #endregion

    #region UseIndexOperator Tests

    [Fact]
    public async Task UseIndexOperator_ArrayLengthMinusOne_ConvertsToHatOperator()
    {
        // Arrange - array[array.Length - 1]
        string code = @"
class C {
    void M() {
        var arr = new int[] { 1, 2, 3 };
        var last = arr[arr.Length - 1];
    }
}";
        var state = await CreateStateAsync(code, "IDE0056", root =>
        {
            var elementAccess = root.DescendantNodes()
                .OfType<ElementAccessExpressionSyntax>().First();
            return elementAccess.Span;
        });

        // Act
        var result = await StandardFixes.UseIndexOperator(state);

        // Assert
        result.Changes.Should().Contain("Use Index Operator");
    }

    [Fact]
    public async Task UseIndexOperator_SimpleIndex_ReturnsUnchanged()
    {
        // Arrange - simple array[0], no Length - 1 pattern
        string code = @"
class C {
    void M() {
        var arr = new int[] { 1, 2, 3 };
        var first = arr[0];
    }
}";
        var state = await CreateStateAsync(code, "IDE0056", root =>
        {
            var elementAccess = root.DescendantNodes()
                .OfType<ElementAccessExpressionSyntax>().First();
            return elementAccess.Span;
        });

        // Act
        var result = await StandardFixes.UseIndexOperator(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task UseIndexOperator_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0056", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseIndexOperator(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    #endregion

    #region UseRangeOperator Tests

    [Fact]
    public async Task UseRangeOperator_SubstringWithOneArg_ConvertsToRange()
    {
        // Arrange - s.Substring(3)
        string code = @"
class C {
    void M() {
        string s = ""hello"";
        var sub = s.Substring(3);
    }
}";
        var state = await CreateStateAsync(code, "IDE0057", root =>
        {
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "Substring");
            return invocation.Span;
        });

        // Act
        var result = await StandardFixes.UseRangeOperator(state);

        // Assert
        result.Changes.Should().Contain("Use Range Operator");
    }

    [Fact]
    public async Task UseRangeOperator_SubstringWithTwoArgs_ReturnsUnchanged()
    {
        // Arrange - s.Substring(0, 3) has two args, not matched
        string code = @"
class C {
    void M() {
        string s = ""hello"";
        var sub = s.Substring(0, 3);
    }
}";
        var state = await CreateStateAsync(code, "IDE0057", root =>
        {
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .First(i => i.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "Substring");
            return invocation.Span;
        });

        // Act
        var result = await StandardFixes.UseRangeOperator(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task UseRangeOperator_NotSubstring_ReturnsUnchanged()
    {
        // Arrange - s.ToUpper() not Substring
        string code = @"
class C {
    void M() {
        string s = ""hello"";
        var upper = s.ToUpper();
    }
}";
        var state = await CreateStateAsync(code, "IDE0057", root =>
        {
            var invocation = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>().First();
            return invocation.Span;
        });

        // Act
        var result = await StandardFixes.UseRangeOperator(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task UseRangeOperator_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0057", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseRangeOperator(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    #endregion

    #region Placeholder Methods - Additional Coverage

    [Fact]
    public async Task UseCollectionExpression_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0300", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseCollectionExpression(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    [Fact]
    public async Task UseCollectionInitializer_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0028", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseCollectionInitializer(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    [Fact]
    public async Task UseExplicitType_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0008", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseExplicitType(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    [Fact]
    public async Task UseNullPropagation_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0031", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseNullPropagation(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    [Fact]
    public async Task UseObjectInitializer_ShouldSkipState_ReturnsUnchanged()
    {
        var state = CreateStateSync("class C { }", "IDE0017", 0, 5);
        var nullRootState = state with { CurrentRoot = null! };

        var result = await StandardFixes.UseObjectInitializer(nullRootState);

        result.CurrentRoot.Should().BeNull();
    }

    #endregion
}
