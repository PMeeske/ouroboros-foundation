// <copyright file="ObsoleteInterfaceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.CognitivePhysics;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Tests;

/// <summary>
/// Tests for interfaces in the cognitive physics and embodied interaction subsystems.
/// </summary>
[Trait("Category", "Unit")]
public class ObsoleteInterfaceTests
{
    [Fact]
    public void IEthicsGate_InterfaceIsAccessible()
    {
        Type interfaceType = typeof(IEthicsGate);

        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.Namespace.Should().Be("Ouroboros.Core.CognitivePhysics");
    }

    [Fact]
    public void IVisionModel_InterfaceIsAccessible()
    {
        Type interfaceType = typeof(IVisionModel);

        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
        interfaceType.Namespace.Should().Be("Ouroboros.Core.EmbodiedInteraction");
    }

    [Fact]
    public void PermissiveEthicsGate_CanBeInstantiated()
    {
        PermissiveEthicsGate gate = new PermissiveEthicsGate();

        gate.Should().NotBeNull();
        gate.Should().BeAssignableTo<IEthicsGate>();
    }

    [Fact]
    public async Task PermissiveEthicsGate_FunctionsCorrectly()
    {
        PermissiveEthicsGate gate = new PermissiveEthicsGate();
        EthicsGateResult result = await gate.EvaluateAsync("from", "to");

        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void EthicsGateResult_CanBeCreated()
    {
        EthicsGateResult result = EthicsGateResult.Allow("test reason");

        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
        result.Reason.Should().Be("test reason");
    }
}
