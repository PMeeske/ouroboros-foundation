// <copyright file="IntentionBus.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// The IntentionBus manages the flow of autonomous intentions in Ouroboros.
/// It acts as the central nervous system for push-based autonomous behavior.
/// </summary>
public sealed class IntentionBus : IDisposable
{
    private readonly ConcurrentDictionary<Guid, Intention> _intentions = new();
    private readonly ConcurrentQueue<Intention> _pendingQueue = new();
    private readonly Subject<IntentionEvent> _intentionEvents = new();
    private readonly Subject<Intention> _newIntentions = new();
    private readonly SemaphoreSlim _processLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();

    private bool _isActive;
#pragma warning disable CS0649 // Field is assigned dynamically or reserved for future use
    private Task? _processingTask;
#pragma warning restore CS0649
    private Task? _expirationTask;

    /// <summary>
    /// Gets or sets the culture for localized messages (e.g., "de-DE" for German).
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Localizes a message based on the current culture.
    /// </summary>
    private string Localize(string englishMessage)
    {
        if (string.IsNullOrEmpty(Culture) || !Culture.Equals("de-DE", StringComparison.OrdinalIgnoreCase))
            return englishMessage;

        return englishMessage switch
        {
            "🧠 IntentionBus activated. I will now propose actions before executing them."
                => "🧠 IntentionBus aktiviert. Ich werde jetzt Aktionen vorschlagen, bevor ich sie ausführe.",
            _ => englishMessage
        };
    }

    /// <summary>
    /// Localizes a parameterized message.
    /// </summary>
    private string LocalizeWithParam(string templateKey, string param, string? param2 = null)
    {
        if (string.IsNullOrEmpty(Culture) || !Culture.Equals("de-DE", StringComparison.OrdinalIgnoreCase))
        {
            return templateKey switch
            {
                "completed" => $"✅ Intention completed: {param}",
                "failed" => $"❌ Intention failed: {param} - {param2}",
                "proposed" => $"💭 **Intention Proposed:** {param}",
                _ => param
            };
        }

        return templateKey switch
        {
            "completed" => $"✅ Absicht abgeschlossen: {param}",
            "failed" => $"❌ Absicht fehlgeschlagen: {param} - {param2}",
            "proposed" => $"💭 **Absicht vorgeschlagen:** {param}",
            _ => param
        };
    }

    /// <summary>
    /// Observable stream of intention events.
    /// </summary>
    public IObservable<IntentionEvent> IntentionEvents => _intentionEvents;

    /// <summary>
    /// Observable stream of new intentions (for UI notifications).
    /// </summary>
    public IObservable<Intention> NewIntentions => _newIntentions;

    /// <summary>
    /// Event fired when a new intention needs user attention.
    /// </summary>
    public event Action<Intention>? OnIntentionRequiresAttention;

    /// <summary>
    /// Event fired when Ouroboros wants to proactively communicate.
    /// </summary>
    public event Action<string, IntentionPriority>? OnProactiveMessage;

    /// <summary>
    /// Whether the bus is running.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Gets the count of pending intentions.
    /// </summary>
    public int PendingCount => _intentions.Values.Count(i => i.Status == IntentionStatus.Pending);

