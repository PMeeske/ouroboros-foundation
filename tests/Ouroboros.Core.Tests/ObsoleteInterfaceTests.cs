// <copyright file="ObsoleteInterfaceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.CognitivePhysics;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Tests;

/// <summary>
/// Tests for obsolete interfaces to ensure backward compatibility.
/// These interfaces should still compile but generate compiler warnings.
/// </summary>
[Trait("Category", "Unit")]
public class ObsoleteInterfaceTests
{
    [Fact]
    public void IEthicsGate_InterfaceIsAccessible()
    {
        // Verify the obsolete interface type exists and is accessible
        var interfaceType = typeof(IEthicsGate);
        
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.Namespace.Should().Be("Ouroboros.Core.CognitivePhysics");
    }

    [Fact]
    public void IEmbeddingProvider_InterfaceIsAccessible()
    {
        // Verify the obsolete interface type exists and is accessible
        var interfaceType = typeof(IEmbeddingProvider);
        
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.Namespace.Should().Be("Ouroboros.Core.CognitivePhysics");
    }

    [Fact]
    public void IVisionModel_InterfaceIsAccessible()
    {
        // Verify the obsolete interface type exists and is accessible
        var interfaceType = typeof(IVisionModel);
        
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.Namespace.Should().Be("Ouroboros.Core.EmbodiedInteraction");
    }

    [Fact]
    public void PermissiveEthicsGate_CanBeInstantiated()
    {
        // Verify that implementations of obsolete interfaces still work
        // This ensures backward compatibility
        
        var gate = new PermissiveEthicsGate();
        
        gate.Should().NotBeNull();
        gate.Should().BeAssignableTo<IEthicsGate>();
    }

    [Fact]
    public async Task PermissiveEthicsGate_FunctionsCorrectly()
    {
        // Verify that obsolete interface implementations still function
        
        var gate = new PermissiveEthicsGate();
        var result = await gate.EvaluateAsync("from", "to");
        
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void EthicsGateResult_CanBeCreated()
    {
        // Verify that types related to obsolete interfaces still work
        
        var result = EthicsGateResult.Allow("test reason");
        
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Reason.Should().Be("test reason");
    }
}
