// <copyright file="OperatingCostAuditResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.States;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the complete audit result for an operating cost statement.
/// This is a non-legal, formal completeness check.
/// </summary>
/// <param name="DocumentsAnalyzed">Whether documents were successfully analyzed.</param>
/// <param name="OverallFormalStatus">Overall status of the audit.</param>
/// <param name="Categories">Audit results for each cost category.</param>
/// <param name="CriticalGaps">List of critical gaps identified.</param>
/// <param name="SummaryShort">Brief summary of the audit findings.</param>
/// <param name="Note">Legal disclaimer note.</param>
public sealed record OperatingCostAuditResult(
    bool DocumentsAnalyzed,
    FormalStatus OverallFormalStatus,
    IReadOnlyList<CostCategoryAudit> Categories,
    IReadOnlyList<string> CriticalGaps,
    string SummaryShort,
    string Note = "This output does not contain legal evaluation or statements on validity or enforceability.")
    : ReasoningState("OperatingCostAudit", SummaryShort)
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Gets the JSON representation of the audit result.
    /// </summary>
    [JsonIgnore]
    public string AsJson => System.Text.Json.JsonSerializer.Serialize(
        new
        {
            documents_analyzed = DocumentsAnalyzed,
            overall_formal_status = OverallFormalStatus.ToString().ToLowerInvariant(),
            categories = Categories.Select(c => new
            {
                category = c.Category,
                total_costs = c.TotalCosts.ToString(),
                reference_metric = c.ReferenceMetric.ToString(),
                total_reference_value = c.TotalReferenceValue.ToString(),
                tenant_share = c.TenantShare.ToString(),
                tenant_cost = c.TenantCost.ToString(),
                balance = c.Balance.ToString(),
                comment = c.Comment,
            }),
            critical_gaps = CriticalGaps,
            summary_short = SummaryShort,
            note = Note,
        },
        JsonOptions);
}
