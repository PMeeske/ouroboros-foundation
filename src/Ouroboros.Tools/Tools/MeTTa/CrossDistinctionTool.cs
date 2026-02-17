using System.Text.Json;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for crossing (negating) a distinction.
/// </summary>
public sealed class CrossDistinctionTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrossDistinctionTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public CrossDistinctionTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_cross_distinction";

    /// <inheritdoc/>
    public string Description =>
        "Crosses (negates) a distinction in the Laws of Form calculus. " +
        "Crossing transforms: Mark->Void, Void->Mark, Imaginary->Imaginary. " +
        "Double crossing cancels (Law of Crossing).";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "context": {
                                            "type": "string",
                                            "description": "The context name of the distinction to cross"
                                        }
                                    },
                                    "required": ["context"]
                                }
                                """;

    /// <inheritdoc/>
    public Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(input) ?? new();
            string context = args.TryGetValue("context", out var c) ? c.GetString() ?? "default" : "default";

            Form result = _bridge.CrossDistinction(context);
            string formState = result.IsMarked() ? "Mark (certain)" : result.IsVoid() ? "Void (negated)" : "Imaginary (uncertain)";

            return Task.FromResult(Result<string, string>.Success($"Distinction crossed in '{context}': {formState}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}