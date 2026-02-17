using System.Text.Json;
using Ouroboros.Domain.SelfModification;

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The code reflection neuron manages code analysis and self-modification.
/// </summary>
public sealed class CodeReflectionNeuron : Neuron
{
    private GitReflectionService? _gitService;
    private DateTime _lastCodeScan = DateTime.MinValue;

    /// <inheritdoc/>
    public override string Id => "neuron.code";

    /// <inheritdoc/>
    public override string Name => "Code Reflection";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.CodeReflection;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "code.*",
        "self.modify",
        "reflection.request",
    };

    /// <summary>
    /// Interval in seconds between automatic code scans.
    /// </summary>
    public int CodeScanIntervalSeconds { get; set; } = 300;

    /// <inheritdoc/>
    protected override void OnStarted()
    {
        _gitService = new GitReflectionService();
    }

    /// <inheritdoc/>
    protected override async Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "code.analyze":
                await HandleCodeAnalyzeAsync(message, ct);
                break;

            case "code.search":
                await HandleCodeSearchAsync(message, ct);
                break;

            case "self.modify":
                await HandleSelfModifyAsync(message, ct);
                break;

            case "reflection.request":
                var status = await _gitService!.GetStatusAsync(ct);
                SendResponse(message, new { GitStatus = status, Service = "active" });
                break;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTickAsync(CancellationToken ct)
    {
        // Periodic code health check
        if ((DateTime.UtcNow - _lastCodeScan).TotalSeconds >= CodeScanIntervalSeconds)
        {
            _lastCodeScan = DateTime.UtcNow;
            await PerformCodeHealthCheckAsync(ct);
        }
    }

    private async Task PerformCodeHealthCheckAsync(CancellationToken ct)
    {
        try
        {
            var status = await _gitService!.GetStatusAsync(ct);

            // If there are uncommitted changes, notify
            if (status.Contains("modified") || status.Contains("new file"))
            {
                SendMessage("code.changes_detected", new { Status = status, Time = DateTime.UtcNow });
            }

            // Periodically propose code improvement analysis
            ProposeIntention(
                "Code Health Check",
                "I want to analyze my codebase for potential improvements, TODOs, and technical debt.",
                "Regular code analysis helps maintain code quality and identifies improvement opportunities.",
                IntentionCategory.CodeModification,
                priority: IntentionPriority.Low);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Code health check failed: {ex.Message}");
        }
    }

    private async Task HandleCodeAnalyzeAsync(NeuronMessage message, CancellationToken ct)
    {
        var filePath = message.Payload?.ToString() ?? "";
        if (string.IsNullOrEmpty(filePath))
        {
            SendResponse(message, new { Error = "File path required" });
            return;
        }

        var analysis = await _gitService!.AnalyzeFileAsync(filePath, ct);
        SendResponse(message, analysis);

        // If issues found, propose fixes
        if (analysis.PotentialIssues.Count > 0)
        {
            ProposeIntention(
                $"Fix Issues in {Path.GetFileName(filePath)}",
                $"Found {analysis.PotentialIssues.Count} potential issues in {filePath}",
                "Fixing code issues improves reliability and maintainability.",
                IntentionCategory.CodeModification,
                priority: IntentionPriority.Normal);
        }
    }

    private async Task HandleCodeSearchAsync(NeuronMessage message, CancellationToken ct)
    {
        var query = message.Payload?.ToString() ?? "";
        var results = await _gitService!.SearchCodeAsync(query, false, ct);

        SendResponse(message, new
        {
            Query = query,
            Count = results.Count,
            Results = results.Take(20).Select(r => new { r.File, r.Line, r.Content })
        });
    }

    private async Task HandleSelfModifyAsync(NeuronMessage message, CancellationToken ct)
    {
        // Self-modification requires explicit approval through intention bus
        var payload = message.Payload as JsonElement? ?? JsonSerializer.Deserialize<JsonElement>(message.Payload?.ToString() ?? "{}");

        if (payload.TryGetProperty("file", out var fileProp) &&
            payload.TryGetProperty("description", out var descProp) &&
            payload.TryGetProperty("old_code", out var oldProp) &&
            payload.TryGetProperty("new_code", out var newProp))
        {
            ProposeIntention(
                $"Code Modification: {descProp.GetString()}",
                $"I want to modify {fileProp.GetString()}: {descProp.GetString()}",
                payload.TryGetProperty("rationale", out var ratProp) ? ratProp.GetString() ?? "Improve code" : "Improve code",
                IntentionCategory.CodeModification,
                new IntentionAction
                {
                    ActionType = "code_change",
                    FilePath = fileProp.GetString(),
                    OldCode = oldProp.GetString(),
                    NewCode = newProp.GetString(),
                },
                IntentionPriority.High);

            SendResponse(message, new { Status = "proposal_created", RequiresApproval = true });
        }
    }
}