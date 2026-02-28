// <copyright file="MeTTaToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Moq;
using Ouroboros.Abstractions;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MeTTaTool"/> (the general-purpose MeTTa tool).
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaToolTests
{
    private readonly Mock<IMeTTaEngine> mockEngine = new();
    private readonly MeTTaTool tool;

    public MeTTaToolTests()
    {
        this.tool = new MeTTaTool(this.mockEngine.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new MeTTaTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("engine");
    }

    #endregion

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("metta");
    }

    [Fact]
    public void Name_IsNotNullOrEmpty()
    {
        // Assert
        this.tool.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Assert
        this.tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ContainsMeTTaKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("MeTTa");
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Assert
        this.tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsExpressionProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("expression");
    }

    [Fact]
    public void JsonSchema_ContainsOperationProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("operation");
    }

    [Fact]
    public void JsonSchema_ContainsOperationEnumValues()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("query");
        this.tool.JsonSchema.Should().Contain("add_fact");
        this.tool.JsonSchema.Should().Contain("apply_rule");
        this.tool.JsonSchema.Should().Contain("verify_plan");
    }

    #endregion

    #region InvokeAsync - Empty Input Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyInput_ReturnsFailure()
    {
        // Act
        var result = await this.tool.InvokeAsync(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespace_ReturnsFailure()
    {
        // Act
        var result = await this.tool.InvokeAsync("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    #endregion

    #region InvokeAsync - Query Operation Tests

    [Fact]
    public async Task InvokeAsync_WithDirectExpression_DefaultsToQuery()
    {
        // Arrange
        string expression = "!(+ 2 3)";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(expression, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("5"));

        // Act
        var result = await this.tool.InvokeAsync(expression);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task InvokeAsync_WithJsonQueryOperation_ExecutesQuery()
    {
        // Arrange
        string jsonInput = """{"expression": "!(+ 1 2)", "operation": "query"}""";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync("!(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task InvokeAsync_WithJsonNoOperation_DefaultsToQuery()
    {
        // Arrange
        string jsonInput = """{"expression": "!(+ 1 2)"}""";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync("!(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region InvokeAsync - AddFact Operation Tests

    [Fact]
    public async Task InvokeAsync_WithAddFactOperation_AddsFact()
    {
        // Arrange
        string jsonInput = """{"expression": "(parent John Mary)", "operation": "add_fact"}""";
        this.mockEngine
            .Setup(e => e.AddFactAsync("(parent John Mary)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(default));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Fact added successfully");
    }

    [Fact]
    public async Task InvokeAsync_WithAddFactFailure_ReturnsFailure()
    {
        // Arrange
        string jsonInput = """{"expression": "(invalid)", "operation": "add_fact"}""";
        this.mockEngine
            .Setup(e => e.AddFactAsync("(invalid)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("Parse error"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    #endregion

    #region InvokeAsync - ApplyRule Operation Tests

    [Fact]
    public async Task InvokeAsync_WithApplyRuleOperation_AppliesRule()
    {
        // Arrange
        string jsonInput = """{"expression": "(implies (A $x) (B $x))", "operation": "apply_rule"}""";
        this.mockEngine
            .Setup(e => e.ApplyRuleAsync("(implies (A $x) (B $x))", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("Rule applied"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Rule applied");
    }

    #endregion

    #region InvokeAsync - VerifyPlan Operation Tests

    [Fact]
    public async Task InvokeAsync_WithVerifyPlanValid_ReturnsValid()
    {
        // Arrange
        string jsonInput = """{"expression": "(plan (step1))", "operation": "verify_plan"}""";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync("(plan (step1))", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("valid");
    }

    [Fact]
    public async Task InvokeAsync_WithVerifyPlanInvalid_ReturnsInvalid()
    {
        // Arrange
        string jsonInput = """{"expression": "(plan (bad))", "operation": "verify_plan"}""";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync("(plan (bad))", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(false));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("invalid");
    }

    #endregion

    #region InvokeAsync - Unknown Operation Tests

    [Fact]
    public async Task InvokeAsync_WithUnknownOperation_ReturnsFailure()
    {
        // Arrange
        string jsonInput = """{"expression": "something", "operation": "unknown_op"}""";

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown operation");
    }

    #endregion

    #region InvokeAsync - JSON Parsing Edge Cases

    [Fact]
    public async Task InvokeAsync_WithJsonMissingExpression_ReturnsFailure()
    {
        // Arrange
        string jsonInput = """{"operation": "query"}""";

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("expression");
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedJsonStartingWithBrace_FallsBackToDirectExpression()
    {
        // Arrange - starts with { but is not valid JSON
        string input = "{not valid json at all";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("ok"));

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