    /// <summary>
    /// Gets all pending intentions ordered by priority and age.
    /// </summary>
    public IReadOnlyList<Intention> GetPendingIntentions() =>
        _intentions.Values
            .Where(i => i.Status == IntentionStatus.Pending)
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => i.CreatedAt)
            .ToList();

    /// <summary>
    /// Gets all intentions (for history/audit).
    /// </summary>
    public IReadOnlyList<Intention> GetAllIntentions() =>
        _intentions.Values.OrderByDescending(i => i.CreatedAt).ToList();

    /// <summary>
    /// Gets intentions by category.
    /// </summary>
    public IReadOnlyList<Intention> GetIntentionsByCategory(IntentionCategory category) =>
        _intentions.Values.Where(i => i.Category == category).ToList();

    /// <summary>
    /// Starts the intention bus.
    /// </summary>
    public void Start()
    {
        if (_isActive) return;
        _isActive = true;

        _expirationTask = Task.Run(ExpirationLoopAsync);
        OnProactiveMessage?.Invoke(Localize("🧠 IntentionBus activated. I will now propose actions before executing them."), IntentionPriority.Normal);
    }

    /// <summary>
    /// Stops the intention bus.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isActive) return;
        _isActive = false;
        _cts.Cancel();

        if (_processingTask != null) await _processingTask;
        if (_expirationTask != null) await _expirationTask;
    }

    /// <summary>
    /// Proposes a new intention for user approval.
    /// </summary>
    public Intention ProposeIntention(
        string title,
        string description,
        string rationale,
        IntentionCategory category,
        string source,
        IntentionAction? action = null,
        IntentionPriority priority = IntentionPriority.Normal,
        bool requiresApproval = true,
        TimeSpan? expiresIn = null,
        string? target = null)
    {
        var intention = new Intention
        {
            Title = title,
            Description = description,
            Rationale = rationale,
            Category = category,
            Source = source,
            Target = target,
            Action = action,
            Priority = priority,
            RequiresApproval = requiresApproval,
            ExpiresAt = expiresIn.HasValue ? DateTime.UtcNow.Add(expiresIn.Value) : null,
        };

        _intentions[intention.Id] = intention;
        _pendingQueue.Enqueue(intention);
        _newIntentions.OnNext(intention);

        if (requiresApproval)
        {
            OnIntentionRequiresAttention?.Invoke(intention);
            OnProactiveMessage?.Invoke(
                $"💭 **Intention Proposed:** {title}\n" +
                $"   Category: {category}, Priority: {priority}\n" +
                $"   Reason: {rationale}\n" +
                $"   Use `/approve {intention.Id.ToString()[..8]}` or `/reject {intention.Id.ToString()[..8]}`",
                priority);
        }

        _intentionEvents.OnNext(new IntentionEvent(intention, IntentionStatus.Pending, IntentionStatus.Pending, DateTime.UtcNow));
        return intention;
    }

    /// <summary>
    /// Approves an intention for execution.
    /// </summary>
    public bool ApproveIntention(Guid id, string? comment = null)
    {
        if (!_intentions.TryGetValue(id, out var intention)) return false;
        if (intention.Status != IntentionStatus.Pending) return false;

        var updated = intention with
        {
            Status = IntentionStatus.Approved,
            ActedAt = DateTime.UtcNow,
            UserComment = comment,
        };

        _intentions[id] = updated;
        _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Approved, DateTime.UtcNow));
        return true;
    }

    /// <summary>
    /// Approves an intention by partial ID match.
    /// </summary>
    public bool ApproveIntentionByPartialId(string partialId, string? comment = null)
    {
        var intention = _intentions.Values.FirstOrDefault(i =>
            i.Id.ToString().StartsWith(partialId, StringComparison.OrdinalIgnoreCase) &&
            i.Status == IntentionStatus.Pending);

        return intention != null && ApproveIntention(intention.Id, comment);
    }

    /// <summary>
    /// Rejects an intention.
    /// </summary>
    public bool RejectIntention(Guid id, string? reason = null)
    {
        if (!_intentions.TryGetValue(id, out var intention)) return false;
        if (intention.Status != IntentionStatus.Pending) return false;

        var updated = intention with
        {
            Status = IntentionStatus.Rejected,
            ActedAt = DateTime.UtcNow,
            UserComment = reason,
        };

        _intentions[id] = updated;
        _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Rejected, DateTime.UtcNow));
        return true;
    }

    /// <summary>
    /// Rejects an intention by partial ID match.
    /// </summary>
    public bool RejectIntentionByPartialId(string partialId, string? reason = null)
    {
        var intention = _intentions.Values.FirstOrDefault(i =>
            i.Id.ToString().StartsWith(partialId, StringComparison.OrdinalIgnoreCase) &&
            i.Status == IntentionStatus.Pending);

        return intention != null && RejectIntention(intention.Id, reason);
    }

    /// <summary>
    /// Gets the next approved intention ready for execution.
    /// </summary>
    public Intention? GetNextApprovedIntention()
    {
        return _intentions.Values
            .Where(i => i.Status == IntentionStatus.Approved)
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => i.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Marks an intention as executing.
    /// </summary>
    public bool MarkExecuting(Guid id)
    {
        if (!_intentions.TryGetValue(id, out var intention)) return false;
        if (intention.Status != IntentionStatus.Approved) return false;

        var updated = intention with { Status = IntentionStatus.Executing };
        _intentions[id] = updated;
        _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Executing, DateTime.UtcNow));
        return true;
    }

    /// <summary>
    /// Marks an intention as completed.
    /// </summary>
    public bool MarkCompleted(Guid id, string result)
    {
        if (!_intentions.TryGetValue(id, out var intention)) return false;
        if (intention.Status != IntentionStatus.Executing) return false;

        var updated = intention with
        {
            Status = IntentionStatus.Completed,
            ExecutionResult = result,
            ActedAt = DateTime.UtcNow,
        };

        _intentions[id] = updated;
        _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Completed, DateTime.UtcNow));
        OnProactiveMessage?.Invoke(LocalizeWithParam("completed", intention.Title), IntentionPriority.Low);
        return true;
    }

    /// <summary>
    /// Marks an intention as failed.
    /// </summary>
    public bool MarkFailed(Guid id, string error)
    {
        if (!_intentions.TryGetValue(id, out var intention)) return false;

        var updated = intention with
        {
            Status = IntentionStatus.Failed,
            ExecutionResult = $"FAILED: {error}",
            ActedAt = DateTime.UtcNow,
        };

        _intentions[id] = updated;
        _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Failed, DateTime.UtcNow));
        OnProactiveMessage?.Invoke(LocalizeWithParam("failed", intention.Title, error), IntentionPriority.Normal);
        return true;
    }

    /// <summary>
    /// Approves all pending low-risk intentions.
    /// </summary>
    public int ApproveAllLowRisk()
    {
        var lowRisk = _intentions.Values
            .Where(i => i.Status == IntentionStatus.Pending &&
                        (i.Category == IntentionCategory.SelfReflection ||
                         i.Category == IntentionCategory.Learning ||
                         i.Category == IntentionCategory.MemoryManagement) &&
                        i.Priority <= IntentionPriority.Normal)
            .ToList();

        foreach (var intention in lowRisk)
        {
            ApproveIntention(intention.Id, "Auto-approved as low-risk");
        }

        return lowRisk.Count;
    }

    private async Task ExpirationLoopAsync()
    {
        while (_isActive && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);

                var now = DateTime.UtcNow;
                var expired = _intentions.Values
                    .Where(i => i.Status == IntentionStatus.Pending &&
                                i.ExpiresAt.HasValue &&
                                i.ExpiresAt.Value < now)
                    .ToList();

                foreach (var intention in expired)
                {
                    var updated = intention with { Status = IntentionStatus.Expired };
                    _intentions[intention.Id] = updated;
                    _intentionEvents.OnNext(new IntentionEvent(updated, intention.Status, IntentionStatus.Expired, now));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Gets a summary of the intention bus state.
    /// </summary>
    public string GetSummary()
    {
        var pending = _intentions.Values.Count(i => i.Status == IntentionStatus.Pending);
        var approved = _intentions.Values.Count(i => i.Status == IntentionStatus.Approved);
        var completed = _intentions.Values.Count(i => i.Status == IntentionStatus.Completed);
        var rejected = _intentions.Values.Count(i => i.Status == IntentionStatus.Rejected);
        var failed = _intentions.Values.Count(i => i.Status == IntentionStatus.Failed);

        return $"📊 **IntentionBus Status**\n" +
               $"  Pending: {pending}, Approved: {approved}, Completed: {completed}\n" +
               $"  Rejected: {rejected}, Failed: {failed}, Total: {_intentions.Count}";
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _isActive = false;
        _cts.Cancel();
        _cts.Dispose();
        _intentionEvents.Dispose();
        _newIntentions.Dispose();
        _processLock.Dispose();
    }
}
