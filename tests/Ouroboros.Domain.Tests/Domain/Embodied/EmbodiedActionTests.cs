namespace Ouroboros.Tests.Domain.Embodied;

using Ouroboros.Domain.Embodied;

[Trait("Category", "Unit")]
public class EmbodiedActionTests
{
    [Fact]
    public void NoOp_HasZeroMovementAndRotation()
    {
        // Act
        var action = EmbodiedAction.NoOp();

        // Assert
        action.Movement.Should().Be(Vector3.Zero);
        action.Rotation.Should().Be(Vector3.Zero);
        action.CustomActions.Should().BeEmpty();
        action.ActionName.Should().Be("NoOp");
    }

    [Fact]
    public void Move_SetsMovementOnly()
    {
        // Arrange
        var movement = new Vector3(1f, 0f, 0f);

        // Act
        var action = EmbodiedAction.Move(movement);

        // Assert
        action.Movement.Should().Be(movement);
        action.Rotation.Should().Be(Vector3.Zero);
        action.ActionName.Should().Be("Move");
    }

    [Fact]
    public void Move_WithCustomName_SetsName()
    {
        // Act
        var action = EmbodiedAction.Move(Vector3.UnitX, "Sprint");

        // Assert
        action.ActionName.Should().Be("Sprint");
    }

    [Fact]
    public void Rotate_SetsRotationOnly()
    {
        // Arrange
        var rotation = new Vector3(0f, 90f, 0f);

        // Act
        var action = EmbodiedAction.Rotate(rotation);

        // Assert
        action.Rotation.Should().Be(rotation);
        action.Movement.Should().Be(Vector3.Zero);
        action.ActionName.Should().Be("Rotate");
    }

    [Fact]
    public void Rotate_WithCustomName_SetsName()
    {
        // Act
        var action = EmbodiedAction.Rotate(Vector3.UnitY, "TurnLeft");

        // Assert
        action.ActionName.Should().Be("TurnLeft");
    }

    [Fact]
    public void Constructor_WithCustomActions_SetsAll()
    {
        // Arrange
        var customActions = new Dictionary<string, float>
        {
            ["gripper_force"] = 0.5f,
            ["jump"] = 1.0f,
        };

        // Act
        var action = new EmbodiedAction(
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 45f, 0f),
            customActions,
            "GrabAndTurn");

        // Assert
        action.CustomActions.Should().HaveCount(2);
        action.CustomActions["gripper_force"].Should().Be(0.5f);
        action.ActionName.Should().Be("GrabAndTurn");
    }

    [Fact]
    public void Constructor_NullActionName_IsAllowed()
    {
        // Act
        var action = new EmbodiedAction(Vector3.Zero, Vector3.Zero, new Dictionary<string, float>());

        // Assert
        action.ActionName.Should().BeNull();
    }
}
