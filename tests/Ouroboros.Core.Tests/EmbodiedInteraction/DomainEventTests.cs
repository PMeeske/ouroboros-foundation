// <copyright file="DomainEventTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class DomainEventTests
{
    // -- EmbodimentDomainEvent --

    [Fact]
    public void EmbodimentDomainEvent_ShouldInitializeAllProperties()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var details = new Dictionary<string, object> { ["provider"] = "camera-1" };

        // Act
        var evt = new EmbodimentDomainEvent(
            EmbodimentDomainEventType.ProviderConnected, ts, details);

        // Assert
        evt.EventType.Should().Be(EmbodimentDomainEventType.ProviderConnected);
        evt.Timestamp.Should().Be(ts);
        evt.Details.Should().ContainKey("provider");
    }

    [Fact]
    public void EmbodimentDomainEvent_DefaultDetails_ShouldBeNull()
    {
        // Act
        var evt = new EmbodimentDomainEvent(
            EmbodimentDomainEventType.AggregateActivated, DateTime.UtcNow);

        // Assert
        evt.Details.Should().BeNull();
    }

    [Fact]
    public void EmbodimentDomainEvent_RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var a = new EmbodimentDomainEvent(EmbodimentDomainEventType.StateChanged, ts);
        var b = new EmbodimentDomainEvent(EmbodimentDomainEventType.StateChanged, ts);

        // Act & Assert
        a.Should().Be(b);
    }

    // -- EmbodimentProviderEvent --

    [Fact]
    public void EmbodimentProviderEvent_ShouldInitializeAllProperties()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var details = new Dictionary<string, object> { ["reason"] = "network error" };

        // Act
        var evt = new EmbodimentProviderEvent(
            EmbodimentProviderEventType.Error, ts, details);

        // Assert
        evt.EventType.Should().Be(EmbodimentProviderEventType.Error);
        evt.Timestamp.Should().Be(ts);
        evt.Details.Should().ContainKey("reason");
    }

    [Fact]
    public void EmbodimentProviderEvent_DefaultDetails_ShouldBeNull()
    {
        // Act
        var evt = new EmbodimentProviderEvent(
            EmbodimentProviderEventType.Connected, DateTime.UtcNow);

        // Assert
        evt.Details.Should().BeNull();
    }

    // -- Limitation --

    [Fact]
    public void Limitation_ShouldInitializeAllProperties()
    {
        // Act
        var limitation = new Limitation(LimitationType.MemoryBounded, "Limited context", 0.7);

        // Assert
        limitation.Type.Should().Be(LimitationType.MemoryBounded);
        limitation.Description.Should().Be("Limited context");
        limitation.Severity.Should().Be(0.7);
    }

    [Fact]
    public void Limitation_DefaultSeverity_ShouldBeHalf()
    {
        // Act
        var limitation = new Limitation(LimitationType.KnowledgeGap, "Outdated knowledge");

        // Assert
        limitation.Severity.Should().Be(0.5);
    }

    [Fact]
    public void Limitation_RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var a = new Limitation(LimitationType.ProcessingTime, "Slow", 0.3);
        var b = new Limitation(LimitationType.ProcessingTime, "Slow", 0.3);

        // Act & Assert
        a.Should().Be(b);
    }
}
