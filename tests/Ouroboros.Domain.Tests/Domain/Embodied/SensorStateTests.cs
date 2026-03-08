namespace Ouroboros.Tests.Domain.Embodied;

using Ouroboros.Domain.Embodied;

[Trait("Category", "Unit")]
public class SensorStateTests
{
    [Fact]
    public void Default_HasZeroPositionAndVelocity()
    {
        // Act
        var state = SensorState.Default();

        // Assert
        state.Position.Should().Be(Vector3.Zero);
        state.Velocity.Should().Be(Vector3.Zero);
        state.Rotation.Should().Be(Quaternion.Identity);
    }

    [Fact]
    public void Default_HasEmptySensorArrays()
    {
        // Act
        var state = SensorState.Default();

        // Assert
        state.VisualObservation.Should().BeEmpty();
        state.ProprioceptiveState.Should().BeEmpty();
        state.CustomSensors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var pos = new Vector3(1f, 2f, 3f);
        var rot = new Quaternion(0f, 0f, 0f, 1f);
        var vel = new Vector3(0.5f, 0f, 0f);
        var visual = new float[] { 0.1f, 0.2f };
        var proprio = new float[] { 0.3f, 0.4f };
        var custom = new Dictionary<string, float> { ["sensor1"] = 0.5f };
        var timestamp = DateTime.UtcNow;

        // Act
        var state = new SensorState(pos, rot, vel, visual, proprio, custom, timestamp);

        // Assert
        state.Position.Should().Be(pos);
        state.Rotation.Should().Be(rot);
        state.Velocity.Should().Be(vel);
        state.VisualObservation.Should().HaveCount(2);
        state.ProprioceptiveState.Should().HaveCount(2);
        state.CustomSensors.Should().ContainKey("sensor1");
        state.Timestamp.Should().Be(timestamp);
    }
}
