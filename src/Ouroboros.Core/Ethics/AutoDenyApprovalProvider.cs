using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Ethics;

/// <summary>
/// Default approval provider that automatically denies all requests.
/// This is the safe default — if no human approval mechanism is configured,
/// actions requiring approval are blocked rather than silently permitted.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class AutoDenyApprovalProvider : IHumanApprovalProvider
{
    /// <inheritdoc/>
    public Task<HumanApprovalResponse> RequestApprovalAsync(
        HumanApprovalRequest request,
        CancellationToken ct = default)
    {
        return Task.FromResult(HumanApprovalResponse.Rejected(
            request.Id,
            "No human approval provider configured. Actions requiring human approval are denied by default for safety."));
    }
}
