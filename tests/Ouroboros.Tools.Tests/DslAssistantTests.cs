// <copyright file="DslAssistantTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for DslAssistant covering suggestions, completions, validation,
/// explanations, code generation, and command processing.
/// </summary>
[Trait("Category", "Unit")]
public class DslAssistantTests
{
    #region SuggestNextSteps Tests

    [Fact]
    public async Task SuggestNextSteps_ValidDsl_ReturnsSuggestions()
    {
        // Act
        var suggestions = await DslAssistant.SuggestNextSteps("SetTopic('Test')");

        // Assert
        suggestions.Should().NotBeEmpty();
        suggestions.Should().HaveCount(3);
        suggestions.Select(s => s.Step).Should().Contain("UseDraft");
        suggestions.Select(s => s.Step).Should().Contain("UseCritique");
        suggestions.Select(s => s.Step).Should().Contain("UseImprove");
    }

    [Fact]
    public async Task SuggestNextSteps_EmptyDsl_ReturnsEmptyList()
    {
        // Act
        var suggestions = await DslAssistant.SuggestNextSteps("");

        // Assert
        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestNextSteps_WhitespaceDsl_ReturnsEmptyList()
    {
        // Act
        var suggestions = await DslAssistant.SuggestNextSteps("   ");

        // Assert
        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestNextSteps_ConfidenceScoresAreValid()
    {
        // Act
        var suggestions = await DslAssistant.SuggestNextSteps("UseDraft");

        // Assert
        foreach (var suggestion in suggestions)
        {
            suggestion.Confidence.Should().BeGreaterThanOrEqualTo(0.0);
            suggestion.Confidence.Should().BeLessThanOrEqualTo(1.0);
        }
    }

    #endregion

    #region GetTokenCompletions Tests

    [Fact]
    public async Task GetTokenCompletions_PartialToken_ReturnsMatches()
    {
        // Act
        var completions = await DslAssistant.GetTokenCompletions("Set");

        // Assert
        completions.Should().Contain("SetTopic");
        completions.Should().Contain("SetPrompt");
    }

    [Fact]
    public async Task GetTokenCompletions_UsePrefix_ReturnsUseTokens()
    {
        // Act
        var completions = await DslAssistant.GetTokenCompletions("Use");

        // Assert
        completions.Should().Contain("UseDraft");
        completions.Should().Contain("UseCritique");
        completions.Should().Contain("UseImprove");
    }

    [Fact]
    public async Task GetTokenCompletions_EmptyInput_ReturnsEmptyList()
    {
        // Act
        var completions = await DslAssistant.GetTokenCompletions("");

        // Assert
        completions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTokenCompletions_NoMatch_ReturnsEmptyList()
    {
        // Act
        var completions = await DslAssistant.GetTokenCompletions("Zzz");

        // Assert
        completions.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTokenCompletions_CaseInsensitive_ReturnsMatches()
    {
        // Act
        var completions = await DslAssistant.GetTokenCompletions("set");

        // Assert
        completions.Should().Contain("SetTopic");
    }

    #endregion

    #region ValidateDsl Tests

    [Fact]
    public void ValidateDsl_ValidPipeline_ReturnsValid()
    {
        // Act
        var result = DslAssistant.ValidateDsl("SetTopic | UseDraft | UseCritique");

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateDsl_EmptyString_ReturnsInvalid()
    {
        // Act
        var result = DslAssistant.ValidateDsl("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("DSL cannot be empty");
    }

    [Fact]
    public void ValidateDsl_UnknownToken_ReturnsError()
    {
        // Act
        var result = DslAssistant.ValidateDsl("UnknownToken");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Unknown token"));
    }

    [Fact]
    public void ValidateDsl_MixedValidAndInvalidTokens_ReportsInvalidOnes()
    {
        // Act
        var result = DslAssistant.ValidateDsl("SetTopic | InvalidStep | UseDraft");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("InvalidStep"));
    }

    [Fact]
    public void ValidateDsl_TokenWithParentheses_RecognizesBaseToken()
    {
        // Act
        var result = DslAssistant.ValidateDsl("SetTopic(Test)");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ExplainPipeline Tests

    [Fact]
    public void ExplainPipeline_EmptyPipeline_ReturnsEmptyMessage()
    {
        // Act
        var explanation = DslAssistant.ExplainPipeline("");

        // Assert
        explanation.Should().Be("Empty pipeline");
    }

    [Fact]
    public void ExplainPipeline_SetTopicToken_DescribesCorrectly()
    {
        // Act
        var explanation = DslAssistant.ExplainPipeline("SetTopic");

        // Assert
        explanation.Should().Contain("topic");
    }

    [Fact]
    public void ExplainPipeline_UseDraftToken_DescribesCorrectly()
    {
        // Act
        var explanation = DslAssistant.ExplainPipeline("UseDraft");

        // Assert
        explanation.Should().Contain("draft");
    }

    [Fact]
    public void ExplainPipeline_MultistepPipeline_DescribesAllSteps()
    {
        // Act
        var explanation = DslAssistant.ExplainPipeline("SetTopic | UseDraft | UseCritique | UseImprove");

        // Assert
        explanation.Should().Contain("topic");
        explanation.Should().Contain("draft");
        explanation.Should().Contain("critiqu");
        explanation.Should().Contain("improv");
    }

    [Fact]
    public void ExplainPipeline_UnknownToken_DescribesExecution()
    {
        // Act
        var explanation = DslAssistant.ExplainPipeline("CustomStep");

        // Assert
        explanation.Should().Contain("executes customstep");
    }

    #endregion

    #region GenerateDslFromGoal Tests

    [Fact]
    public void GenerateDslFromGoal_EmptyGoal_ReturnsEmpty()
    {
        // Act
        var dsl = DslAssistant.GenerateDslFromGoal("");

        // Assert
        dsl.Should().BeEmpty();
    }

    [Fact]
    public void GenerateDslFromGoal_AnalyzeGoal_ReturnsAnalysisPipeline()
    {
        // Act
        var dsl = DslAssistant.GenerateDslFromGoal("I want to analyze code");

        // Assert
        dsl.Should().Contain("UseDraft");
        dsl.Should().Contain("UseCritique");
        dsl.Should().Contain("UseImprove");
    }

    [Fact]
    public void GenerateDslFromGoal_QualityGoal_ReturnsAnalysisPipeline()
    {
        // Act
        var dsl = DslAssistant.GenerateDslFromGoal("Check code quality");

        // Assert
        dsl.Should().Contain("UseCritique");
    }

    [Fact]
    public void GenerateDslFromGoal_GenericGoal_ReturnsBasicPipeline()
    {
        // Act
        var dsl = DslAssistant.GenerateDslFromGoal("Write some code");

        // Assert
        dsl.Should().Contain("SetTopic");
        dsl.Should().Contain("UseDraft");
    }

    #endregion

    #region BuildDsl Tests

    [Fact]
    public void BuildDsl_WithTopic_IncludesTopicAndFullPipeline()
    {
        // Act
        var dsl = DslAssistant.BuildDsl("My Topic");

        // Assert
        dsl.Should().Contain("SetTopic('My Topic')");
        dsl.Should().Contain("UseDraft");
        dsl.Should().Contain("UseCritique");
        dsl.Should().Contain("UseImprove");
    }

    #endregion

    #region SuggestImprovements Tests

    [Fact]
    public void SuggestImprovements_MissingCritique_AddsCritique()
    {
        // Act
        var improved = DslAssistant.SuggestImprovements("SetTopic | UseDraft");

        // Assert
        improved.Should().Contain("UseCritique");
    }

    [Fact]
    public void SuggestImprovements_MissingImprove_AddsImprove()
    {
        // Act
        var improved = DslAssistant.SuggestImprovements("SetTopic | UseDraft | UseCritique");

        // Assert
        improved.Should().Contain("UseImprove");
    }

    [Fact]
    public void SuggestImprovements_Complete_ReturnsSame()
    {
        // Arrange
        var dsl = "SetTopic | UseDraft | UseCritique | UseImprove";

        // Act
        var improved = DslAssistant.SuggestImprovements(dsl);

        // Assert
        improved.Should().Be(dsl);
    }

    #endregion

    #region GenerateCode Tests

    [Fact]
    public void GenerateCode_ResultMonad_ReturnsResultClass()
    {
        // Act
        var code = DslAssistant.GenerateCode("Generate a Result<T> monad");

        // Assert
        code.Should().Contain("class Result<T>");
        code.Should().Contain("IsSuccess");
    }

    [Fact]
    public void GenerateCode_UnknownDescription_ReturnsPlaceholder()
    {
        // Act
        var code = DslAssistant.GenerateCode("something else");

        // Assert
        code.Should().Contain("placeholder");
    }

    #endregion

    #region ExplainCode Tests

    [Fact]
    public void ExplainCode_ReturnsExplanation()
    {
        // Act
        var explanation = DslAssistant.ExplainCode("class MyClass { }");

        // Assert
        explanation.Should().NotBeNullOrEmpty();
        explanation.Should().Contain("Result monad");
    }

    #endregion

    #region ProcessCommandAsync Tests

    [Fact]
    public async Task ProcessCommandAsync_SuggestCommand_ReturnsSuggestions()
    {
        // Act
        var result = await DslAssistant.ProcessCommandAsync("suggest UseDraft");

        // Assert
        result.Should().Contain("Suggestions");
    }

    [Fact]
    public async Task ProcessCommandAsync_CompleteCommand_ReturnsCompletions()
    {
        // Act
        var result = await DslAssistant.ProcessCommandAsync("complete Set");

        // Assert
        result.Should().Contain("Completions");
    }

    [Fact]
    public async Task ProcessCommandAsync_HelpCommand_ReturnsAvailableCommands()
    {
        // Act
        var result = await DslAssistant.ProcessCommandAsync("help");

        // Assert
        result.Should().Contain("Available commands");
    }

    [Fact]
    public async Task ProcessCommandAsync_ExitCommand_ReturnsTerminated()
    {
        // Act
        var result = await DslAssistant.ProcessCommandAsync("exit");

        // Assert
        result.Should().Contain("terminated");
    }

    [Fact]
    public async Task ProcessCommandAsync_UnknownCommand_ReturnsUnknown()
    {
        // Act
        var result = await DslAssistant.ProcessCommandAsync("foobar");

        // Assert
        result.Should().Be("Unknown command");
    }

    #endregion

    #region ProcessCommand (Sync) Tests

    [Fact]
    public void ProcessCommand_Help_ReturnsAvailableCommands()
    {
        // Act
        var result = DslAssistant.ProcessCommand("help");

        // Assert
        result.Should().Contain("Available commands");
    }

    #endregion

    #region DslAssistant Constructor Test

    [Fact]
    public void Constructor_CanBeInstantiated()
    {
        // Act
        var assistant = new DslAssistant();

        // Assert
        assistant.Should().NotBeNull();
    }

    #endregion
}
