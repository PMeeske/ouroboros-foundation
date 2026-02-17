using System.Text.Json;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for drawing a distinction (creating a mark) in a context.
/// </summary>
public sealed class DrawDistinctionTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawDistinctionTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public DrawDistinctionTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_draw_distinction";

    /// <inheritdoc/>
    public string Description =>
        "Draws a distinction (creates a mark) in the Laws of Form calculus. " +
        "A mark indicates certain affirmation. Use to establish a definite boundary or assertion.";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "context": {
                                            "type": "string",
                                            "description": "The context name for this distinction"
                                        },
                                        "reason": {
                                            "type": "string",
                                            "description": "Optional reason for drawing this distinction"
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

            Form result = _bridge.DrawDistinction(context);
            string formState = result.IsMarked() ? "Mark (certain)" : result.IsVoid() ? "Void (negated)" : "Imaginary (uncertain)";

            return Task.FromResult(Result<string, string>.Success($"Distinction drawn in '{context}': {formState}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}