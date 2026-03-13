using Ouroboros.Core;

namespace Ouroboros.Core.Tests;

[Trait("Category", "Unit")]
public class EnvironmentDetectorTests
{
    [Fact]
    public void GetEnvironmentName_ReturnsEnvironmentVariable()
    {
        // This test verifies the method doesn't throw
        // Actual value depends on runtime environment
        _ = EnvironmentDetector.GetEnvironmentName();
        // Result may be null or a string - both are valid
    }

    [Fact]
    public void IsRunningInKubernetes_ReturnsBool()
    {
        // In test environment, should not be in K8s
        var result = EnvironmentDetector.IsRunningInKubernetes();
        result.Should().BeFalse("tests should not run in Kubernetes");
    }

    [Fact]
    public void IsLocalDevelopment_ReturnsBool()
    {
        // Just verify it doesn't throw
        var act = () => EnvironmentDetector.IsLocalDevelopment();
        act.Should().NotThrow();
    }

    [Fact]
    public void IsProduction_ReturnsBool()
    {
        var act = () => EnvironmentDetector.IsProduction();
        act.Should().NotThrow();
    }

    [Fact]
    public void IsStaging_ReturnsBool()
    {
        var result = EnvironmentDetector.IsStaging();
        // In test environment, we shouldn't be in staging
        result.Should().BeFalse();
    }
}
