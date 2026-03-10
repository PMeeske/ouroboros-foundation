using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class AutoDenyApprovalProviderTests
{
    private readonly AutoDenyApprovalProvider _sut = new();

    private static HumanApprovalRequest CreateRequest(Guid? id = null)
    {
        return new HumanApprovalRequest
        {
            Id = id ?? Guid.NewGuid(),
            Category = "action",
            Description = "Test action requiring approval",
            Clearance = EthicalClearance.RequiresApproval("Needs human review")
        };
    }

    [Fact]
    public async Task RequestApprovalAsync_ReturnsRejectedDecision()
    {
        var request = CreateRequest();

        var response = await _sut.RequestApprovalAsync(request);

        response.Decision.Should().Be(HumanApprovalDecision.Rejected);
    }

    [Fact]
    public async Task RequestApprovalAsync_ResponseContainsMatchingRequestId()
    {
        var requestId = Guid.NewGuid();
        var request = CreateRequest(requestId);

        var response = await _sut.RequestApprovalAsync(request);

        response.RequestId.Should().Be(requestId);
    }

    [Fact]
    public async Task RequestApprovalAsync_ResponseContainsSafetyExplanation()
    {
        var request = CreateRequest();

        var response = await _sut.RequestApprovalAsync(request);

        response.ReviewerComments.Should().NotBeNullOrEmpty();
        response.ReviewerComments.Should().Contain("denied by default");
    }

    [Fact]
    public async Task RequestApprovalAsync_ResponseContainsNoProviderConfiguredMessage()
    {
        var request = CreateRequest();

        var response = await _sut.RequestApprovalAsync(request);

        response.ReviewerComments.Should().Contain("No human approval provider configured");
    }

    [Fact]
    public async Task RequestApprovalAsync_WithCancellationToken_CompletesSuccessfully()
    {
        var request = CreateRequest();
        using var cts = new CancellationTokenSource();

        var response = await _sut.RequestApprovalAsync(request, cts.Token);

        response.Decision.Should().Be(HumanApprovalDecision.Rejected);
    }

    [Fact]
    public async Task RequestApprovalAsync_MultipleRequests_EachGetUniqueRejection()
    {
        var request1 = CreateRequest();
        var request2 = CreateRequest();

        var response1 = await _sut.RequestApprovalAsync(request1);
        var response2 = await _sut.RequestApprovalAsync(request2);

        response1.RequestId.Should().Be(request1.Id);
        response2.RequestId.Should().Be(request2.Id);
        response1.RequestId.Should().NotBe(response2.RequestId);
    }

    [Fact]
    public async Task RequestApprovalAsync_NeverReturnsApproved()
    {
        var request = CreateRequest();

        var response = await _sut.RequestApprovalAsync(request);

        response.Decision.Should().NotBe(HumanApprovalDecision.Approved);
        response.Decision.Should().NotBe(HumanApprovalDecision.TimedOut);
    }

    [Fact]
    public void ImplementsIHumanApprovalProvider()
    {
        _sut.Should().BeAssignableTo<IHumanApprovalProvider>();
    }

    [Fact]
    public async Task RequestApprovalAsync_WithDifferentCategories_AllDenied()
    {
        var categories = new[] { "plan", "action", "self-modification", "goal" };

        foreach (var category in categories)
        {
            var request = new HumanApprovalRequest
            {
                Category = category,
                Description = $"Test {category}",
                Clearance = EthicalClearance.RequiresApproval($"Review {category}")
            };

            var response = await _sut.RequestApprovalAsync(request);
            response.Decision.Should().Be(HumanApprovalDecision.Rejected,
                because: $"category '{category}' should be denied");
        }
    }

    [Fact]
    public async Task RequestApprovalAsync_CompletesImmediately()
    {
        var request = CreateRequest();

        var task = _sut.RequestApprovalAsync(request);

        task.IsCompleted.Should().BeTrue("AutoDenyApprovalProvider should return synchronously");
    }
}
