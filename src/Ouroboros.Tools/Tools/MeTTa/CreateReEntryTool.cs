using System.Text.Json;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for creating re-entry (self-referential) forms.
/// </summary>
public sealed class CreateReEntryTool : ITool
{
    private readonly FormMeTTaBridge _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateReEntryTool"/> class.
    /// </summary>
    /// <param name="bridge">The FormMeTTaBridge to use.</param>
    public CreateReEntryTool(FormMeTTaBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    /// <inheritdoc/>
    public string Name => "lof_create_reentry";

    /// <inheritdoc/>
    public string Description =>
        "Creates a re-entry (self-referential) form in Laws of Form. " +
        "Re-entry produces the Imaginary state - representing oscillation, paradox, or self-reference. " +
        "Use for modeling recursive or self-modifying concepts.";

    /// <inheritdoc/>
    public string JsonSchema => """
                                {
                                    "type": "object",
                                    "properties": {
                                        "context": {
                                            "type": "string",
                                            "description": "The context name for the re-entry"
                                        },
                                        "depth": {
                                            "type": "integer",
                                            "description": "The depth of self-reference (default: 1)"
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

            Form result = _bridge.CreateReEntry(context);
            string formState = result.IsImaginary() ? "Imaginary (self-referential)"
                : result.IsMarked() ? "Mark (collapsed to certain)"
                : "Void (collapsed to negated)";

            return Task.FromResult(Result<string, string>.Success($"Re-entry created in '{context}': {formState}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string, string>.Failure($"Error: {ex.Message}"));
        }
    }
}