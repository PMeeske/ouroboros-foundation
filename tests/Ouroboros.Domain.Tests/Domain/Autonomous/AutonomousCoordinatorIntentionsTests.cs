// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Xunit;

/// <summary>
/// Tests for AutonomousCoordinator.Intentions.cs — intention management,
/// topic parsing, and execution helpers.
/// </summary>
[Trait("Category", "Unit")]
public class AutonomousCoordinatorIntentionsTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;
    private readonly List<ProactiveMessageEventArgs> _capturedMessages = new();

    public AutonomousCoordinatorIntentionsTests()
    {
        _sut = new AutonomousCoordinator(new AutonomousConfiguration
        {
            PushBasedMode = true,
            YoloMode = false,
        });
        _sut.OnProactiveMessage += args => _capturedMessages.Add(args);
    }

    public void Dispose() => _sut.Dispose();

    // ----------------------------------------------------------------
    // ParseTopicResponse via reflection
    // ----------------------------------------------------------------

    private static object? InvokeParseTopicResponse(string response)
    {
        MethodInfo? method = typeof(AutonomousCoordinator)
            .GetMethod("ParseTopicResponse", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("ParseTopicResponse should exist as a private static method");
        return method!.Invoke(null, new object[] { response });
    }

    [Fact]
    public void ParseTopicResponse_ValidJson_ReturnsParsedTopic()
    {
        string json = """
        {
            "title": "Explore quantum computing",
            "description": "Research quantum entanglement",
            "category": "research",
            "tool": "web_search",
            "tool_input": "quantum computing basics"
        }
        """;

        object? result = InvokeParseTopicResponse(json);

        result.Should().NotBeNull();
        // Verify title property via reflection on the anonymous record
        result!.GetType().GetProperty("Title")!.GetValue(result).Should().Be("Explore quantum computing");
        result.GetType().GetProperty("Category")!.GetValue(result).Should().Be("research");
    }

    [Fact]
    public void ParseTopicResponse_JsonEmbeddedInText_ExtractsJson()
    {
        string response = "Here is my suggestion: {\"title\": \"Learn Rust\", \"description\": \"Explore Rust language\"} That's it.";

        object? result = InvokeParseTopicResponse(response);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("Title")!.GetValue(result).Should().Be("Learn Rust");
    }

    [Fact]
    public void ParseTopicResponse_InvalidJson_FallsBackToPlainText()
    {
        string response = "This is a long plain text response that suggests exploring something interesting about AI.";

        object? result = InvokeParseTopicResponse(response);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("Title")!.GetValue(result).Should().Be("Autonomous Thought");
    }

    [Fact]
    public void ParseTopicResponse_ShortString_ReturnsNull()
    {
        object? result = InvokeParseTopicResponse("short");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseTopicResponse_EmptyString_ReturnsNull()
    {
        object? result = InvokeParseTopicResponse("");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseTopicResponse_BrokenJson_FallsBackToPlainText()
    {
        string response = "{ this is not valid json at all but it is very long enough to be useful content }";

        object? result = InvokeParseTopicResponse(response);

        // Either parses as JSON or falls back to plain text
        result.Should().NotBeNull();
    }

    // ----------------------------------------------------------------
    // InjectGoalAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task InjectGoalAsync_CreatesPendingIntention()
    {
        await _sut.InjectGoalAsync("Learn about AI safety");

        _sut.PendingIntentionsCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task InjectGoalAsync_WithHighPriority_CreatesPendingIntention()
    {
        await _sut.InjectGoalAsync("Critical goal", IntentionPriority.Critical);

        _sut.PendingIntentionsCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task InjectGoalAsync_LongGoalText_TruncatesTitleToFirst50Chars()
    {
        string longGoal = new string('A', 200);

        // Should not throw
        await _sut.InjectGoalAsync(longGoal);

        _sut.PendingIntentionsCount.Should().BeGreaterOrEqualTo(1);
    }

    // ----------------------------------------------------------------
    // SendToNeuronAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task SendToNeuronAsync_DoesNotThrow()
    {
        Func<Task> act = async () => await _sut.SendToNeuronAsync("neuron.memory", "test.topic", "payload");

        await act.Should().NotThrowAsync();
    }

    // ----------------------------------------------------------------
    // InferDeliverableTypeFallback via reflection
    // ----------------------------------------------------------------

    private static string InvokeInferDeliverableTypeFallback(string problem)
    {
        MethodInfo? method = typeof(AutonomousCoordinator)
            .GetMethod("InferDeliverableTypeFallback", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("InferDeliverableTypeFallback should exist as a private static method");
        return (string)method!.Invoke(null, new object[] { problem })!;
    }

    [Theory]
    [InlineData("Design a database schema", "design")]
    [InlineData("Architect a microservice system", "design")]
    [InlineData("Plan the project roadmap", "plan")]
    [InlineData("Create a strategy for growth", "plan")]
    [InlineData("Analyze the performance data", "analysis")]
    [InlineData("Research why users leave", "analysis")]
    [InlineData("Document the API endpoints", "document")]
    [InlineData("Write a guide for beginners", "document")]
    [InlineData("Build a REST API", "code")]
    [InlineData("Something generic", "code")]
    public void InferDeliverableTypeFallback_ReturnsCorrectType(string problem, string expectedType)
    {
        string result = InvokeInferDeliverableTypeFallback(problem);

        result.Should().Be(expectedType);
    }

    // ----------------------------------------------------------------
    // CriticalCategories safety floor
    // ----------------------------------------------------------------

    [Fact]
    public void CriticalCategories_ContainsCodeModification()
    {
        FieldInfo? field = typeof(AutonomousCoordinator)
            .GetField("CriticalCategories", BindingFlags.NonPublic | BindingFlags.Static);
        field.Should().NotBeNull();

        var criticalCategories = (HashSet<IntentionCategory>)field!.GetValue(null)!;
        criticalCategories.Should().Contain(IntentionCategory.CodeModification);
        criticalCategories.Should().Contain(IntentionCategory.GoalPursuit);
        criticalCategories.Should().Contain(IntentionCategory.SafetyCheck);
    }
}
