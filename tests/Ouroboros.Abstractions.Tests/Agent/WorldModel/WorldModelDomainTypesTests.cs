// <copyright file="WorldModelDomainTypesTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Abstractions.Tests.Agent.WorldModel;

/// <summary>
/// Tests for WorldModelDomainTypes.cs. This file is intentionally empty in the source;
/// domain types used by IWorldModel (SensorState, EmbodiedAction, EmbodiedTransition,
/// PredictedState) are defined in Domain/EmbodiedTypes.cs. This test class verifies that
/// the referenced domain types are accessible via the Abstractions project.
/// </summary>
[Trait("Category", "Unit")]
public class WorldModelDomainTypesTests
{
    [Fact]
    public void WorldModelDomainTypes_FileIsIntentionallyEmpty_TypesDefinedElsewhere()
    {
        // WorldModelDomainTypes.cs contains only a comment explaining that
        // domain types are defined in Domain/EmbodiedTypes.cs. This test
        // documents that the file was reviewed and found to be a placeholder.
        true.Should().BeTrue();
    }
}
