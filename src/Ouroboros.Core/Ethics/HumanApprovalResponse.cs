using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Ethics;

/// <summary>
/// The result of a human approval request.
/// </summary>
public sealed record HumanApprovalResponse
{
    /// <summary>
    /// Gets the request ID this response corresponds to.
    /// </summary>
    public required Guid RequestId { get; init; }

    /// <summary>
    /// Gets the decision made by the human reviewer.
    /// </summary>
    public required HumanApprovalDecision Decision { get; init; }

    /// <summary>
    /// Gets optional comments or reasoning from the reviewer.
    /// </summary>
    public string? ReviewerComments { get; init; }

    /// <summary>
    /// Gets optional modifications suggested by the reviewer.
    /// For plans, this could be adjusted parameters or removed steps.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Modifications { get; init; }

    /// <summary>
    /// Gets the identifier of the reviewer (user ID or role).
    /// </summary>
    public string? ReviewerId { get; init; }

    /// <summary>
    /// Gets the timestamp when this response was created.
    /// </summary>
    public DateTime RespondedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates an approved response.
    /// </summary>
    public static HumanApprovalResponse Approved(Guid requestId, string? reviewerId = null, string? comments = null) =>
        new()
        {
            RequestId = requestId,
            Decision = HumanApprovalDecision.Approved,
            ReviewerId = reviewerId,
            ReviewerComments = comments
        };

    /// <summary>
    /// Creates a rejected response.
    /// </summary>
    public static HumanApprovalResponse Rejected(Guid requestId, string reason, string? reviewerId = null) =>
        new()
        {
            RequestId = requestId,
            Decision = HumanApprovalDecision.Rejected,
            ReviewerComments = reason,
            ReviewerId = reviewerId
        };

    /// <summary>
    /// Creates a timed-out response.
    /// </summary>
    public static HumanApprovalResponse TimedOut(Guid requestId) =>
        new()
        {
            RequestId = requestId,
            Decision = HumanApprovalDecision.TimedOut,
            ReviewerComments = "Approval request timed out without a response"
        };
}

/// <summary>
/// The possible decisions for a human approval request.
/// </summary>
[ExcludeFromCodeCoverage]
public enum HumanApprovalDecision
{
    /// <summary>The action/plan was approved by the reviewer.</summary>
    Approved,

    /// <summary>The action/plan was rejected by the reviewer.</summary>
    Rejected,

    /// <summary>The approval request timed out without a response.</summary>
    TimedOut
}
