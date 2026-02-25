namespace Ouroboros.Tools;

/// <summary>
/// Monadic tool builder for creating sophisticated tool compositions.
/// </summary>
public static class ToolBuilder
{
    /// <summary>
    /// Create a tool that chains multiple operations.
    /// </summary>
    /// <returns></returns>
    public static ITool Chain(string name, string description, params ITool[] tools)
    {
        return new DelegateTool(name, description, async (input, ct) =>
        {
            Result<string, string> result = Result<string, string>.Success(input);

            foreach (ITool tool in tools)
            {
                if (ct.IsCancellationRequested)
                {
                    return Result<string, string>.Failure("Operation cancelled");
                }

                result = await result.Match(
                    async success => await tool.InvokeAsync(success, ct),
                    failure => Task.FromResult(Result<string, string>.Failure(failure)));

                if (result.IsFailure)
                {
                    break;
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Create a tool that tries multiple tools in sequence.
    /// </summary>
    /// <returns></returns>
    public static ITool FirstSuccess(string name, string description, params ITool[] tools)
    {
        return new DelegateTool(name, description, async (input, ct) =>
        {
            foreach (ITool tool in tools)
            {
                if (ct.IsCancellationRequested)
                {
                    return Result<string, string>.Failure("Operation cancelled");
                }

                Result<string, string> result = await tool.InvokeAsync(input, ct);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return Result<string, string>.Failure("All tools failed");
        });
    }

    /// <summary>
    /// Create a conditional tool that selects based on input.
    /// </summary>
    /// <returns></returns>
    public static ITool Conditional(string name, string description,
        Func<string, ITool> selector)
    {
        return new DelegateTool(name, description, async (input, ct) =>
        {
            try
            {
                ITool selectedTool = selector(input);
                return await selectedTool.InvokeAsync(input, ct);
            }
            catch (Exception ex)
            {
                return Result<string, string>.Failure($"Tool selection failed: {ex.Message}");
            }
        });
    }
}