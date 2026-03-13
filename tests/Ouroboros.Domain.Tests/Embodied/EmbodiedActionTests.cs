using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class EmbodiedActionTests
{
    [Fact]
    public void NoOp_ShouldHaveZeroMovementAndRotation()
    {
        var action = EmbodiedAction.NoOp();
        action.Movement.Should().Be(Vector3.Zero);
        action.Rotation.Should().Be(Vector3.Zero);
        action.ActionName.Should().Be("NoOp");
    }

    [Fact]
    public void Move_ShouldSetMovementAndZeroRotation()
    {
        var movement = new Vector3(1f, 0f, 0f);
        var action = EmbodiedAction.Move(movement);

        action.Movement.Should().Be(movement);
        action.Rotation.Should().Be(Vector3.Zero);
        action.ActionName.Should().Be("Move");
    }

    [Fact]
    public void Rotate_ShouldSetRotationAndZeroMovement()
    {
        var rotation = new Vector3(0f, 90f, 0f);
        var action = EmbodiedAction.Rotate(rotation);

        action.Movement.Should().Be(Vector3.Zero);
        action.Rotation.Should().Be(rotation);
        action.ActionName.Should().Be("Rotate");
    }

    [Fact]
    public void Move_WithCustomName_ShouldUseCustomName()
    {
        var action = EmbodiedAction.Move(Vector3.UnitX, "Sprint");
        action.ActionName.Should().Be("Sprint");
    }

    [Fact]
    public void CustomActions_ShouldBeEmptyForFactoryMethods()
    {
        EmbodiedAction.NoOp().CustomActions.Should().BeEmpty();
        EmbodiedAction.Move(Vector3.UnitX).CustomActions.Should().BeEmpty();
        EmbodiedAction.Rotate(Vector3.UnitY).CustomActions.Should().BeEmpty();
    }
}
