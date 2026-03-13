// <copyright file="OuroborosErrorTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Ouroboros.Abstractions.Errors;
using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Errors;

[Trait("Category", "Unit")]
public class OuroborosErrorTests
{
    // --- Factory: From(code, message) ---

    [Fact]
    public void From_CodeAndMessage_SetsProperties()
    {
        // Arrange & Act
        var error = OuroborosError.From("LLM_001", "Parse failed");

        // Assert
        error.Code.Should().Be("LLM_001");
        error.Message.Should().Be("Parse failed");
        error.Detail.Should().BeNull();
        error.InnerException.Should().BeNull();
    }

    // --- Factory: From(code, exception) ---

    [Fact]
    public void From_CodeAndException_SetsMessageAndException()
    {
        // Arrange
        var ex = new InvalidOperationException("boom");

        // Act
        var error = OuroborosError.From("REASONING_001", ex);

        // Assert
        error.Code.Should().Be("REASONING_001");
        error.Message.Should().Be("boom");
        error.InnerException.Should().BeSameAs(ex);
    }

    // --- Implicit conversion from string ---

    [Fact]
    public void ImplicitConversion_FromString_CreatesUnknownCode()
    {
        // Arrange & Act
        OuroborosError error = "something went wrong";

        // Assert
        error.Code.Should().Be("UNKNOWN");
        error.Message.Should().Be("something went wrong");
    }

    // --- ToString ---

    [Fact]
    public void ToString_FormatsCodeAndMessage()
    {
        // Arrange
        var error = OuroborosError.From("ETHICS_001", "Denied");

        // Act & Assert
        error.ToString().Should().Be("[ETHICS_001] Denied");
    }

    // --- Detail property ---

    [Fact]
    public void Detail_CanBeSetViaInitializer()
    {
        // Arrange & Act
        var error = new OuroborosError
        {
            Code = "VAL_001",
            Message = "Input invalid",
            Detail = "Field 'name' must not be empty"
        };

        // Assert
        error.Detail.Should().Be("Field 'name' must not be empty");
    }

    // --- Record equality ---

    [Fact]
    public void Equality_SameCodeAndMessage_AreEqual()
    {
        // Arrange
        var a = OuroborosError.From("LLM_001", "Parse failed");
        var b = OuroborosError.From("LLM_001", "Parse failed");

        // Assert
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        // Arrange
        var a = OuroborosError.From("LLM_001", "Parse failed");
        var b = OuroborosError.From("VAL_001", "Parse failed");

        // Assert
        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        // Arrange
        var a = OuroborosError.From("LLM_001", "Parse failed");
        var b = OuroborosError.From("LLM_001", "Different error");

        // Assert
        a.Should().NotBe(b);
    }

    // --- With expression (record immutability) ---

    [Fact]
    public void With_ProducesNewInstanceWithUpdatedField()
    {
        // Arrange
        var original = OuroborosError.From("LLM_001", "Parse failed");

        // Act
        var updated = original with { Detail = "at line 42" };

        // Assert
        updated.Code.Should().Be("LLM_001");
        updated.Message.Should().Be("Parse failed");
        updated.Detail.Should().Be("at line 42");
        original.Detail.Should().BeNull("original should be unchanged");
    }

    // --- Integration with Result<T, OuroborosError> ---

    [Fact]
    public void Result_WithOuroborosError_FailurePreservesError()
    {
        // Arrange
        var error = OuroborosError.From(ErrorCodes.EthicsViolation, "Action denied by ethics framework");

        // Act
        var result = Result<int, OuroborosError>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ETHICS_001");
        result.Error.Message.Should().Be("Action denied by ethics framework");
    }

    [Fact]
    public void Result_WithOuroborosError_SuccessPreservesValue()
    {
        // Arrange & Act
        var result = Result<int, OuroborosError>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_WithOuroborosError_MapErrorPreservesStructure()
    {
        // Arrange
        var result = Result<int, OuroborosError>.Failure(
            OuroborosError.From(ErrorCodes.LlmParseFailure, "Bad JSON"));

        // Act
        var mapped = result.MapError(e => $"[{e.Code}] {e.Message}");

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("[LLM_001] Bad JSON");
    }

    [Fact]
    public void Result_WithOuroborosError_BindPropagatesError()
    {
        // Arrange
        var result = Result<int, OuroborosError>.Failure(
            OuroborosError.From(ErrorCodes.TimeoutExpired, "Took too long"));

        // Act
        var bound = result.Bind(v => Result<string, OuroborosError>.Success(v.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Code.Should().Be("TIMEOUT_001");
    }

    // --- Implicit conversion in Result context ---

    [Fact]
    public void ImplicitConversion_WorksInResultFailureContext()
    {
        // Arrange & Act
        // Simulates backward-compatible usage: passing a string where OuroborosError is expected
        OuroborosError error = "legacy string error";
        var result = Result<int, OuroborosError>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UNKNOWN");
        result.Error.Message.Should().Be("legacy string error");
    }

    // --- Exception preservation ---

    [Fact]
    public void From_Exception_PreservesStackTrace()
    {
        // Arrange
        Exception? caught = null;
        try
        {
            throw new ArgumentException("bad arg");
        }
        catch (ArgumentException ex)
        {
            caught = ex;
        }

        // Act
        var error = OuroborosError.From(ErrorCodes.ValidationFailed, caught!);

        // Assert
        error.InnerException.Should().NotBeNull();
        error.InnerException!.StackTrace.Should().NotBeNullOrEmpty();
        error.Message.Should().Be("bad arg");
    }
}
