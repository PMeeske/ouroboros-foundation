using System.Collections.Immutable;
using Ouroboros.Roslynator.Providers;

namespace Ouroboros.Tests.Providers;

[Trait("Category", "Unit")]
public sealed class UniversalCodeFixProviderTests
{
    [Fact]
    public void FixableDiagnosticIds_ContainsExpectedIds()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Act
        var ids = provider.FixableDiagnosticIds;

        // Assert
        ids.Should().Contain("CS8600");
        ids.Should().Contain("CS8602");
        ids.Should().Contain("IDE0008");
        ids.Should().Contain("CS0168");
    }

    [Fact]
    public void FixableDiagnosticIds_HasFourEntries()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Act
        var ids = provider.FixableDiagnosticIds;

        // Assert
        ids.Length.Should().Be(4);
    }

    [Fact]
    public void GetFixAllProvider_ReturnsBatchFixer()
    {
        // Arrange
        var provider = new UniversalCodeFixProvider();

        // Act
        var fixAllProvider = provider.GetFixAllProvider();

        // Assert
        fixAllProvider.Should().NotBeNull();
    }

    [Fact]
    public void UniversalChain_Title_IsExpected()
    {
        // Arrange
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        // Assert
        chain.Title.Should().Be("Fix (Standard + AI)");
    }

    [Fact]
    public void UniversalChain_EquivalenceKey_IsExpected()
    {
        // Arrange
        var chain = new UniversalCodeFixProvider.ConcreteChains.UniversalChain();

        // Assert
        chain.EquivalenceKey.Should().Be("Ouroboros.UniversalFix");
    }
}
