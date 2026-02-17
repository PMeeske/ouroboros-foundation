// <copyright file="Policy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines a governance policy with rules, quotas, and thresholds.
/// Phase 5: Governance, Safety, and Ops.
/// </summary>
public sealed record Policy
{
    /// <summary>
    /// Gets the unique policy identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the policy name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the policy description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the policy priority (higher values take precedence).
    /// </summary>
    public double Priority { get; init; } = 1.0;

    /// <summary>
    /// Gets a value indicating whether this policy is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the policy rules.
    /// </summary>
    public IReadOnlyList<PolicyRule> Rules { get; init; } = Array.Empty<PolicyRule>();

    /// <summary>
    /// Gets the resource quotas defined by this policy.
    /// </summary>
    public IReadOnlyList<ResourceQuota> Quotas { get; init; } = Array.Empty<ResourceQuota>();

    /// <summary>
    /// Gets the thresholds that trigger policy actions.
    /// </summary>
    public IReadOnlyList<Threshold> Thresholds { get; init; } = Array.Empty<Threshold>();

    /// <summary>
    /// Gets the approval gates required by this policy.
    /// </summary>
    public IReadOnlyList<ApprovalGate> ApprovalGates { get; init; } = Array.Empty<ApprovalGate>();

    /// <summary>
    /// Gets the timestamp when the policy was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the timestamp when the policy was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new policy with the specified name and description.
    /// </summary>
    public static Policy Create(string name, string description) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = description,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}