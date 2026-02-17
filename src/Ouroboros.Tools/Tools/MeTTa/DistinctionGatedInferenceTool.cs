using System.Text.Json;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for distinction-gated inference.
/// </summary>
public sealed class DistinctionGatedInferenceTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctionGatedInferenceTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public DistinctionGatedInferenceTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_gated_inference";

    /// <inheritdoc/>
    public string Description =>
        "Performs distinction-gated inference: only derives conclusions if the distinction context is marked (certain). " +
        "Pattern is matched in MeTTa knowledge base, but results are only returned if the gate is certain.";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "context": {
                                            "type": "string",
                                            "description": "The distinction context that gates the inference"
                                        },
                                        "pattern": {
                                            "type": "string",
                                            "description": "The MeTTa pattern to match"
                                        }
                                    },
                                    "required": ["context", "pattern"]
                                }
                                """;

    /// <inheritdoc/>
    public Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(input) ?? new();
            string context = args.TryGetValue("context", out var c) ? c.GetString() ?? "default" : "default";
            string pattern = args.TryGetValue("pattern", out var p) ? p.GetString() ?? "" : "";

            var results = _bridge.DistinctionGatedInference(context, Atom.Sym(pattern)).ToList();
            string output = results.Count > 0
                ? $"Gated inference succeeded: {string.Join(", ", results)}"
                : "Gated inference blocked: distinction not marked (uncertain/negated)";

            return Task.FromResult(Result<string, string>.Success(output));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}