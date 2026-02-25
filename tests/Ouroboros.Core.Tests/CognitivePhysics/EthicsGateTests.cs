// <copyright file="EthicsGateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class EthicsGateTests
{
    [Fact]
    public void AllowResult_ShouldBeAllowed()
    {
        EthicsGateResult result = EthicsGateResult.Allow("ok");

        result.IsAllowed.Should().BeTrue();
        result.IsDenied.Should().BeFalse();
        result.IsUncertain.Should().BeFalse();
    }

    [Fact]
    public void DenyResult_ShouldBeDenied()
    {
        EthicsGateResult result = EthicsGateResult.Deny("forbidden");

        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeTrue();
        result.IsUncertain.Should().BeFalse();
    }

    [Fact]
    public void UncertainResult_ShouldBeUncertain()
    {
        EthicsGateResult result = EthicsGateResult.Uncertain("ambiguous");

        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeFalse();
        result.IsUncertain.Should().BeTrue();
    }

    [Fact]
    public async Task PermissiveGate_ShouldAlwaysAllow()
    {
        PermissiveEthicsGate gate = new();

        EthicsGateResult result = await gate.EvaluateAsync("anything", "anywhere");

        result.IsAllowed.Should().BeTrue();
    }
}
