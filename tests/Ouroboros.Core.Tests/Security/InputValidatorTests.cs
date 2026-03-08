// <copyright file="InputValidatorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Security;

namespace Ouroboros.Core.Tests.Security;

/// <summary>
/// Tests for InputValidator covering injection detection, sanitization, and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class InputValidatorTests
{
    private readonly InputValidator _validator;
    private readonly InputValidator _lenientValidator;

    public InputValidatorTests()
    {
        _validator = new InputValidator();
        _lenientValidator = new InputValidator(ValidationOptions.Lenient);
    }

    // --- Empty input ---

    [Fact]
    public void ValidateAndSanitize_EmptyInput_WithAllowEmpty_Succeeds()
    {
        var context = new ValidationContext { AllowEmpty = true };
        var result = _validator.ValidateAndSanitize("", context);

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidateAndSanitize_EmptyInput_WithoutAllowEmpty_Fails()
    {
        var context = new ValidationContext { AllowEmpty = false };
        var result = _validator.ValidateAndSanitize("", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Input cannot be empty");
    }

    [Fact]
    public void ValidateAndSanitize_NullInput_WithoutAllowEmpty_Fails()
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize(null!, context);

        result.IsValid.Should().BeFalse();
    }

    // --- Length checks ---

    [Fact]
    public void ValidateAndSanitize_ExceedsMaxLength_Fails()
    {
        var context = new ValidationContext { MaxLength = 10 };
        var result = _validator.ValidateAndSanitize("This is a long input string", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("maximum length"));
    }

    [Fact]
    public void ValidateAndSanitize_BelowMinLength_Fails()
    {
        var context = new ValidationContext { MinLength = 10 };
        var result = _validator.ValidateAndSanitize("short", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least"));
    }

    [Fact]
    public void ValidateAndSanitize_WithinLengthBounds_Succeeds()
    {
        var context = new ValidationContext { MinLength = 3, MaxLength = 20 };
        var result = _validator.ValidateAndSanitize("valid input", context);

        result.IsValid.Should().BeTrue();
    }

    // --- SQL injection detection ---

    [Theory]
    [InlineData("'; DROP TABLE users; --")]
    [InlineData("admin' OR '1'='1")]
    [InlineData("input UNION SELECT * FROM passwords")]
    [InlineData("'; DELETE FROM accounts; --")]
    public void ValidateAndSanitize_SQLInjection_Fails(string input)
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize(input, context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SQL injection"));
    }

    // --- Command injection detection ---

    [Theory]
    [InlineData("file.txt; rm -rf /")]
    [InlineData("input && cat /etc/passwd")]
    [InlineData("test || malicious_command")]
    [InlineData("../../../etc/shadow")]
    public void ValidateAndSanitize_CommandInjection_Fails(string input)
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize(input, context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("command injection"));
    }

    // --- Script injection detection ---

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:void(0)")]
    [InlineData("<iframe src='evil.com'></iframe>")]
    [InlineData("test onerror=alert(1)")]
    public void ValidateAndSanitize_ScriptInjection_Fails(string input)
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize(input, context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("script injection"));
    }

    // --- Dangerous characters ---

    [Fact]
    public void ValidateAndSanitize_NullByte_Fails()
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize("test\0input", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("null bytes"));
    }

    [Fact]
    public void ValidateAndSanitize_ControlCharacters_Fails()
    {
        var context = ValidationContext.Default;
        // ASCII control character (bell)
        var result = _validator.ValidateAndSanitize("test\u0007input", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("control character"));
    }

    [Fact]
    public void ValidateAndSanitize_AllowedControlChars_Succeeds()
    {
        // Newline, carriage return, tab are allowed
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize("line1\nline2\ttab", context);

        result.IsValid.Should().BeTrue();
    }

    // --- Blocked characters ---

    [Fact]
    public void ValidateAndSanitize_BlockedCharacters_Fails()
    {
        var context = new ValidationContext
        {
            BlockedCharacters = new HashSet<char> { '@', '#' }
        };
        var result = _validator.ValidateAndSanitize("user@email.com", context);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("blocked character"));
    }

    // --- Sanitization ---

    [Fact]
    public void ValidateAndSanitize_TrimWhitespace_Trims()
    {
        var context = new ValidationContext { TrimWhitespace = true };
        var result = _validator.ValidateAndSanitize("  hello  ", context);

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("hello");
    }

    [Fact]
    public void ValidateAndSanitize_NormalizeLineEndings_NormalizesToLF()
    {
        var context = new ValidationContext { NormalizeLineEndings = true };
        var result = _validator.ValidateAndSanitize("line1\r\nline2\rline3", context);

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Be("line1\nline2\nline3");
    }

    [Fact]
    public void ValidateAndSanitize_EscapeHtml_EncodesHtmlEntities()
    {
        var context = new ValidationContext { EscapeHtml = true };
        var result = _validator.ValidateAndSanitize("a < b & c > d", context);

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().Contain("&lt;");
        result.SanitizedValue.Should().Contain("&amp;");
        result.SanitizedValue.Should().Contain("&gt;");
    }

    // --- Lenient mode ---

    [Fact]
    public void LenientValidator_SkipsInjectionChecks()
    {
        var context = ValidationContext.Default;
        // This would fail with default options, but lenient skips injection patterns
        var result = _lenientValidator.ValidateAndSanitize("test OR 1=1", context);

        result.IsValid.Should().BeTrue();
    }

    // --- Strict context ---

    [Fact]
    public void StrictContext_HasLowerMaxLength()
    {
        ValidationContext.Strict.MaxLength.Should().Be(1000);
    }

    [Fact]
    public void StrictContext_EscapesHtml()
    {
        ValidationContext.Strict.EscapeHtml.Should().BeTrue();
    }

    // --- ToolParameter context ---

    [Fact]
    public void ToolParameterContext_HasModerateMaxLength()
    {
        ValidationContext.ToolParameter.MaxLength.Should().Be(5000);
    }

    // --- Normal valid input ---

    [Fact]
    public void ValidateAndSanitize_NormalText_Succeeds()
    {
        var context = ValidationContext.Default;
        var result = _validator.ValidateAndSanitize(
            "Hello, this is a normal user input with some text.", context);

        result.IsValid.Should().BeTrue();
        result.SanitizedValue.Should().NotBeNullOrEmpty();
    }
}
