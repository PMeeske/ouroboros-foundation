using Ouroboros.Core.Security;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class ValidationResultTests
{
    // --- Success factory ---

    [Fact]
    public void Success_IsValid_IsTrue()
    {
        var result = ValidationResult.Success("clean input");

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Success_SanitizedValue_MatchesInput()
    {
        var result = ValidationResult.Success("sanitized text");

        result.SanitizedValue.Should().Be("sanitized text");
    }

    [Fact]
    public void Success_Errors_IsEmpty()
    {
        var result = ValidationResult.Success("clean");

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithEmptyString_SetsEmptySanitizedValue()
    {
        var result = ValidationResult.Success("");

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().BeEmpty();
    }

    // --- Failure factory ---

    [Fact]
    public void Failure_IsValid_IsFalse()
    {
        var result = ValidationResult.Failure("Input too long");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Failure_SanitizedValue_IsNull()
    {
        var result = ValidationResult.Failure("Error");

        result.SanitizedValue.Should().BeNull();
    }

    [Fact]
    public void Failure_SingleError_ContainsError()
    {
        var result = ValidationResult.Failure("Input too long");

        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be("Input too long");
    }

    [Fact]
    public void Failure_MultipleErrors_ContainsAllErrors()
    {
        var result = ValidationResult.Failure("Too long", "Contains injection", "Blocked chars");

        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Too long");
        result.Errors.Should().Contain("Contains injection");
        result.Errors.Should().Contain("Blocked chars");
    }

    [Fact]
    public void Failure_NoErrors_CreatesEmptyErrorList()
    {
        var result = ValidationResult.Failure();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    // --- Default property values ---

    [Fact]
    public void DefaultConstructor_IsValid_IsFalse()
    {
        var result = new ValidationResult();

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructor_SanitizedValue_IsNull()
    {
        var result = new ValidationResult();

        result.SanitizedValue.Should().BeNull();
    }

    [Fact]
    public void DefaultConstructor_Errors_IsEmptyList()
    {
        var result = new ValidationResult();

        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    // --- Init properties ---

    [Fact]
    public void IsValid_CanBeSetViaInit()
    {
        var result = new ValidationResult { IsValid = true };

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SanitizedValue_CanBeSetViaInit()
    {
        var result = new ValidationResult { SanitizedValue = "test" };

        result.SanitizedValue.Should().Be("test");
    }

    [Fact]
    public void Errors_CanBeSetViaInit()
    {
        var errors = new List<string> { "err1", "err2" };
        var result = new ValidationResult { Errors = errors };

        result.Errors.Should().BeEquivalentTo(errors);
    }

    // --- Sealed class ---

    [Fact]
    public void IsSealed()
    {
        typeof(ValidationResult).IsSealed.Should().BeTrue();
    }

    // --- Edge cases ---

    [Fact]
    public void Success_WithWhitespaceValue_PreservesWhitespace()
    {
        var result = ValidationResult.Success("  spaces  ");

        result.SanitizedValue.Should().Be("  spaces  ");
    }

    [Fact]
    public void Success_WithSpecialCharacters_PreservesCharacters()
    {
        var result = ValidationResult.Success("<script>alert('xss')</script>");

        result.SanitizedValue.Should().Be("<script>alert('xss')</script>");
    }

    [Fact]
    public void Failure_ErrorsListPreservesOrder()
    {
        var result = ValidationResult.Failure("first", "second", "third");

        result.Errors[0].Should().Be("first");
        result.Errors[1].Should().Be("second");
        result.Errors[2].Should().Be("third");
    }
}
