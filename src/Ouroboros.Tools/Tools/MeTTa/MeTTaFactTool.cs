using Ouroboros.Abstractions;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for adding facts to the MeTTa knowledge base.
/// </summary>
public sealed class MeTTaFactTool : ITool
{
    private readonly IMeTTaEngine engine;

    /// <inheritdoc />
    public string Name => "metta_add_fact";

    /// <inheritdoc />
    public string Description => "Add a fact to the MeTTa knowledge base for symbolic reasoning.";

    /// <inheritdoc />
    public string? JsonSchema => @"{
        ""type"": ""object"",
        ""properties"": {
            ""fact"": {
                ""type"": ""string"",
                ""description"": ""The fact to add in MeTTa format""
            }
        },
        ""required"": [""fact""]
    }";

    /// <summary>
    /// Initializes a new instance of the <see cref="MeTTaFactTool"/> class.
    /// Creates a new MeTTa fact tool.
    /// </summary>
    /// <param name="engine">The MeTTa engine to use.</param>
    public MeTTaFactTool(IMeTTaEngine engine)
    {
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<string, string>.Failure("Fact cannot be empty");
        }

        // Parse JSON if provided
        string fact;
        try
        {
            System.Text.Json.JsonDocument json = System.Text.Json.JsonDocument.Parse(input);
            if (json.RootElement.TryGetProperty("fact", out System.Text.Json.JsonElement factProp))
            {
                fact = factProp.GetString() ?? input;
            }
            else
            {
                fact = input;
            }
        }
        catch
        {
            fact = input;
        }

        Result<Unit, string> result = await this.engine.AddFactAsync(fact, ct);

        return result.Match(
            _ => Result<string, string>.Success("Fact added successfully"),
            error => Result<string, string>.Failure(error));
    }
}