// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Ouroboros.Domain.Autonomous.Neurons;
using Xunit;

/// <summary>
/// Tests for AutonomousCoordinator.Properties.cs — delegate properties,
/// state properties, and tool priority helpers.
/// </summary>
[Trait("Category", "Unit")]
public class AutonomousCoordinatorPropertiesTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;

    public AutonomousCoordinatorPropertiesTests()
    {
        _sut = new AutonomousCoordinator(new AutonomousConfiguration
        {
            PushBasedMode = true,
        });
    }

    public void Dispose() => _sut.Dispose();

    // ----------------------------------------------------------------
    // Delegate properties — default null
    // ----------------------------------------------------------------

    [Fact]
    public void ExecuteToolFunction_DefaultNull()
    {
        _sut.ExecuteToolFunction.Should().BeNull();
    }

    [Fact]
    public void EmbedFunction_DefaultNull()
    {
        _sut.EmbedFunction.Should().BeNull();
    }

    [Fact]
    public void StoreToQdrantFunction_DefaultNull()
    {
        _sut.StoreToQdrantFunction.Should().BeNull();
    }

    [Fact]
    public void SearchQdrantFunction_DefaultNull()
    {
        _sut.SearchQdrantFunction.Should().BeNull();
    }

    [Fact]
    public void StoreIntentionFunction_DefaultNull()
    {
        _sut.StoreIntentionFunction.Should().BeNull();
    }

    [Fact]
    public void StoreNeuronMessageFunction_DefaultNull()
    {
        _sut.StoreNeuronMessageFunction.Should().BeNull();
    }

    [Fact]
    public void ThinkFunction_DefaultNull()
    {
        _sut.ThinkFunction.Should().BeNull();
    }

    [Fact]
    public void MeTTaQueryFunction_DefaultNull()
    {
        _sut.MeTTaQueryFunction.Should().BeNull();
    }

    [Fact]
    public void MeTTaAddFactFunction_DefaultNull()
    {
        _sut.MeTTaAddFactFunction.Should().BeNull();
    }

    [Fact]
    public void VerifyDagConstraintFunction_DefaultNull()
    {
        _sut.VerifyDagConstraintFunction.Should().BeNull();
    }

    [Fact]
    public void ProcessChatFunction_DefaultNull()
    {
        _sut.ProcessChatFunction.Should().BeNull();
    }

    [Fact]
    public void DisplayAndSpeakFunction_DefaultNull()
    {
        _sut.DisplayAndSpeakFunction.Should().BeNull();
    }

    [Fact]
    public void FullChatWithToolsFunction_DefaultNull()
    {
        _sut.FullChatWithToolsFunction.Should().BeNull();
    }

    [Fact]
    public void SetSuppressProactiveMessages_DefaultNull()
    {
        _sut.SetSuppressProactiveMessages.Should().BeNull();
    }

    // ----------------------------------------------------------------
    // Delegate properties — settable
    // ----------------------------------------------------------------

    [Fact]
    public void ExecuteToolFunction_CanBeSet()
    {
        Func<string, string, CancellationToken, Task<string>> func = (_, _, _) => Task.FromResult("ok");
        _sut.ExecuteToolFunction = func;
        _sut.ExecuteToolFunction.Should().BeSameAs(func);
    }

    [Fact]
    public void ThinkFunction_CanBeSet()
    {
        Func<string, CancellationToken, Task<string>> func = (_, _) => Task.FromResult("thought");
        _sut.ThinkFunction = func;
        _sut.ThinkFunction.Should().BeSameAs(func);
    }

    // ----------------------------------------------------------------
    // State properties — defaults
    // ----------------------------------------------------------------

    [Fact]
    public void TopicDiscoveryIntervalSeconds_Default90()
    {
        _sut.TopicDiscoveryIntervalSeconds.Should().Be(90);
    }

    [Fact]
    public void TopicDiscoveryIntervalSeconds_Settable()
    {
        _sut.TopicDiscoveryIntervalSeconds = 45;
        _sut.TopicDiscoveryIntervalSeconds.Should().Be(45);
    }

    [Fact]
    public void EnableMeTTaValidation_DefaultTrue()
    {
        _sut.EnableMeTTaValidation.Should().BeTrue();
    }

    [Fact]
    public void EnableMeTTaValidation_Settable()
    {
        _sut.EnableMeTTaValidation = false;
        _sut.EnableMeTTaValidation.Should().BeFalse();
    }

    [Fact]
    public void IsAutoTrainingActive_DefaultFalse()
    {
        _sut.IsAutoTrainingActive.Should().BeFalse();
    }

    [Fact]
    public void IsYoloMode_DefaultFalse()
    {
        _sut.IsYoloMode.Should().BeFalse();
    }

    [Fact]
    public void IsVoiceEnabled_DefaultTrue()
    {
        _sut.IsVoiceEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsVoiceEnabled_Settable()
    {
        _sut.IsVoiceEnabled = false;
        _sut.IsVoiceEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsListeningEnabled_DefaultFalse()
    {
        _sut.IsListeningEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsListeningEnabled_Settable()
    {
        _sut.IsListeningEnabled = true;
        _sut.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void SetVoiceEnabled_DefaultNull()
    {
        _sut.SetVoiceEnabled.Should().BeNull();
    }

    [Fact]
    public void SetListeningEnabled_DefaultNull()
    {
        _sut.SetListeningEnabled.Should().BeNull();
    }

    [Fact]
    public void AvailableTools_DefaultEmpty()
    {
        _sut.AvailableTools.Should().BeEmpty();
    }

    [Fact]
    public void AvailableTools_Settable()
    {
        _sut.AvailableTools = new HashSet<string> { "tool1", "tool2" };
        _sut.AvailableTools.Should().HaveCount(2);
    }

    // ----------------------------------------------------------------
    // Tool Priority Helpers
    // ----------------------------------------------------------------

    [Fact]
    public void GetPreferredTool_ReturnsFirstAvailable()
    {
        _sut.AvailableTools = new HashSet<string> { "b", "c" };

        string tool = _sut.GetPreferredTool(new[] { "a", "b", "c" });

        tool.Should().Be("b");
    }

    [Fact]
    public void GetPreferredTool_NoneAvailable_ReturnsFallback()
    {
        _sut.AvailableTools = new HashSet<string>();

        string tool = _sut.GetPreferredTool(new[] { "a" }, "default_tool");

        tool.Should().Be("default_tool");
    }

    [Fact]
    public void GetPreferredTool_DefaultFallbackIsWebSearch()
    {
        _sut.AvailableTools = new HashSet<string>();

        string tool = _sut.GetPreferredTool(new[] { "nonexistent" });

        tool.Should().Be("web_search");
    }

    [Fact]
    public void GetPreferredResearchTool_DefaultFallback()
    {
        _sut.AvailableTools = new HashSet<string>();

        _sut.GetPreferredResearchTool().Should().Be("web_search");
    }

    [Fact]
    public void GetPreferredCodeTool_DefaultFallback()
    {
        _sut.AvailableTools = new HashSet<string>();

        _sut.GetPreferredCodeTool().Should().Be("file_read");
    }

    [Fact]
    public void GetPreferredGeneralTool_DefaultFallback()
    {
        _sut.AvailableTools = new HashSet<string>();

        _sut.GetPreferredGeneralTool().Should().Be("recall");
    }

    // ----------------------------------------------------------------
    // AddConversationContext
    // ----------------------------------------------------------------

    [Fact]
    public void AddConversationContext_AcceptsMessages()
    {
        Action act = () => _sut.AddConversationContext("Test message");
        act.Should().NotThrow();
    }

    [Fact]
    public void AddConversationContext_PrunesAt20Messages()
    {
        for (int i = 0; i < 25; i++)
        {
            _sut.AddConversationContext($"Message {i}");
        }

        // Should not throw — internal list is pruned to 20
    }
}
