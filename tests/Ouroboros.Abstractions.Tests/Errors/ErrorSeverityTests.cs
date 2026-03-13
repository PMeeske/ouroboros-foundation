using Ouroboros.Abstractions.Errors;

namespace Ouroboros.Abstractions.Tests.Errors;

[Trait("Category", "Unit")]
public class ErrorSeverityTests
{
    [Fact]
    public void ErrorSeverity_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ErrorSeverity>().Should().HaveCount(4);
        Enum.IsDefined(ErrorSeverity.Critical).Should().BeTrue();
        Enum.IsDefined(ErrorSeverity.Error).Should().BeTrue();
        Enum.IsDefined(ErrorSeverity.Warning).Should().BeTrue();
        Enum.IsDefined(ErrorSeverity.Info).Should().BeTrue();
    }

    [Fact]
    public void ErrorSeverity_Critical_HasLowestOrdinal()
    {
        // Critical is the most severe, represented by ordinal 0
        ((int)ErrorSeverity.Critical).Should().Be(0);
    }

    [Fact]
    public void ErrorSeverity_CanBeUsedInComparisons()
    {
        // Arrange & Act & Assert
        (ErrorSeverity.Critical < ErrorSeverity.Error).Should().BeTrue();
        (ErrorSeverity.Error < ErrorSeverity.Warning).Should().BeTrue();
        (ErrorSeverity.Warning < ErrorSeverity.Info).Should().BeTrue();
    }
}
