namespace Ouroboros.Domain.States;

/// <summary>
/// Represents the audit results for a single cost category.
/// </summary>
/// <param name="Category">The cost category name (e.g., heating, water, garbage).</param>
/// <param name="TotalCosts">Status of total costs visibility.</param>
/// <param name="ReferenceMetric">Status of declared allocation key/reference metric.</param>
/// <param name="TotalReferenceValue">Status of total reference metric value.</param>
/// <param name="TenantShare">Status of allocated share for the tenant.</param>
/// <param name="TenantCost">Status of calculated cost portion for the tenant.</param>
/// <param name="Balance">Status of resulting balance (credit/amount due).</param>
/// <param name="Comment">Optional comment explaining the evaluation.</param>
public sealed record CostCategoryAudit(
    string Category,
    FieldStatus TotalCosts,
    FieldStatus ReferenceMetric,
    FieldStatus TotalReferenceValue,
    FieldStatus TenantShare,
    FieldStatus TenantCost,
    FieldStatus Balance,
    string? Comment = null);