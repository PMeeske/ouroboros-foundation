using Ouroboros.Core.Security;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class ValidationOptionsTests
{
    // --- Default property values ---

    [Fact]
    public void Default_CheckInjectionPatterns_IsTrue()
    {
        var options = new ValidationOptions();

        options.CheckInjectionPatterns.Should().BeTrue();
    }

    [Fact]
    public void Default_CheckDangerousCharacters_IsTrue()
    {
        var options = new ValidationOptions();

        options.CheckDangerousCharacters.Should().BeTrue();
    }

    [Fact]
    public void Default_RemoveControlCharacters_IsTrue()
    {
        var options = new ValidationOptions();

        options.RemoveControlCharacters.Should().BeTrue();
    }

    // --- Static Default factory ---

    [Fact]
    public void StaticDefault_ReturnsInstanceWithAllChecksEnabled()
    {
        var options = ValidationOptions.Default;

        options.CheckInjectionPatterns.Should().BeTrue();
        options.CheckDangerousCharacters.Should().BeTrue();
        options.RemoveControlCharacters.Should().BeTrue();
    }

    [Fact]
    public void StaticDefault_ReturnsNewInstanceEachTime()
    {
        var options1 = ValidationOptions.Default;
        var options2 = ValidationOptions.Default;

        options1.Should().NotBeSameAs(options2);
    }

    // --- Static Lenient factory ---

    [Fact]
    public void Lenient_CheckInjectionPatterns_IsFalse()
    {
        var options = ValidationOptions.Lenient;

        options.CheckInjectionPatterns.Should().BeFalse();
    }

    [Fact]
    public void Lenient_CheckDangerousCharacters_IsTrue()
    {
        var options = ValidationOptions.Lenient;

        options.CheckDangerousCharacters.Should().BeTrue();
    }

    [Fact]
    public void Lenient_RemoveControlCharacters_IsTrue()
    {
        var options = ValidationOptions.Lenient;

        options.RemoveControlCharacters.Should().BeTrue();
    }

    [Fact]
    public void Lenient_ReturnsNewInstanceEachTime()
    {
        var options1 = ValidationOptions.Lenient;
        var options2 = ValidationOptions.Lenient;

        options1.Should().NotBeSameAs(options2);
    }

    // --- Property setters ---

    [Fact]
    public void CheckInjectionPatterns_CanBeSetToFalse()
    {
        var options = new ValidationOptions { CheckInjectionPatterns = false };

        options.CheckInjectionPatterns.Should().BeFalse();
    }

    [Fact]
    public void CheckDangerousCharacters_CanBeSetToFalse()
    {
        var options = new ValidationOptions { CheckDangerousCharacters = false };

        options.CheckDangerousCharacters.Should().BeFalse();
    }

    [Fact]
    public void RemoveControlCharacters_CanBeSetToFalse()
    {
        var options = new ValidationOptions { RemoveControlCharacters = false };

        options.RemoveControlCharacters.Should().BeFalse();
    }

    // --- Sealed class ---

    [Fact]
    public void IsSealed()
    {
        typeof(ValidationOptions).IsSealed.Should().BeTrue();
    }

    // --- Custom combinations ---

    [Fact]
    public void AllChecksDisabled_CanBeCreated()
    {
        var options = new ValidationOptions
        {
            CheckInjectionPatterns = false,
            CheckDangerousCharacters = false,
            RemoveControlCharacters = false
        };

        options.CheckInjectionPatterns.Should().BeFalse();
        options.CheckDangerousCharacters.Should().BeFalse();
        options.RemoveControlCharacters.Should().BeFalse();
    }
}
