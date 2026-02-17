namespace Ouroboros.Tools;

/// <summary>
/// Advanced tool builder for creating composable, performance-aware tools.
/// </summary>
public static class AdvancedToolBuilder
{
    /// <summary>
    /// Creates a pipeline of tools that execute sequentially.
    /// </summary>
    /// <returns></returns>
    public static ITool Pipeline(
        string name,
        string description,
        params ITool[] tools)
    {
        return ToolBuilder.Chain(name, description, tools);
    }

    /// <summary>
    /// Creates a conditional tool that routes to different tools based on predicate.
    /// </summary>
    /// <returns></returns>
    public static ITool Switch(
        string name,
        string description,
        params (Func<string, bool> Predicate, ITool Tool)[] cases)
    {
        return new DelegateTool(
            name,
            description,
            async (input, ct) =>
            {
                foreach ((Func<string, bool> predicate, ITool tool) in cases)
                {
                    if (predicate(input))
                    {
                        return await tool.InvokeAsync(input, ct);
                    }
                }

                return Result<string, string>.Failure(
                    "No matching condition found for input");
            });
    }

    /// <summary>
    /// Creates a tool that aggregates results from multiple tools.
    /// </summary>
    /// <returns></returns>
    public static ITool Aggregate(
        string name,
        string description,
        Func<List<string>, string> aggregator,
        params ITool[] tools)
    {
        return new DelegateTool(
            name,
            description,
            async (input, ct) =>
            {
                List<string> results = new List<string>();

                foreach (ITool tool in tools)
                {
                    Result<string, string> result = await tool.InvokeAsync(input, ct);
                    result.Match(
                        success => results.Add(success),
                        failure => { /* Skip failed tools */ });
                }

                if (results.Count == 0)
                {
                    return Result<string, string>.Failure(
                        "All tools in aggregate failed");
                }

                try
                {
                    string aggregated = aggregator(results);
                    return Result<string, string>.Success(aggregated);
                }
                catch (Exception ex)
                {
                    return Result<string, string>.Failure(
                        $"Aggregation failed: {ex.Message}");
                }
            });
    }
}