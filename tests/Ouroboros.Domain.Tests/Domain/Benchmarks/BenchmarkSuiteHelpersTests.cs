// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Benchmarks;

using System.Reflection;
using Ouroboros.Domain.Benchmarks;

/// <summary>
/// Tests for the private helper methods in BenchmarkSuite.Helpers.cs.
/// Uses reflection to test private/static methods.
/// </summary>
[Trait("Category", "Unit")]
public class BenchmarkSuiteHelpersTests
{
    // ----------------------------------------------------------------
    // IdentifyStrengths (static private -> use reflection)
    // ----------------------------------------------------------------

    [Fact]
    public void IdentifyStrengths_HighScores_ReturnsStrengthDescriptions()
    {
        // Arrange
        var results = new Dictionary<string, BenchmarkReport>
        {
            ["ARC"] = CreateReport(0.8),
            ["MMLU"] = CreateReport(0.75),
        };

        // Act
        List<string> strengths = InvokeIdentifyStrengths(results);

        // Assert
        strengths.Should().HaveCount(2);
        strengths.Should().Contain(s => s.Contains("ARC"));
        strengths.Should().Contain(s => s.Contains("MMLU"));
    }

    [Fact]
    public void IdentifyStrengths_NoHighScores_ReturnsFallbackMessage()
    {
        // Arrange
        var results = new Dictionary<string, BenchmarkReport>
        {
            ["ARC"] = CreateReport(0.3),
            ["MMLU"] = CreateReport(0.4),
        };

        // Act
        List<string> strengths = InvokeIdentifyStrengths(results);

        // Assert
        strengths.Should().HaveCount(1);
        strengths[0].Should().Contain("room for improvement");
    }

    // ----------------------------------------------------------------
    // IdentifyWeaknesses (static private -> use reflection)
    // ----------------------------------------------------------------

    [Fact]
    public void IdentifyWeaknesses_LowScores_ReturnsWeaknessDescriptions()
    {
        // Arrange
        var results = new Dictionary<string, BenchmarkReport>
        {
            ["ARC"] = CreateReport(0.3),
            ["MMLU"] = CreateReport(0.45),
        };

        // Act
        List<string> weaknesses = InvokeIdentifyWeaknesses(results);

        // Assert
        weaknesses.Should().HaveCount(2);
        weaknesses.Should().Contain(w => w.Contains("ARC"));
        weaknesses.Should().Contain(w => w.Contains("MMLU"));
    }

    [Fact]
    public void IdentifyWeaknesses_NoLowScores_ReturnsFallbackMessage()
    {
        // Arrange
        var results = new Dictionary<string, BenchmarkReport>
        {
            ["ARC"] = CreateReport(0.8),
            ["MMLU"] = CreateReport(0.9),
        };

        // Act
        List<string> weaknesses = InvokeIdentifyWeaknesses(results);

        // Assert
        weaknesses.Should().HaveCount(1);
        weaknesses[0].Should().Contain("No significant weaknesses");
    }

    // ----------------------------------------------------------------
    // GenerateRecommendations (static private -> use reflection)
    // ----------------------------------------------------------------

    [Fact]
    public void GenerateRecommendations_ArcWeakness_RecommendsReasoningImprovement()
    {
        // Arrange
        var strengths = new List<string>();
        var weaknesses = new List<string> { "Below target performance in ARC" };

        // Act
        List<string> recs = InvokeGenerateRecommendations(strengths, weaknesses);

        // Assert
        recs.Should().Contain(r => r.Contains("abstract reasoning"));
    }

    [Fact]
    public void GenerateRecommendations_MmluWeakness_RecommendsKnowledgeBase()
    {
        // Arrange
        var strengths = new List<string>();
        var weaknesses = new List<string> { "Below target in MMLU" };

        // Act
        List<string> recs = InvokeGenerateRecommendations(strengths, weaknesses);

        // Assert
        recs.Should().Contain(r => r.Contains("knowledge base"));
    }

    [Fact]
    public void GenerateRecommendations_ContinualWeakness_RecommendsMemoryConsolidation()
    {
        // Arrange
        var strengths = new List<string>();
        var weaknesses = new List<string> { "Continual learning deficit" };

        // Act
        List<string> recs = InvokeGenerateRecommendations(strengths, weaknesses);

        // Assert
        recs.Should().Contain(r => r.Contains("memory consolidation"));
    }

    [Fact]
    public void GenerateRecommendations_CognitiveWeakness_RecommendsCognitiveTraining()
    {
        // Arrange
        var strengths = new List<string>();
        var weaknesses = new List<string> { "Cognitive dimension issues" };

        // Act
        List<string> recs = InvokeGenerateRecommendations(strengths, weaknesses);

        // Assert
        recs.Should().Contain(r => r.Contains("cognitive dimensions"));
    }

