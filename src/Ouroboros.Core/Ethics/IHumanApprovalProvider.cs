namespace Ouroboros.Core.Ethics;

/// <summary>
/// Provides a mechanism for requesting and receiving human approval
/// for actions or plans that require human-in-the-loop authorization.
/// </summary>
/// <remarks>
/// Implementations should be registered via DI. The default implementation
/// (<see cref="AutoDenyApprovalProvider"/>) denies all requests for safety.
/// CLI hosts should use a console-based provider; API hosts should use
/// a webhook/polling-based provider.
/// </remarks>
public interface IHumanApprovalProvider
{
    /// <summary>
    /// Requests human approval for an action or plan that requires authorization.
    /// </summary>
    /// <param name="request">The approval request containing context and clearance details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A response indicating whether the action was approved, rejected, or timed out.</returns>
    Task<HumanApprovalResponse> RequestApprovalAsync(
        HumanApprovalRequest request,
        CancellationToken ct = default);
}
