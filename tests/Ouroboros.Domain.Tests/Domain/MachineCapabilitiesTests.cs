namespace Ouroboros.Tests.Domain;

[Trait("Category", "Unit")]
public class MachineCapabilitiesTests
{
    [Fact]
    public void CpuCores_ShouldBePositive()
    {
        Ouroboros.Domain.MachineCapabilities.CpuCores.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TotalMemoryMb_ShouldBePositive()
    {
        Ouroboros.Domain.MachineCapabilities.TotalMemoryMb.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GpuCount_ShouldBeNonNegative()
    {
        Ouroboros.Domain.MachineCapabilities.GpuCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CpuCores_ShouldMatchProcessorCount()
    {
        Ouroboros.Domain.MachineCapabilities.CpuCores.Should().Be(System.Environment.ProcessorCount);
    }
}
