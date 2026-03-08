using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Providers;

namespace Ouroboros.Tests.Providers;

/// <summary>
/// Deep coverage tests for UniversalCodeFixProvider covering provider properties,
/// FixAllProvider, and ConcreteChains.UniversalChain pipeline definition.
/// </summary>
[Trait("Category", "Unit")]
public sealed class UniversalCodeFixProviderDeepTests
{
    #region FixableDiagnosticIds

    [Fact]
    public void FixableDiagnosticIds_ContainsExactlyFourIds()
    {
        var provider = new UniversalCodeFixProvider();
        provider.FixableDiagnosticIds.Length.Should().Be(4);
    }

    [Theory]
    [InlineData("CS8600")]
    [InlineData("CS8602")]
    [InlineData("IDE0008")]
    [InlineData("CS0168")]
    public void FixableDiagnosticIds_ContainsId(string expectedId)
    {
        var provider = new UniversalCodeFixProvider();
        provider.FixableDiagnosticIds.Should().Contain(expectedId);
    }

    [Fact]
    public void FixableDiagnosticIds_DoesNotContainUnexpectedIds()
    {
        var provider = new UniversalCodeFixProvider();
        provider.FixableDiagnosticIds.Should().NotContain("CS0001");
        provider.FixableDiagnosticIds.Should().NotContain("CS0266");
    }

    [Fact]
    public void FixableDiagnosticIds_IsNotDefault()
    {
        var provider = new UniversalCodeFixProvider();
        provider.FixableDiagnosticIds.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void FixableDiagnosticIds_ConsistentAcrossMultipleCalls()
    {
        var provider = new UniversalCodeFixProvider();
        var first = provider.FixableDiagnosticIds;
        var second = provider.FixableDiagnosticIds;
        first.Should().Equal(second);
    }

    #endregion

    #region GetFixAllProvider

    [Fact]
    public void GetFixAllProvider_ReturnsNonNull()
    {
        var provider = new UniversalCodeFixProvider();
        provider.GetFixAllProvider().Should().NotBeNull();
    }

    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        var provider = new UniversalCodeFixProvider();
        provider.GetFixAllProvider().Should().BeSameAs(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public void GetFixAllProvider_ConsistentAcrossCalls()
    {
        var provider = new UniversalCodeFixProvider();
        var first = provider.GetFixAllProvider();
        var second = provider.GetFixAllProvider();
        first.Should().BeSameAs(second);
    }

    #endregion

    #region ConcreteChains.UniversalChain

    [Fact]
    public void UniversalChain_Title_IsFixStandardPlusAI()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.Title.Should().Be("Fix (Standard + AI)");
    }

    [Fact]
    public void UniversalChain_EquivalenceKey_IsOuroborosUniversalFix()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.EquivalenceKey.Should().Be("Ouroboros.UniversalFix");
    }

    [Fact]
    public void UniversalChain_IsInstanceOfFixChain()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.Should().BeAssignableTo<FixChain>();
    }

    [Fact]
    public void UniversalChain_TitleContainsFix()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.Title.Should().Contain("Fix");
    }

    [Fact]
    public void UniversalChain_EquivalenceKeyContainsOuroboros()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.EquivalenceKey.Should().Contain("Ouroboros");
    }

    [Fact]
    public void UniversalChain_TitleNotEmpty()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void UniversalChain_EquivalenceKeyNotEmpty()
    {
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        chain.EquivalenceKey.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Multiple Instances

    [Fact]
    public void MultipleProviderInstances_HaveSameFixableIds()
    {
        var p1 = new UniversalCodeFixProvider();
        var p2 = new UniversalCodeFixProvider();

        p1.FixableDiagnosticIds.Should().Equal(p2.FixableDiagnosticIds);
    }

    [Fact]
    public void MultipleChainInstances_HaveSameTitleAndKey()
    {
        var c1 = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();
        var c2 = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        c1.Title.Should().Be(c2.Title);
        c1.EquivalenceKey.Should().Be(c2.EquivalenceKey);
    }

    #endregion

    #region Provider Type

    [Fact]
    public void Provider_IsCodeFixProvider()
    {
        var provider = new UniversalCodeFixProvider();
        provider.Should().BeAssignableTo<CodeFixProvider>();
    }

    [Fact]
    public void Provider_CanBeInstantiated()
    {
        var act = () => new UniversalCodeFixProvider();
        act.Should().NotThrow();
    }

    #endregion
}
