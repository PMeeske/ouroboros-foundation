// <copyright file="LangChainIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LangChain.Abstractions.Schema;
using LangChain.Chains.HelperChains;
using LangChain.Chains.LLM;
using LangChain.Providers;
using LangChain.Schema;
using Ouroboros.Core.LangChain;

namespace Ouroboros.Core.Tests.LangChain;

/// <summary>
/// Tests for LangChainIntegration static extension methods that bridge
/// LangChain chains with the monadic pipeline system.
/// </summary>
[Trait("Category", "Unit")]
public class LangChainIntegrationTests
{
    // ========================================================================
    // ToMonadicKleisli (BaseStackableChain)
    // ========================================================================

    [Fact]
    public async Task ToMonadicKleisli_BaseStackableChain_HttpRequestException_ReturnsFailure()
    {
        // Arrange
        var mockChain = new Mock<BaseStackableChain>();
        mockChain
            .Setup(c => c.CallAsync(It.IsAny<IChainValues>(), null))
            .ThrowsAsync(new HttpRequestException("connection refused"));

        var kleisli = mockChain.Object.ToMonadicKleisli();
        var input = new Dictionary<string, object> { ["query"] = "test" };

        // Act
        var result = await kleisli(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("LangChain execution failed");
    }

    [Fact]
    public async Task ToMonadicKleisli_BaseStackableChain_InvalidOperationException_ReturnsFailure()
    {
        // Arrange
        var mockChain = new Mock<BaseStackableChain>();
        mockChain
            .Setup(c => c.CallAsync(It.IsAny<IChainValues>(), null))
            .ThrowsAsync(new InvalidOperationException("bad state"));

        var kleisli = mockChain.Object.ToMonadicKleisli();
        var input = new Dictionary<string, object> { ["query"] = "test" };

        // Act
        var result = await kleisli(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("LangChain execution failed");
        result.Error.Should().Contain("bad state");
    }

    [Fact]
    public async Task ToMonadicKleisli_BaseStackableChain_OperationCanceled_Rethrows()
    {
        // Arrange
        var mockChain = new Mock<BaseStackableChain>();
        mockChain
            .Setup(c => c.CallAsync(It.IsAny<IChainValues>(), null))
            .ThrowsAsync(new OperationCanceledException());

        var kleisli = mockChain.Object.ToMonadicKleisli();
        var input = new Dictionary<string, object> { ["query"] = "test" };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => kleisli(input));
    }

    // ========================================================================
    // ToStep (BaseStackableChain)
    // ========================================================================

    [Fact]
    public void ToStep_BaseStackableChain_ReturnsNonNullStep()
    {
        // Arrange
        var mockChain = new Mock<BaseStackableChain>();

        // Act
        var step = mockChain.Object.ToStep();

        // Assert
        step.Should().NotBeNull();
    }

    // ========================================================================
    // CreateSetKleisli
    // ========================================================================

    [Fact]
    public void CreateSetKleisli_ReturnsNonNullKleisli()
    {
        // Act
        var kleisli = LangChainIntegration.CreateSetKleisli("hello", "myKey");

        // Assert
        kleisli.Should().NotBeNull();
    }

    // ========================================================================
    // CreateSetStep
    // ========================================================================

    [Fact]
    public void CreateSetStep_ReturnsNonNullStep()
    {
        // Act
        var step = LangChainIntegration.CreateSetStep("value", "outputKey");

        // Assert
        step.Should().NotBeNull();
    }

    // ========================================================================
    // CreateLlmKleisli
    // ========================================================================

    [Fact]
    public void CreateLlmKleisli_ReturnsNonNullKleisli()
    {
        // Arrange
        var mockLlm = new Mock<IChatModel>();
        var prompt = new LangChain.Prompts.PromptTemplate("{query}", "query");

        // Act
        var kleisli = LangChainIntegration.CreateLlmKleisli(mockLlm.Object, prompt);

        // Assert
        kleisli.Should().NotBeNull();
    }

    // ========================================================================
    // CreateLlmStep
    // ========================================================================

    [Fact]
    public void CreateLlmStep_ReturnsNonNullStep()
    {
        // Arrange
        var mockLlm = new Mock<IChatModel>();
        var prompt = new LangChain.Prompts.PromptTemplate("{query}", "query");

        // Act
        var step = LangChainIntegration.CreateLlmStep(mockLlm.Object, prompt);

        // Assert
        step.Should().NotBeNull();
    }
}
