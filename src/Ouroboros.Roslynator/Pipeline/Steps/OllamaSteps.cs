using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using OllamaSharp;
using OllamaSharp.Models;
using Ouroboros.Core.Configuration;

namespace Ouroboros.Roslynator.Pipeline.Steps;

/// <summary>
/// Ollama AI pipeline step implemented as a self-contained Kleisli-like function.
/// Reads configuration from environment variables:
///  - OLLAMA_ENDPOINT (default <see cref="DefaultEndpoints.Ollama"/>)
///  - OLLAMA_MODEL (default "codellama")
/// </summary>
public static class OllamaSteps
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(2),
        BaseAddress = new Uri(
            Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")
            ?? DefaultEndpoints.Ollama),
    };

    private static readonly OllamaApiClient _ollama = new OllamaApiClient(_httpClient);

    private static string Model => Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "codellama";

    /// <summary>
    /// Main Ollama step. Returns the same state if no change is made or on failure.
    /// </summary>
    public static async Task<FixState> GenerateFix(FixState state)
    {
        if (!state.Changes.IsEmpty) return state;

        try
        {
            TextSpan span = state.Diagnostic.Location.SourceSpan;
            SyntaxNode node = state.CurrentRoot.FindNode(span);
            string code = node.ToFullString();

            string prompt = BuildPrompt(state.Diagnostic.Id, state.Diagnostic.GetMessage(), code);

            var request = new GenerateRequest
            {
                Model = Model,
                Prompt = prompt,
                Stream = false,
                Options = new RequestOptions { Temperature = 0.05f, TopP = 0.95f },
            };

            StringBuilder responseBuilder = new StringBuilder();
            await foreach (var chunk in _ollama.GenerateAsync(request, state.CancellationToken).ConfigureAwait(false))
            {
                if (chunk?.Response is not null)
                {
                    responseBuilder.Append(chunk.Response);
                }
            }

            string fixedCode = CleanOutput(responseBuilder.ToString());
            if (string.IsNullOrWhiteSpace(fixedCode) || fixedCode == code) return state;

            // parse produced code back into a SyntaxNode (best-effort)
            SyntaxNode? fixedNode = CSharpSyntaxTree.ParseText(fixedCode).GetRoot().DescendantNodesAndSelf().FirstOrDefault();
            if (fixedNode is null) return state;

            SyntaxNode newRoot = state.CurrentRoot.ReplaceNode(node, fixedNode);
            return state.WithNewRoot(newRoot, "Ollama AI Fix");
        }
        catch (OperationCanceledException) { throw; }
        catch (HttpRequestException ex)
        {
            // Fail gracefully: log and return unchanged state
            System.Diagnostics.Debug.WriteLine($"[OllamaStep] Error: {ex.Message}");
            return state;
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OllamaStep] Error: {ex.Message}");
            return state;
        }
    }

    private static string BuildPrompt(string id, string message, string code) =>
$@"You are a C# expert. Fix this exact compiler diagnostic: [{id}] {message}
Return only the corrected code snippet (no explanation, no markdown code fences).
Code to fix:
```csharp
{code}
```";

    private static string CleanOutput(string? output)
    {
        if (string.IsNullOrWhiteSpace(output)) return string.Empty;
        string clean = Regex.Replace(output, @"^```[a-zA-Z]*\s*", "", RegexOptions.Multiline);
        clean = Regex.Replace(clean, @"\s*```$", "", RegexOptions.Multiline);
        return clean.Trim();
    }
}
