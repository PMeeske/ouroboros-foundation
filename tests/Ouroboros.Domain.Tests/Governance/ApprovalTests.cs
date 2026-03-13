using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ApprovalTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var approval = new Approval
        {
            ApproverId = "user-123",
            Decision = ApprovalDecision.Approve
        };

        approval.ApproverId.Should().Be("user-123");
        approval.Decision.Should().Be(ApprovalDecision.Approve);
        approval.Comments.Should().BeNull();
        approval.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var timestamp = new DateTime(2025, 7, 1, 10, 0, 0, DateTimeKind.Utc);
        var approval = new Approval
        {
            ApproverId = "admin-1",
            Decision = ApprovalDecision.Reject,
            Comments = "Insufficient justification",
            ApprovedAt = timestamp
        };

        approval.ApproverId.Should().Be("admin-1");
        approval.Decision.Should().Be(ApprovalDecision.Reject);
        approval.Comments.Should().Be("Insufficient justification");
        approval.ApprovedAt.Should().Be(timestamp);
    }

    [Theory]
    [InlineData(ApprovalDecision.Approve)]
    [InlineData(ApprovalDecision.Reject)]
    [InlineData(ApprovalDecision.RequestInfo)]
    public void Construction_AllDecisionTypes_Accepted(ApprovalDecision decision)
    {
        var approval = new Approval { ApproverId = "user", Decision = decision };

        approval.Decision.Should().Be(decision);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = DateTime.UtcNow;
        var a1 = new Approval { ApproverId = "u", Decision = ApprovalDecision.Approve, ApprovedAt = ts };
        var a2 = new Approval { ApproverId = "u", Decision = ApprovalDecision.Approve, ApprovedAt = ts };

        a1.Should().Be(a2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var approval = new Approval { ApproverId = "u", Decision = ApprovalDecision.Approve };
        var modified = approval with { Comments = "Looks good" };

        modified.Comments.Should().Be("Looks good");
        approval.Comments.Should().BeNull();
    }
}
