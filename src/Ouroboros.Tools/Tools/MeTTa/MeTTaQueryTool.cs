// <copyright file="MeTTaTools.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Tool for executing MeTTa symbolic queries.
/// </summary>
public sealed class MeTTaQueryTool : ITool
{
    private readonly IMeTTaEngine engine;

    /// <inheritdoc />
    public string Name => "metta_query";

    /// <inheritdoc />
    public string Description => "Execute a MeTTa symbolic query against the knowledge base. Returns symbolic reasoning results.";

    /// <inheritdoc />
    public string? JsonSchema => @"{
        ""type"": ""object"",
        ""properties"": {
            ""query"": {
                ""type"": ""string"",
                ""description"": ""The MeTTa query expression to execute""
            }
        },
        ""required"": [""query""]
    }";

    /// <summary>
    /// Initializes a new instance of the <see cref="MeTTaQueryTool"/> class.
    /// Creates a new MeTTa query tool.
    /// </summary>
    /// <param name="engine">The MeTTa engine to use for query execution.</param>
    public MeTTaQueryTool(IMeTTaEngine engine)
    {
        this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<string, string>.Failure("Query cannot be empty");
        }

        // Parse JSON if provided, otherwise treat as direct query
        string query;
        try
        {
            System.Text.Json.JsonDocument json = System.Text.Json.JsonDocument.Parse(input);
            if (json.RootElement.TryGetProperty("query", out System.Text.Json.JsonElement queryProp))
            {
                query = queryProp.GetString() ?? input;
            }
            else
            {
                query = input;
            }
        }
        catch
        {
            // If not valid JSON, use input directly
            query = input;
        }

        return await this.engine.ExecuteQueryAsync(query, ct);
    }
}