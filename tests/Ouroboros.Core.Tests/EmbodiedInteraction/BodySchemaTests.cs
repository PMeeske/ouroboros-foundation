// <copyright file="BodySchemaTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class BodySchemaTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateEmptySchema()
    {
        // Act
        var schema = new BodySchema();

        // Assert
        schema.Sensors.Should().BeEmpty();
        schema.Actuators.Should().BeEmpty();
        schema.Capabilities.Should().BeEmpty();
        schema.Limitations.Should().BeEmpty();
    }

    [Fact]
    public void WithSensor_ShouldAddSensorAndCapabilities()
    {
        // Arrange
        var schema = new BodySchema();
        var sensor = SensorDescriptor.Audio("mic-1");

        // Act
        var updated = schema.WithSensor(sensor);

        // Assert
        updated.Sensors.Should().ContainKey("mic-1");
        updated.Capabilities.Should().Contain(Capability.Hearing);
        schema.Sensors.Should().BeEmpty("original should be immutable");
    }

    [Fact]
    public void WithActuator_ShouldAddActuatorAndCapabilities()
    {
        // Arrange
        var schema = new BodySchema();
        var actuator = ActuatorDescriptor.Voice("voice-1");

        // Act
        var updated = schema.WithActuator(actuator);

        // Assert
        updated.Actuators.Should().ContainKey("voice-1");
        updated.Capabilities.Should().Contain(Capability.Speaking);
    }

    [Fact]
    public void WithCapability_ShouldAddCapability()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var updated = schema.WithCapability(Capability.Reasoning);

        // Assert
        updated.Capabilities.Should().Contain(Capability.Reasoning);
    }

    [Fact]
    public void WithLimitation_ShouldAddLimitation()
    {
        // Arrange
        var schema = new BodySchema();
        var limitation = new Limitation(LimitationType.MemoryBounded, "Limited context");

        // Act
        var updated = schema.WithLimitation(limitation);

        // Assert
        updated.Limitations.Should().ContainSingle()
            .Which.Type.Should().Be(LimitationType.MemoryBounded);
    }

    [Fact]
    public void WithoutSensor_ExistingSensor_ShouldRemoveIt()
    {
        // Arrange
        var schema = new BodySchema()
            .WithSensor(SensorDescriptor.Audio("mic-1"))
            .WithSensor(SensorDescriptor.Visual("cam-1"));

        // Act
        var updated = schema.WithoutSensor("mic-1");

        // Assert
        updated.Sensors.Should().NotContainKey("mic-1");
        updated.Sensors.Should().ContainKey("cam-1");
    }

    [Fact]
    public void WithoutSensor_NonExistentSensor_ShouldReturnSameInstance()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var updated = schema.WithoutSensor("nonexistent");

        // Assert
        updated.Should().BeSameAs(schema);
    }

    [Fact]
    public void WithoutActuator_ExistingActuator_ShouldRemoveIt()
    {
        // Arrange
        var schema = new BodySchema()
            .WithActuator(ActuatorDescriptor.Voice("v1"))
            .WithActuator(ActuatorDescriptor.Text("t1"));

        // Act
        var updated = schema.WithoutActuator("v1");

        // Assert
        updated.Actuators.Should().NotContainKey("v1");
        updated.Actuators.Should().ContainKey("t1");
    }

    [Fact]
    public void WithoutActuator_NonExistentActuator_ShouldReturnSameInstance()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var updated = schema.WithoutActuator("nonexistent");

        // Assert
        updated.Should().BeSameAs(schema);
    }

    [Fact]
    public void HasCapability_WithCapability_ShouldReturnTrue()
    {
        // Arrange
        var schema = new BodySchema().WithCapability(Capability.Reasoning);

        // Act & Assert
        schema.HasCapability(Capability.Reasoning).Should().BeTrue();
    }

    [Fact]
    public void HasCapability_WithoutCapability_ShouldReturnFalse()
    {
        // Arrange
        var schema = new BodySchema();

        // Act & Assert
        schema.HasCapability(Capability.Reasoning).Should().BeFalse();
    }

    [Fact]
    public void GetSensor_ExistingSensor_ShouldReturnSome()
    {
        // Arrange
        var schema = new BodySchema().WithSensor(SensorDescriptor.Audio("mic-1"));

        // Act
        var result = schema.GetSensor("mic-1");

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Id.Should().Be("mic-1");
    }

    [Fact]
    public void GetSensor_NonExistentSensor_ShouldReturnNone()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var result = schema.GetSensor("nonexistent");

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void GetActuator_ExistingActuator_ShouldReturnSome()
    {
        // Arrange
        var schema = new BodySchema().WithActuator(ActuatorDescriptor.Voice("v1"));

        // Act
        var result = schema.GetActuator("v1");

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Id.Should().Be("v1");
    }

    [Fact]
    public void GetActuator_NonExistentActuator_ShouldReturnNone()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var result = schema.GetActuator("nonexistent");

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void GetSensorsByModality_ShouldFilterCorrectly()
    {
        // Arrange
        var schema = new BodySchema()
            .WithSensor(SensorDescriptor.Audio("mic-1"))
            .WithSensor(SensorDescriptor.Visual("cam-1"))
            .WithSensor(SensorDescriptor.Audio("mic-2"));

        // Act
        var audioSensors = schema.GetSensorsByModality(SensorModality.Audio).ToList();

        // Assert
        audioSensors.Should().HaveCount(2);
        audioSensors.Should().OnlyContain(s => s.Modality == SensorModality.Audio);
    }

    [Fact]
    public void GetActuatorsByModality_ShouldFilterCorrectly()
    {
        // Arrange
        var schema = new BodySchema()
            .WithActuator(ActuatorDescriptor.Voice("v1"))
            .WithActuator(ActuatorDescriptor.Text("t1"));

        // Act
        var voiceActuators = schema.GetActuatorsByModality(ActuatorModality.Voice).ToList();

        // Assert
        voiceActuators.Should().ContainSingle();
        voiceActuators[0].Id.Should().Be("v1");
    }

    [Fact]
    public void CanPerceive_ActiveSensorWithCapability_ShouldReturnTrue()
    {
        // Arrange
        var schema = new BodySchema().WithSensor(SensorDescriptor.Audio("mic-1"));

        // Act & Assert
        schema.CanPerceive(Capability.Hearing).Should().BeTrue();
    }

    [Fact]
    public void CanPerceive_InactiveSensor_ShouldReturnFalse()
    {
        // Arrange
        var inactiveSensor = new SensorDescriptor(
            "mic-1", SensorModality.Audio, "Mic", false,
            new HashSet<Capability> { Capability.Hearing });
        var schema = new BodySchema().WithSensor(inactiveSensor);

        // Act & Assert
        schema.CanPerceive(Capability.Hearing).Should().BeFalse();
    }

    [Fact]
    public void CanAct_ActiveActuatorWithCapability_ShouldReturnTrue()
    {
        // Arrange
        var schema = new BodySchema().WithActuator(ActuatorDescriptor.Voice("v1"));

        // Act & Assert
        schema.CanAct(Capability.Speaking).Should().BeTrue();
    }

    [Fact]
    public void CanAct_InactiveActuator_ShouldReturnFalse()
    {
        // Arrange
        var inactiveActuator = new ActuatorDescriptor(
            "v1", ActuatorModality.Voice, "Voice", false,
            new HashSet<Capability> { Capability.Speaking });
        var schema = new BodySchema().WithActuator(inactiveActuator);

        // Act & Assert
        schema.CanAct(Capability.Speaking).Should().BeFalse();
    }

    [Fact]
    public void CreateConversational_ShouldSetupCorrectly()
    {
        // Act
        var schema = BodySchema.CreateConversational();

        // Assert
        schema.Sensors.Should().ContainKey("text-in");
        schema.Actuators.Should().ContainKey("text-out");
        schema.HasCapability(Capability.Reasoning).Should().BeTrue();
        schema.HasCapability(Capability.Remembering).Should().BeTrue();
        schema.HasCapability(Capability.Learning).Should().BeTrue();
        schema.HasCapability(Capability.Reading).Should().BeTrue();
        schema.HasCapability(Capability.Writing).Should().BeTrue();
        schema.Limitations.Should().HaveCount(2);
    }

    [Fact]
    public void CreateMultimodal_ShouldExtendConversational()
    {
        // Act
        var schema = BodySchema.CreateMultimodal();

        // Assert
        schema.Sensors.Should().ContainKey("text-in");
        schema.Sensors.Should().ContainKey("mic");
        schema.Sensors.Should().ContainKey("camera");
        schema.Actuators.Should().ContainKey("text-out");
        schema.Actuators.Should().ContainKey("voice");
        schema.HasCapability(Capability.EmotionPerception).Should().BeTrue();
        schema.HasCapability(Capability.EmotionExpression).Should().BeTrue();
    }

    [Fact]
    public void DescribeSelf_EmptySchema_ShouldReturnBaseDescription()
    {
        // Arrange
        var schema = new BodySchema();

        // Act
        var description = schema.DescribeSelf();

        // Assert
        description.Should().Contain("AI assistant");
    }

    [Fact]
    public void DescribeSelf_WithSensorsAndActuators_ShouldDescribeThem()
    {
        // Arrange
        var schema = BodySchema.CreateConversational();

        // Act
        var description = schema.DescribeSelf();

        // Assert
        description.Should().Contain("Sensors:");
        description.Should().Contain("Actuators:");
        description.Should().Contain("Capabilities:");
        description.Should().Contain("Limitations:");
    }

    [Fact]
    public void WithSensor_SameId_ShouldReplaceExisting()
    {
        // Arrange
        var schema = new BodySchema().WithSensor(SensorDescriptor.Audio("mic-1", "Old Mic"));

        // Act
        var updated = schema.WithSensor(SensorDescriptor.Audio("mic-1", "New Mic"));

        // Assert
        updated.Sensors.Should().ContainKey("mic-1");
        updated.Sensors["mic-1"].Name.Should().Be("New Mic");
    }

    [Fact]
    public void WithoutSensor_ShouldRecomputeCapabilities()
    {
        // Arrange - only sensor providing Hearing
        var schema = new BodySchema()
            .WithSensor(SensorDescriptor.Audio("mic-1"))
            .WithSensor(SensorDescriptor.Visual("cam-1"));

        // Act
        var updated = schema.WithoutSensor("mic-1");

        // Assert - Hearing should be gone, Seeing should remain
        updated.HasCapability(Capability.Hearing).Should().BeFalse();
        updated.HasCapability(Capability.Seeing).Should().BeTrue();
    }
}