    [Fact]
    public void GenerateRecommendations_NoWeaknesses_RecommendsContinueTraining()
    {
        // Arrange
        var strengths = new List<string> { "Strong performance" };
        var weaknesses = new List<string> { "No significant weaknesses" };

        // Act
        List<string> recs = InvokeGenerateRecommendations(strengths, weaknesses);

        // Assert
        recs.Should().Contain(r => r.Contains("Continue current"));
    }

    // ----------------------------------------------------------------
    // GetDimensionBaseScore
    // ----------------------------------------------------------------

    [Theory]
    [InlineData(CognitiveDimension.Reasoning, 0.65)]
    [InlineData(CognitiveDimension.Planning, 0.70)]
    [InlineData(CognitiveDimension.Learning, 0.75)]
    [InlineData(CognitiveDimension.Memory, 0.80)]
    [InlineData(CognitiveDimension.Generalization, 0.60)]
    [InlineData(CognitiveDimension.Creativity, 0.55)]
    [InlineData(CognitiveDimension.SocialIntelligence, 0.50)]
    public void GetDimensionBaseScore_ReturnsExpectedScore(CognitiveDimension dimension, double expected)
    {
        // Act
        double score = InvokeGetDimensionBaseScore(dimension);

        // Assert
        score.Should().Be(expected);
    }

    // ----------------------------------------------------------------
    // GetDimensionCategory
    // ----------------------------------------------------------------

    [Fact]
    public void GetDimensionCategory_Reasoning_EvenIndex_ReturnsDeductive()
    {
        string category = InvokeGetDimensionCategory(CognitiveDimension.Reasoning, 0);
        category.Should().Be("deductive");
    }

    [Fact]
    public void GetDimensionCategory_Reasoning_OddIndex_ReturnsInductive()
    {
        string category = InvokeGetDimensionCategory(CognitiveDimension.Reasoning, 1);
        category.Should().Be("inductive");
    }

    [Fact]
    public void GetDimensionCategory_Planning_EvenIndex_ReturnsShortTerm()
    {
        string category = InvokeGetDimensionCategory(CognitiveDimension.Planning, 0);
        category.Should().Be("short_term");
    }

    [Fact]
    public void GetDimensionCategory_Memory_OddIndex_ReturnsSemantic()
    {
        string category = InvokeGetDimensionCategory(CognitiveDimension.Memory, 1);
        category.Should().Be("semantic");
    }

    [Fact]
    public void GetDimensionCategory_Other_ReturnsGeneral()
    {
        string category = InvokeGetDimensionCategory(CognitiveDimension.Creativity, 0);
        category.Should().Be("general");
    }

    // ----------------------------------------------------------------
    // Helpers for reflection-based invocation
    // ----------------------------------------------------------------

    private static BenchmarkReport CreateReport(double overallScore)
    {
        return new BenchmarkReport
        {
            OverallScore = overallScore,
            TaskResults = new List<TaskResult>(),
        };
    }

    private static List<string> InvokeIdentifyStrengths(Dictionary<string, BenchmarkReport> results)
    {
        MethodInfo? method = typeof(BenchmarkSuite).GetMethod(
            "IdentifyStrengths", BindingFlags.Static | BindingFlags.NonPublic);
        return (List<string>)method!.Invoke(null, new object[] { results })!;
    }

    private static List<string> InvokeIdentifyWeaknesses(Dictionary<string, BenchmarkReport> results)
    {
        MethodInfo? method = typeof(BenchmarkSuite).GetMethod(
            "IdentifyWeaknesses", BindingFlags.Static | BindingFlags.NonPublic);
        return (List<string>)method!.Invoke(null, new object[] { results })!;
    }

    private static List<string> InvokeGenerateRecommendations(List<string> strengths, List<string> weaknesses)
    {
        MethodInfo? method = typeof(BenchmarkSuite).GetMethod(
            "GenerateRecommendations", BindingFlags.Static | BindingFlags.NonPublic);
        return (List<string>)method!.Invoke(null, new object[] { strengths, weaknesses })!;
    }

    private static double InvokeGetDimensionBaseScore(CognitiveDimension dimension)
    {
        MethodInfo? method = typeof(BenchmarkSuite).GetMethod(
            "GetDimensionBaseScore", BindingFlags.Static | BindingFlags.NonPublic);
        return (double)method!.Invoke(null, new object[] { dimension })!;
    }

    private static string InvokeGetDimensionCategory(CognitiveDimension dimension, int taskIndex)
    {
        MethodInfo? method = typeof(BenchmarkSuite).GetMethod(
            "GetDimensionCategory", BindingFlags.Static | BindingFlags.NonPublic);
        return (string)method!.Invoke(null, new object[] { dimension, taskIndex })!;
    }
}
