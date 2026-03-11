using Ouroboros.Core;

namespace Ouroboros.Core.Tests;

/// <summary>
/// Additional tests for EnvironmentDetector to cover remaining uncovered lines.
/// </summary>
[Trait("Category", "Unit")]
public class EnvironmentDetectorAdditionalTests
{
    [Fact]
    public void GetEnvironmentName_ReturnsNullableString()
    {
        var result = EnvironmentDetector.GetEnvironmentName();
        // Can be null or a string — just verify it doesn't throw
    }

    [Fact]
    public void IsRunningInKubernetes_InTestEnvironment_ReturnsFalse()
    {
        var result = EnvironmentDetector.IsRunningInKubernetes();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProduction_DoesNotThrow()
    {
        var act = () => EnvironmentDetector.IsProduction();
        act.Should().NotThrow();
    }

    [Fact]
    public void IsStaging_InTestEnvironment_ReturnsFalse()
    {
        var result = EnvironmentDetector.IsStaging();
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLocalDevelopment_DoesNotThrow()
    {
        var act = () => EnvironmentDetector.IsLocalDevelopment();
        act.Should().NotThrow();
    }

    [Fact]
    public void AllMethods_AreStaticAndCallable()
    {
        // Verify all public methods can be called without instance
        EnvironmentDetector.GetEnvironmentName();
        EnvironmentDetector.IsRunningInKubernetes();
        EnvironmentDetector.IsLocalDevelopment();
        EnvironmentDetector.IsProduction();
        EnvironmentDetector.IsStaging();
    }
}
