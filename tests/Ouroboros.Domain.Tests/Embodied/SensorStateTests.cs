using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class SensorStateTests
{
    [Fact]
    public void Default_ShouldHaveZeroPositionAndVelocity()
    {
        var state = SensorState.Default();

        state.Position.Should().Be(Vector3.Zero);
        state.Velocity.Should().Be(Vector3.Zero);
        state.Rotation.Should().Be(Quaternion.Identity);
    }

    [Fact]
    public void Default_ShouldHaveEmptyObservations()
    {
        var state = SensorState.Default();
        state.VisualObservation.Should().BeEmpty();
        state.ProprioceptiveState.Should().BeEmpty();
        state.CustomSensors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var pos = new Vector3(1f, 2f, 3f);
        var rot = Quaternion.Identity;
        var vel = new Vector3(0f, 1f, 0f);
        var visual = new float[] { 0.5f };
        var proprio = new float[] { 1.0f };
        var sensors = new Dictionary<string, float> { ["temp"] = 25f };

        var state = new SensorState(pos, rot, vel, visual, proprio, sensors, DateTime.UtcNow);

        state.Position.Should().Be(pos);
        state.Velocity.Should().Be(vel);
        state.VisualObservation.Should().HaveCount(1);
        state.CustomSensors.Should().ContainKey("temp");
    }
}
