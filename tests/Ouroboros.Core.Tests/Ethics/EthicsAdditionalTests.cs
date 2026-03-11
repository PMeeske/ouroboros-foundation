using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

/// <summary>
/// Additional tests for HumanApprovalResponse, HumanApprovalRequest, and HumanApprovalDecision.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalResponseAdditionalTests
{
    [Fact]
    public void Approved_SetsDecisionAndRequestId()
    {
        var requestId = Guid.NewGuid();
        var response = HumanApprovalResponse.Approved(requestId);

        response.RequestId.Should().Be(requestId);
        response.Decision.Should().Be(HumanApprovalDecision.Approved);
        response.ReviewerId.Should().BeNull();
        response.ReviewerComments.Should().BeNull();
    }

    [Fact]
    public void Approved_WithReviewerAndComments_SetsAllProperties()
    {
        var requestId = Guid.NewGuid();
        var response = HumanApprovalResponse.Approved(requestId, "reviewer-1", "Looks good");

        response.RequestId.Should().Be(requestId);
        response.Decision.Should().Be(HumanApprovalDecision.Approved);
        response.ReviewerId.Should().Be("reviewer-1");
        response.ReviewerComments.Should().Be("Looks good");
    }

    [Fact]
    public void Rejected_SetsDecisionAndReason()
    {
        var requestId = Guid.NewGuid();
        var response = HumanApprovalResponse.Rejected(requestId, "Too risky");

        response.RequestId.Should().Be(requestId);
        response.Decision.Should().Be(HumanApprovalDecision.Rejected);
        response.ReviewerComments.Should().Be("Too risky");
    }

    [Fact]
    public void Rejected_WithReviewer_SetsReviewerId()
    {
        var requestId = Guid.NewGuid();
        var response = HumanApprovalResponse.Rejected(requestId, "Denied", "admin-1");

        response.ReviewerId.Should().Be("admin-1");
    }

    [Fact]
    public void TimedOut_SetsDecisionAndMessage()
    {
        var requestId = Guid.NewGuid();
        var response = HumanApprovalResponse.TimedOut(requestId);

        response.RequestId.Should().Be(requestId);
        response.Decision.Should().Be(HumanApprovalDecision.TimedOut);
        response.ReviewerComments.Should().Contain("timed out");
    }

    [Fact]
    public void Modifications_CanBeSet()
    {
        var requestId = Guid.NewGuid();
        var mods = new Dictionary<string, object> { ["maxSteps"] = 10 };

        var response = new HumanApprovalResponse
        {
            RequestId = requestId,
            Decision = HumanApprovalDecision.Approved,
            Modifications = mods
        };

        response.Modifications.Should().ContainKey("maxSteps");
    }

    [Fact]
    public void RespondedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var response = HumanApprovalResponse.Approved(Guid.NewGuid());
        var after = DateTime.UtcNow;

        response.RespondedAt.Should().BeOnOrAfter(before);
        response.RespondedAt.Should().BeOnOrBefore(after);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalRequestAdditionalTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var request = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test action",
            Clearance = EthicalClearance.RequiresApproval("Needs review")
        };

        request.Id.Should().NotBe(Guid.Empty);
        request.Timeout.Should().Be(TimeSpan.FromMinutes(5));
        request.Context.Should().BeEmpty();
    }

    [Fact]
    public void Id_AutoGeneratesUniqueGuids()
    {
        var request1 = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test 1",
            Clearance = EthicalClearance.RequiresApproval("Review")
        };
        var request2 = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test 2",
            Clearance = EthicalClearance.RequiresApproval("Review")
        };

        request1.Id.Should().NotBe(request2.Id);
    }

    [Fact]
    public void Context_CanBeSet()
    {
        var context = new Dictionary<string, object> { ["reason"] = "safety" };
        var request = new HumanApprovalRequest
        {
            Category = "plan",
            Description = "Execute plan",
            Clearance = EthicalClearance.RequiresApproval("Review"),
            Context = context
        };

        request.Context.Should().ContainKey("reason");
    }

    [Fact]
    public void Timeout_CanBeSetToNull()
    {
        var request = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test",
            Clearance = EthicalClearance.RequiresApproval("Review"),
            Timeout = null
        };

        request.Timeout.Should().BeNull();
    }

    [Fact]
    public void CreatedAt_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var request = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Test",
            Clearance = EthicalClearance.RequiresApproval("Review")
        };
        var after = DateTime.UtcNow;

        request.CreatedAt.Should().BeOnOrAfter(before);
        request.CreatedAt.Should().BeOnOrBefore(after);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalDecisionAdditionalTests
{
    [Theory]
    [InlineData(HumanApprovalDecision.Approved)]
    [InlineData(HumanApprovalDecision.Rejected)]
    [InlineData(HumanApprovalDecision.TimedOut)]
    public void AllValues_AreDefined(HumanApprovalDecision decision)
    {
        Enum.IsDefined(decision).Should().BeTrue();
    }

    [Fact]
    public void AllValues_Count()
    {
        Enum.GetValues<HumanApprovalDecision>().Should().HaveCount(3);
    }
}
