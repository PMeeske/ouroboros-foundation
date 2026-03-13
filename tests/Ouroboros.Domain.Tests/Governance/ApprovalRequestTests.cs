using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ApprovalRequestTests
{
    private static ApprovalGate CreateGate(int minimumApprovals = 1) => new()
    {
        Id = Guid.NewGuid(),
        Name = "TestGate",
        Condition = "always",
        MinimumApprovals = minimumApprovals
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var gate = CreateGate();
        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy v2"
        };

        request.Id.Should().NotBeEmpty();
        request.Gate.Should().Be(gate);
        request.OperationDescription.Should().Be("Deploy v2");
        request.Status.Should().Be(ApprovalStatus.Pending);
        request.Approvals.Should().BeEmpty();
        request.Context.Should().BeEmpty();
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IsApproved_WithEnoughApprovals_ReturnsTrue()
    {
        var gate = CreateGate(minimumApprovals: 1);
        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Approved,
            Approvals = new[]
            {
                new Approval { ApproverId = "admin", Decision = ApprovalDecision.Approve }
            }
        };

        request.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void IsApproved_WithInsufficientApprovals_ReturnsFalse()
    {
        var gate = CreateGate(minimumApprovals: 2);
        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Approved,
            Approvals = new[]
            {
                new Approval { ApproverId = "admin", Decision = ApprovalDecision.Approve }
            }
        };

        request.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void IsApproved_WhenNotApprovedStatus_ReturnsFalse()
    {
        var gate = CreateGate(minimumApprovals: 1);
        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Pending,
            Approvals = new[]
            {
                new Approval { ApproverId = "admin", Decision = ApprovalDecision.Approve }
            }
        };

        request.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastDeadline_ReturnsTrue()
    {
        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy",
            Deadline = DateTime.UtcNow.AddHours(-1)
        };

        request.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_FutureDeadline_ReturnsFalse()
    {
        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy",
            Deadline = DateTime.UtcNow.AddHours(1)
        };

        request.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy"
        };

        var modified = request with { Status = ApprovalStatus.Approved };

        modified.Status.Should().Be(ApprovalStatus.Approved);
        request.Status.Should().Be(ApprovalStatus.Pending);
    }
}
