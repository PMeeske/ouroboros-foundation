using System.Collections.Immutable;
using Ouroboros.Roslynator.Providers;

namespace Ouroboros.Tests.Providers;

/// <summary>
/// Extended tests for UniversalCodeFixProvider covering provider behavior
/// and ConcreteChains configuration.
/// </summary>
[Trait("Category", "Unit")]
public sealed class UniversalCodeFixProviderExtendedTests
{
    [Fact]
    public void FixableDiagnosticIds_ContainsCS8600()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("CS8600");
    }

    [Fact]
    public void FixableDiagnosticIds_ContainsCS8602()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("CS8602");
    }

    [Fact]
    public void FixableDiagnosticIds_ContainsIDE0008()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("IDE0008");
    }

    [Fact]
    public void FixableDiagnosticIds_ContainsCS0168()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Assert
        provider.FixableDiagnosticIds.Should().Contain("CS0168");
    }

    [Fact]
    public void FixableDiagnosticIds_IsImmutableArray()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Act
        var ids = provider.FixableDiagnosticIds;

        // Assert
        ids.Should().BeOfType<ImmutableArray<string>>();
        ids.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void GetFixAllProvider_IsWellKnownBatchFixer()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Act
        var fixAllProvider = provider.GetFixAllProvider();

        // Assert
        fixAllProvider.Should().NotBeNull();
        fixAllProvider.Should().BeSameAs(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public void UniversalChain_CanBeInstantiated()
    {
        // Act
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        // Assert
        chain.Should().NotBeNull();
        chain.Title.Should().NotBeNullOrEmpty();
        chain.EquivalenceKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void UniversalChain_TitleContainsFix()
    {
        // Arrange
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        // Assert
        chain.Title.Should().Contain("Fix");
    }

    [Fact]
    public void UniversalChain_EquivalenceKeyContainsOuroboros()
    {
        // Arrange
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        // Assert
        chain.EquivalenceKey.Should().Contain("Ouroboros");
    }

    [Fact]
    public void MultipleInstances_HaveSameFixableDiagnosticIds()
    {
        // Arrange
        var provider1 = new UniversalCodeFixProvider();
        var provider2 = new UniversalCodeFixProvider();

        // Assert
        provider1.FixableDiagnosticIds.Should().Equal(provider2.FixableDiagnosticIds);
    }
}
