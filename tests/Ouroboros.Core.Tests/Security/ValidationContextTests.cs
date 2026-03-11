using Ouroboros.Core.Security;

namespace Ouroboros.Core.Tests.Security;

[Trait("Category", "Unit")]
[Trait("Category", "Security")]
public class ValidationContextTests
{
    // --- Default property values ---

    [Fact]
    public void Default_MaxLength_Is10000()
    {
        var context = new ValidationContext();

        context.MaxLength.Should().Be(10_000);
    }

    [Fact]
    public void Default_MinLength_Is0()
    {
        var context = new ValidationContext();

        context.MinLength.Should().Be(0);
    }

    [Fact]
    public void Default_AllowEmpty_IsFalse()
    {
        var context = new ValidationContext();

        context.AllowEmpty.Should().BeFalse();
    }

    [Fact]
    public void Default_TrimWhitespace_IsTrue()
    {
        var context = new ValidationContext();

        context.TrimWhitespace.Should().BeTrue();
    }

    [Fact]
    public void Default_NormalizeLineEndings_IsTrue()
    {
        var context = new ValidationContext();

        context.NormalizeLineEndings.Should().BeTrue();
    }

    [Fact]
    public void Default_EscapeHtml_IsFalse()
    {
        var context = new ValidationContext();

        context.EscapeHtml.Should().BeFalse();
    }

    [Fact]
    public void Default_BlockedCharacters_IsNull()
    {
        var context = new ValidationContext();

        context.BlockedCharacters.Should().BeNull();
    }

    // --- Static Default factory ---

    [Fact]
    public void StaticDefault_ReturnsNewInstanceWithDefaultValues()
    {
        var context = ValidationContext.Default;

        context.MaxLength.Should().Be(10_000);
        context.MinLength.Should().Be(0);
        context.AllowEmpty.Should().BeFalse();
        context.TrimWhitespace.Should().BeTrue();
        context.NormalizeLineEndings.Should().BeTrue();
        context.EscapeHtml.Should().BeFalse();
        context.BlockedCharacters.Should().BeNull();
    }

    [Fact]
    public void StaticDefault_ReturnsNewInstanceEachTime()
    {
        var context1 = ValidationContext.Default;
        var context2 = ValidationContext.Default;

        context1.Should().NotBeSameAs(context2);
    }

    // --- Static Strict factory ---

    [Fact]
    public void Strict_MaxLength_Is1000()
    {
        var context = ValidationContext.Strict;

        context.MaxLength.Should().Be(1000);
    }

    [Fact]
    public void Strict_EscapeHtml_IsTrue()
    {
        var context = ValidationContext.Strict;

        context.EscapeHtml.Should().BeTrue();
    }

    [Fact]
    public void Strict_BlockedCharacters_ContainsDangerousHtmlChars()
    {
        var context = ValidationContext.Strict;

        context.BlockedCharacters.Should().NotBeNull();
        context.BlockedCharacters.Should().Contain('<');
        context.BlockedCharacters.Should().Contain('>');
        context.BlockedCharacters.Should().Contain('&');
        context.BlockedCharacters.Should().Contain('"');
        context.BlockedCharacters.Should().Contain('\'');
    }

    [Fact]
    public void Strict_ReturnsNewInstanceEachTime()
    {
        var context1 = ValidationContext.Strict;
        var context2 = ValidationContext.Strict;

        context1.Should().NotBeSameAs(context2);
    }

    // --- Static ToolParameter factory ---

    [Fact]
    public void ToolParameter_MaxLength_Is5000()
    {
        var context = ValidationContext.ToolParameter;

        context.MaxLength.Should().Be(5000);
    }

    [Fact]
    public void ToolParameter_TrimWhitespace_IsTrue()
    {
        var context = ValidationContext.ToolParameter;

        context.TrimWhitespace.Should().BeTrue();
    }

    [Fact]
    public void ToolParameter_NormalizeLineEndings_IsTrue()
    {
        var context = ValidationContext.ToolParameter;

        context.NormalizeLineEndings.Should().BeTrue();
    }

    [Fact]
    public void ToolParameter_ReturnsNewInstanceEachTime()
    {
        var context1 = ValidationContext.ToolParameter;
        var context2 = ValidationContext.ToolParameter;

        context1.Should().NotBeSameAs(context2);
    }

    // --- Property setters ---

    [Fact]
    public void MaxLength_CanBeSet()
    {
        var context = new ValidationContext { MaxLength = 500 };

        context.MaxLength.Should().Be(500);
    }

    [Fact]
    public void MinLength_CanBeSet()
    {
        var context = new ValidationContext { MinLength = 10 };

        context.MinLength.Should().Be(10);
    }

    [Fact]
    public void AllowEmpty_CanBeSetToTrue()
    {
        var context = new ValidationContext { AllowEmpty = true };

        context.AllowEmpty.Should().BeTrue();
    }

    [Fact]
    public void TrimWhitespace_CanBeSetToFalse()
    {
        var context = new ValidationContext { TrimWhitespace = false };

        context.TrimWhitespace.Should().BeFalse();
    }

    [Fact]
    public void NormalizeLineEndings_CanBeSetToFalse()
    {
        var context = new ValidationContext { NormalizeLineEndings = false };

        context.NormalizeLineEndings.Should().BeFalse();
    }

    [Fact]
    public void EscapeHtml_CanBeSetToTrue()
    {
        var context = new ValidationContext { EscapeHtml = true };

        context.EscapeHtml.Should().BeTrue();
    }

    [Fact]
    public void BlockedCharacters_CanBeSetToCustomSet()
    {
        var blocked = new HashSet<char> { 'x', 'y', 'z' };
        var context = new ValidationContext { BlockedCharacters = blocked };

        context.BlockedCharacters.Should().BeEquivalentTo(blocked);
    }

    // --- Sealed class ---

    [Fact]
    public void IsSealed()
    {
        typeof(ValidationContext).IsSealed.Should().BeTrue();
    }
}
