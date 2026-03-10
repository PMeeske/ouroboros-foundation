// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.SelfModification;

using System.Reflection;
using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

/// <summary>
/// Tests for StringExtensions.Truncate — the internal string truncation helper.
/// Since StringExtensions is internal, we test via reflection.
/// </summary>
[Trait("Category", "Unit")]
public class StringExtensionsTests
{
    private static string? InvokeTruncate(string value, int maxLength)
    {
        Type? type = typeof(GitReflectionService).Assembly
            .GetType("Ouroboros.Domain.SelfModification.StringExtensions");
        type.Should().NotBeNull("StringExtensions should exist as an internal type");

        MethodInfo? method = type!.GetMethod("Truncate", BindingFlags.Public | BindingFlags.Static);
        method.Should().NotBeNull("Truncate should be a public static extension method");

        return (string?)method!.Invoke(null, new object[] { value, maxLength });
    }

    // ----------------------------------------------------------------
    // Null / empty input
    // ----------------------------------------------------------------

    [Fact]
    public void Truncate_NullString_ReturnsNull()
    {
        string? result = InvokeTruncate(null!, 10);

        result.Should().BeNull();
    }

    [Fact]
    public void Truncate_EmptyString_ReturnsEmpty()
    {
        string? result = InvokeTruncate(string.Empty, 10);

        result.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // String shorter than or equal to maxLength
    // ----------------------------------------------------------------

    [Fact]
    public void Truncate_ShorterThanMax_ReturnsOriginal()
    {
        string? result = InvokeTruncate("Hello", 10);

        result.Should().Be("Hello");
    }

    [Fact]
    public void Truncate_ExactlyMaxLength_ReturnsOriginal()
    {
        string? result = InvokeTruncate("12345", 5);

        result.Should().Be("12345");
    }

    // ----------------------------------------------------------------
    // String longer than maxLength
    // ----------------------------------------------------------------

    [Fact]
    public void Truncate_LongerThanMax_TruncatesWithEllipsis()
    {
        string? result = InvokeTruncate("Hello, World!", 8);

        result.Should().NotBeNull();
        result!.Length.Should().Be(8);
        result.Should().EndWith("...");
    }

    [Fact]
    public void Truncate_LongerThanMax_PreservesPrefix()
    {
        // maxLength = 10 => keeps first 7 chars + "..."
        string? result = InvokeTruncate("abcdefghijklmnop", 10);

        result.Should().Be("abcdefg...");
    }

    [Theory]
    [InlineData("abcdef", 4, "a...")]
    [InlineData("abcdef", 6, "abcdef")]
    [InlineData("abcdef", 5, "ab...")]
    public void Truncate_VariousLengths_BehavesCorrectly(string input, int maxLength, string expected)
    {
        string? result = InvokeTruncate(input, maxLength);

        result.Should().Be(expected);
    }
}
