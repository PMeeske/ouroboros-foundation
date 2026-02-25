using System.Text.Json;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for evaluating the certainty of a form.
/// </summary>
public sealed class EvaluateCertaintyTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluateCertaintyTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public EvaluateCertaintyTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_evaluate_certainty";

    /// <inheritdoc/>
    public string Description =>
        "Evaluates the certainty of an expression in the Laws of Form calculus. " +
        "Returns: Mark (certain affirmation), Void (certain negation), or Imaginary (uncertain/paradoxical).";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "expression": {
                                            "type": "string",
                                            "description": "The MeTTa expression to evaluate for certainty"
                                        }
                                    },
                                    "required": ["expression"]
                                }
                                """;

    /// <inheritdoc/>
    public Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(input) ?? new();
            string expression = args.TryGetValue("expression", out var e) ? e.GetString() ?? "" : "";

            Form result = _bridge.EvaluateTruthValue(Atom.Sym(expression));
            string certainty = result.IsMarked() ? "Certain (affirmed)"
                : result.IsVoid() ? "Certain (negated)"
                : "Uncertain (imaginary/paradoxical)";

            return Task.FromResult(Result<string, string>.Success($"Certainty of '{expression}': {certainty}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}