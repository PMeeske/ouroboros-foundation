// <copyright file="BenchmarkSuite.Helpers.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Randomness;

namespace Ouroboros.Domain.Benchmarks;

/// <summary>
/// Private helper methods for the BenchmarkSuite.
/// </summary>
public sealed partial class BenchmarkSuite
{
    private static async Task<(bool success, double score, string? error, string difficulty, string patternType)> ExecuteARCTaskAsync(
        string taskId,
        CancellationToken ct)
    {
        await Task.Delay(10, ct);

        SeededRandomProvider random = new SeededRandomProvider(taskId.GetHashCode());
        string difficulty = random.Next(3) switch
        {
            0 => "easy",
            1 => "medium",
            _ => "hard",
        };

        string patternType = random.Next(4) switch
        {
            0 => "rotation",
            1 => "scaling",
            2 => "color_mapping",
            _ => "shape_transformation",
        };

        double baseScore = difficulty switch
        {
            "easy" => 0.7,
            "medium" => 0.4,
            _ => 0.15,
        };

        double score = Math.Max(0, Math.Min(1.0, baseScore + (random.NextDouble() * 0.2 - 0.1)));
        bool success = score > 0.5;

        return (success, score, success ? null : "Task failed", difficulty, patternType);
    }

    private static async Task<(bool success, double score, string? error, int questionCount)> ExecuteMMLUSubjectAsync(
        string subject,
        CancellationToken ct)
    {
        await Task.Delay(50, ct);

        SeededRandomProvider random = new SeededRandomProvider(subject.GetHashCode());
        int questionCount = random.Next(50, 100);
        double score = 0.65 + (random.NextDouble() * 0.2);

        return (true, score, null, questionCount);
    }

    private static async Task<(bool success, double retentionScore, string? error, double initialAccuracy, double finalAccuracy)> ExecuteContinualLearningSequenceAsync(
        TaskSequence sequence,
        CancellationToken ct)
    {
        await Task.Delay(100, ct);

        SeededRandomProvider random = new SeededRandomProvider(sequence.Name.GetHashCode());
        double initialAccuracy = 0.85 + (random.NextDouble() * 0.1);
        double finalAccuracy = sequence.MeasureRetention ? 0.75 + (random.NextDouble() * 0.1) : initialAccuracy;
        double retentionScore = finalAccuracy / initialAccuracy;

        return (true, retentionScore, null, initialAccuracy, finalAccuracy);
    }

    private static async Task<(List<TaskResult> tasks, string? error)> ExecuteCognitiveDimensionAsync(
        CognitiveDimension dimension,
        CancellationToken ct)
    {
        await Task.Delay(50, ct);

        List<TaskResult> tasks = new List<TaskResult>();
        SeededRandomProvider random = new SeededRandomProvider((int)dimension);

        int taskCount = 10;
        for (int i = 0; i < taskCount; i++)
        {
            double score = GetDimensionBaseScore(dimension) + (random.NextDouble() * 0.2 - 0.1);
            score = Math.Max(0, Math.Min(1.0, score));

            tasks.Add(new TaskResult(
                TaskId: $"{dimension}-task-{i}",
                TaskName: $"{dimension} Task {i}",
                Success: score > 0.5,
                Score: score,
                Duration: TimeSpan.FromMilliseconds(random.Next(50, 200)),
                ErrorMessage: null,
                Metadata: new Dictionary<string, object>
                {
                    ["dimension"] = dimension.ToString(),
                    ["category"] = GetDimensionCategory(dimension, i),
                }));
        }

        return (tasks, null);
    }

    private static double GetDimensionBaseScore(CognitiveDimension dimension)
    {
        return dimension switch
        {
            CognitiveDimension.Reasoning => 0.65,
            CognitiveDimension.Planning => 0.70,
            CognitiveDimension.Learning => 0.75,
            CognitiveDimension.Memory => 0.80,
            CognitiveDimension.Generalization => 0.60,
            CognitiveDimension.Creativity => 0.55,
            CognitiveDimension.SocialIntelligence => 0.50,
            _ => 0.50,
        };
    }

    private static string GetDimensionCategory(CognitiveDimension dimension, int taskIndex)
    {
        return dimension switch
        {
            CognitiveDimension.Reasoning => taskIndex % 2 == 0 ? "deductive" : "inductive",
            CognitiveDimension.Planning => taskIndex % 2 == 0 ? "short_term" : "long_term",
            CognitiveDimension.Memory => taskIndex % 2 == 0 ? "episodic" : "semantic",
            _ => "general",
        };
    }

    private static List<string> IdentifyStrengths(Dictionary<string, BenchmarkReport> results)
    {
        List<string> strengths = new List<string>();

        foreach ((string? name, BenchmarkReport? report) in results)
        {
            if (report.OverallScore >= 0.7)
            {
                strengths.Add($"Strong performance in {name} ({report.OverallScore:P1})");
            }
        }

        if (strengths.Count == 0)
        {
            strengths.Add("All benchmarks show room for improvement");
        }

        return strengths;
    }

    private static List<string> IdentifyWeaknesses(Dictionary<string, BenchmarkReport> results)
    {
        List<string> weaknesses = new List<string>();

        foreach ((string? name, BenchmarkReport? report) in results)
        {
            if (report.OverallScore < 0.5)
            {
                weaknesses.Add($"Below target performance in {name} ({report.OverallScore:P1})");
            }
        }

        if (weaknesses.Count == 0)
        {
            weaknesses.Add("No significant weaknesses identified");
        }

        return weaknesses;
    }

    private static List<string> GenerateRecommendations(List<string> strengths, List<string> weaknesses)
    {
        List<string> recommendations = new List<string>();

        if (weaknesses.Any(w => w.Contains("ARC")))
        {
            recommendations.Add("Focus on improving abstract reasoning and pattern recognition capabilities");
        }

        if (weaknesses.Any(w => w.Contains("MMLU")))
        {
            recommendations.Add("Enhance knowledge base and multi-domain understanding");
        }

        if (weaknesses.Any(w => w.Contains("Continual")))
        {
            recommendations.Add("Implement better memory consolidation to reduce catastrophic forgetting");
        }

        if (weaknesses.Any(w => w.Contains("Cognitive")))
        {
            recommendations.Add("Develop targeted training for specific cognitive dimensions");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Continue current training regimen and monitor for regressions");
        }

        return recommendations;
    }
}
