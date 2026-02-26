namespace Ouroboros.Tests.Domain;

using Ouroboros.Domain;

[Trait("Category", "Unit")]
public class MachineCapabilitiesTests
{
    [Fact]
    public void CpuCores_ReturnsPositiveValue()
    {
        // Act
        var cores = MachineCapabilities.CpuCores;

        // Assert
        cores.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void TotalMemoryMb_ReturnsPositiveValue()
    {
        // Act
        var memoryMb = MachineCapabilities.TotalMemoryMb;

        // Assert
        memoryMb.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GpuCount_ReturnsNonNegativeValue()
    {
        // Act
        var gpuCount = MachineCapabilities.GpuCount;

        // Assert
        gpuCount.Should().BeGreaterThanOrEqualTo(0);
    }
}
