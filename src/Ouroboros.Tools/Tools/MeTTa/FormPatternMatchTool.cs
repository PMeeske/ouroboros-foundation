using System.Text.Json;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for form-based pattern matching.
/// </summary>
public sealed class FormPatternMatchTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormPatternMatchTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public FormPatternMatchTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_pattern_match";

    /// <inheritdoc/>
    public string Description =>
        "Performs Laws of Form-gated pattern matching. Only returns matches where the form " +
        "evaluates to Mark (certain). Uncertain matches are filtered out.";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "pattern": {
                                            "type": "string",
                                            "description": "The MeTTa pattern to match"
                                        },
                                        "template": {
                                            "type": "string",
                                            "description": "The template to apply to matches"
                                        }
                                    },
                                    "required": ["pattern", "template"]
                                }
                                """;

    /// <inheritdoc/>
    public Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(input) ?? new();
            string pattern = args.TryGetValue("pattern", out var p) ? p.GetString() ?? "" : "";

            // FormGatedMatch returns tuples of (Result, State, Bindings) - filter to certain (marked) results
            var results = _bridge.FormGatedMatch(Atom.Sym(pattern))
                .Where(r => r.State.IsMarked())
                .Select(r => r.Result.ToString())
                .ToList();

            string output = results.Count > 0
                ? $"Form-gated matches: {string.Join(", ", results)}"
                : "No certain matches found";

            return Task.FromResult(Result<string, string>.Success(output));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}