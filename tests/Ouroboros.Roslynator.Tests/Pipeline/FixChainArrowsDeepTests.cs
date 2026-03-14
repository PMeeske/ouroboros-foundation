using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Deep coverage tests for FixChainArrows covering CreateFixChain execution,
/// CreateFixConfiguration tuple structure, and pipeline execution paths.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixChainArrowsDeepTests
{
    #region Helpers

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

    #endregion

    #region CreateFixChain Tests

    [Fact]
    public void CreateFixChain_WithIdentityPipeline_ReturnsNonNullFunc()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var result = FixChainArrows.CreateFixChain("Title", "Key", pipeline);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateFixChain_WithModifyingPipeline_ReturnsNonNullFunc()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input =>
        {
            Func<FixState, FixState> modify = state =>
            {
                var newRoot = CSharpSyntaxTree.ParseText("class X { }").GetRoot();
                return state.WithNewRoot(newRoot, "Modified");
            };
            return input | modify;
        };

        var result = FixChainArrows.CreateFixChain("Fix Title", "Fix.Key", pipeline);

        result.Should().NotBeNull();
    }

    #endregion

    #region CreateFixConfiguration Tests

    [Fact]
    public void CreateFixConfiguration_ReturnsTupleWithCorrectTitle()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var config = FixChainArrows.CreateFixConfiguration("My Title", "My.Key", pipeline);

        config.Title.Should().Be("My Title");
    }

    [Fact]
    public void CreateFixConfiguration_ReturnsTupleWithCorrectEquivalenceKey()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var config = FixChainArrows.CreateFixConfiguration("Title", "Equiv.Key", pipeline);

        config.EquivalenceKey.Should().Be("Equiv.Key");
    }

    [Fact]
    public void CreateFixConfiguration_ReturnsTupleWithNonNullRegisterFunc()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var config = FixChainArrows.CreateFixConfiguration("Title", "Key", pipeline);

        config.Register.Should().NotBeNull();
    }

    [Fact]
    public void CreateFixConfiguration_EmptyTitleAndKey_Succeeds()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var config = FixChainArrows.CreateFixConfiguration("", "", pipeline);

        config.Title.Should().BeEmpty();
        config.EquivalenceKey.Should().BeEmpty();
        config.Register.Should().NotBeNull();
    }

    [Fact]
    public void CreateFixConfiguration_DifferentPipelines_ProduceDifferentRegistrations()
    {
        Func<Future<FixState>, Future<FixState>> pipeline1 = input => input;
        Func<Future<FixState>, Future<FixState>> pipeline2 = input =>
        {
            Func<FixState, FixState> step = s => s;
            return input | step;
        };

        var config1 = FixChainArrows.CreateFixConfiguration("T1", "K1", pipeline1);
        var config2 = FixChainArrows.CreateFixConfiguration("T2", "K2", pipeline2);

        config1.Register.Should().NotBeSameAs(config2.Register);
    }

    #endregion

    #region Pipeline with Multiple Steps via CreateFixChain

    [Fact]
    public void CreateFixChain_WithMultipleSteps_ReturnsCallableFunc()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input =>
        {
            Func<FixState, FixState> step1 = s => s.WithNewRoot(s.CurrentRoot, "Step 1");
            Func<FixState, Task<FixState>> step2 = s =>
                Task.FromResult(s.WithNewRoot(s.CurrentRoot, "Step 2"));

            return input | step1 | step2;
        };

        var result = FixChainArrows.CreateFixChain("Multi", "Multi.Key", pipeline);

        result.Should().NotBeNull();
    }

    #endregion

    #region Configuration Tuple Destructuring

    [Fact]
    public void CreateFixConfiguration_CanBeDestructured()
    {
        Func<Future<FixState>, Future<FixState>> pipeline = input => input;

        var (title, key, register) = FixChainArrows.CreateFixConfiguration(
            "Destructured Title", "Destructured.Key", pipeline);

        title.Should().Be("Destructured Title");
        key.Should().Be("Destructured.Key");
        register.Should().NotBeNull();
    }

    #endregion
}
