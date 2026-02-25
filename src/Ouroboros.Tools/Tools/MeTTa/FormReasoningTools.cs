// <copyright file="FormReasoningTools.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tools that expose Laws of Form reasoning to the agent system.
/// These tools enable distinction-gated inference and form-based logic.
/// </summary>
public static class FormReasoningTools
{
    /// <summary>
    /// Extends a ToolRegistry with Laws of Form reasoning tools.
    /// </summary>
    /// <param name="registry">The registry to extend.</param>
    /// <param name="bridge">The FormMeTTaBridge instance to use.</param>
    /// <returns>A new registry with LoF tools added.</returns>
    public static ToolRegistry WithFormReasoningTools(this ToolRegistry registry, FormMeTTaBridge bridge)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(bridge);

        return registry
            .WithTool(new DrawDistinctionTool(bridge))
            .WithTool(new CrossDistinctionTool(bridge))
            .WithTool(new EvaluateCertaintyTool(bridge))
            .WithTool(new DistinctionGatedInferenceTool(bridge))
            .WithTool(new FormPatternMatchTool(bridge))
            .WithTool(new CreateReEntryTool(bridge));
    }

    /// <summary>
    /// Creates a ToolRegistry with Laws of Form tools pre-registered.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge instance to use.</param>
    /// <returns>A new ToolRegistry with LoF tools.</returns>
    public static ToolRegistry CreateWithFormReasoning(FormMeTTaBridge bridge)
    {
        return ToolRegistry.CreateDefault().WithFormReasoningTools(bridge);
    }
}