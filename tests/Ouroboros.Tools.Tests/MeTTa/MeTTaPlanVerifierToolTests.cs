// <copyright file="MeTTaPlanVerifierToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Moq;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MeTTaPlanVerifierTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaPlanVerifierToolTests
{
    private readonly Mock<IMeTTaEngine> mockEngine = new();
    private readonly MeTTaPlanVerifierTool tool;

    public MeTTaPlanVerifierToolTests()
    {
        this.tool = new MeTTaPlanVerifierTool(this.mockEngine.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new MeTTaPlanVerifierTool(null!);

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
        this.tool.Name.Should().Be("metta_verify_plan");
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
    public void Description_ContainsVerifyKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("Verify");
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Assert
        this.tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsPlanProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("plan");
    }

    #endregion

    #region InvokeAsync Tests

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

    [Fact]
    public async Task InvokeAsync_WithValidPlan_ReturnsValid()
    {
        // Arrange
        string plan = "(plan (step1) (step2) (step3))";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync(plan, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        // Act
        var result = await this.tool.InvokeAsync(plan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("valid");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidPlan_ReturnsInvalid()
    {
        // Arrange
        string plan = "(plan (missing-dep))";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync(plan, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(false));

        // Act
        var result = await this.tool.InvokeAsync(plan);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("invalid");
    }

    [Fact]
    public async Task InvokeAsync_WithJsonInput_ExtractsPlanFromJson()
    {
        // Arrange
        string jsonInput = """{"plan": "(plan (step1) (step2))"}""";
        string expectedPlan = "(plan (step1) (step2))";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync(expectedPlan, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.VerifyPlanAsync(expectedPlan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEngineReturnsFailure_PropagatesFailure()
    {
        // Arrange
        string plan = "(plan)";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync(plan, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Failure("Verification error"));

        // Act
        var result = await this.tool.InvokeAsync(plan);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Verification error");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesTokenToEngine()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        string plan = "(plan (step1))";
        this.mockEngine
            .Setup(e => e.VerifyPlanAsync(plan, cts.Token))
            .ReturnsAsync(Result<bool, string>.Success(true));

        // Act
        var result = await this.tool.InvokeAsync(plan, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.VerifyPlanAsync(plan, cts.Token),
            Times.Once);
    }

    #endregion
}
