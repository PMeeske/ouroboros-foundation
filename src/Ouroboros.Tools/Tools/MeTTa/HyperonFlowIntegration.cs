// <copyright file="HyperonFlowIntegration.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable SA1101 // Prefix local calls with this

namespace Ouroboros.Tools.MeTTa;

using System.Collections.Concurrent;
using Ouroboros.Core.Hyperon;

/// <summary>
/// Integration layer for Hyperon-based reactive flows.
/// Provides pattern subscriptions, flow orchestration, and consciousness loops.
/// </summary>
public sealed class HyperonFlowIntegration : IAsyncDisposable
{
    private readonly HyperonMeTTaEngine hyperonEngine;
    private readonly ConcurrentDictionary<string, PatternSubscription> subscriptions = new();
    private readonly ConcurrentDictionary<string, HyperonFlow> flows = new();
    private readonly CancellationTokenSource disposeCts = new();
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HyperonFlowIntegration"/> class.
    /// </summary>
    /// <param name="engine">The Hyperon engine to use.</param>
    public HyperonFlowIntegration(HyperonMeTTaEngine engine)
    {
        hyperonEngine = engine ?? throw new ArgumentNullException(nameof(engine));
        hyperonEngine.AtomAdded += OnAtomAdded;
    }

    /// <summary>
    /// Gets the underlying engine.
    /// </summary>
    public HyperonMeTTaEngine Engine => hyperonEngine;

    /// <summary>
    /// Event raised when a pattern is matched.
    /// </summary>
    public event Action<PatternMatch>? OnPatternMatch;

    /// <summary>
    /// Subscribes to a pattern in the AtomSpace.
    /// </summary>
    /// <param name="subscriptionId">Unique subscription identifier.</param>
    /// <param name="pattern">MeTTa pattern to match.</param>
    /// <param name="handler">Handler invoked when pattern matches.</param>
    public void SubscribePattern(string subscriptionId, string pattern, Action<PatternMatch> handler)
    {
        var subscription = new PatternSubscription
        {
            Id = subscriptionId,
            Pattern = pattern,
            Handler = handler,
        };

        subscriptions[subscriptionId] = subscription;
    }

    /// <summary>
    /// Unsubscribes from a pattern.
    /// </summary>
    /// <param name="subscriptionId">The subscription to remove.</param>
    public void UnsubscribePattern(string subscriptionId)
    {
        subscriptions.TryRemove(subscriptionId, out _);
    }

    /// <summary>
    /// Creates a new reasoning flow.
    /// </summary>
    /// <param name="name">Flow name.</param>
    /// <param name="description">Flow description.</param>
    /// <returns>A chainable HyperonFlow.</returns>
    public HyperonFlow CreateFlow(string name, string description)
    {
        var flow = new HyperonFlow(hyperonEngine, name, description);
        flows[name] = flow;
        return flow;
    }

    /// <summary>
    /// Gets a flow by name.
    /// </summary>
    /// <param name="name">The flow name.</param>
    /// <returns>The flow if found, null otherwise.</returns>
    public HyperonFlow? GetFlow(string name)
    {
        return flows.TryGetValue(name, out HyperonFlow? flow) ? flow : null;
    }

    /// <summary>
    /// Executes a named flow.
    /// </summary>
    /// <param name="flowName">Name of the flow to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task completing when flow is executed.</returns>
    public async Task ExecuteFlowAsync(string flowName, CancellationToken ct = default)
    {
        if (flows.TryGetValue(flowName, out HyperonFlow? flow))
        {
            await flow.ExecuteAsync(ct);
        }
    }

    /// <summary>
    /// Creates a consciousness loop that periodically reflects on the AtomSpace.
    /// </summary>
    /// <param name="loopId">Unique loop identifier.</param>
    /// <param name="reflectionDepth">Depth of reflection.</param>
    /// <param name="interval">Interval between reflections.</param>
    /// <returns>CancellationTokenSource to stop the loop.</returns>
    public CancellationTokenSource CreateConsciousnessLoop(
        string loopId,
        int reflectionDepth = 2,
        TimeSpan? interval = null)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        TimeSpan loopInterval = interval ?? TimeSpan.FromSeconds(5);

        Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await PerformReflectionAsync(loopId, reflectionDepth, cts.Token);
                    await Task.Delay(loopInterval, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Continue loop on errors
                }
            }
        }, cts.Token);

        return cts;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return ValueTask.CompletedTask;
        }

        disposed = true;
        disposeCts.Cancel();
        disposeCts.Dispose();
        hyperonEngine.AtomAdded -= OnAtomAdded;
        subscriptions.Clear();
        flows.Clear();

        return ValueTask.CompletedTask;
    }

    private void OnAtomAdded(Atom atom)
    {
        // Check each subscription for pattern matches
        foreach (PatternSubscription subscription in subscriptions.Values)
        {
            if (TryMatch(atom, subscription.Pattern, out Substitution? bindings))
            {
                var match = new PatternMatch
                {
                    Pattern = subscription.Pattern,
                    SubscriptionId = subscription.Id,
                    Bindings = bindings!,
                    MatchedAtoms = new[] { atom },
                };

                try
                {
                    subscription.Handler(match);
                    OnPatternMatch?.Invoke(match);
                }
                catch
                {
                    // Swallow handler exceptions
                }
            }
        }
    }

    private bool TryMatch(Atom atom, string pattern, out Substitution? bindings)
    {
        bindings = null;

        try
        {
            Result<Atom> parseResult = hyperonEngine.Parser.Parse(pattern);
            if (!parseResult.IsSuccess)
            {
                return false;
            }

            if (parseResult.Value is not Atom parsedAtom)
            {
                return false;
            }

            Substitution? result = Unifier.Unify(parsedAtom, atom);
            if (result is not null && (!result.IsEmpty || parsedAtom.ToSExpr() == atom.ToSExpr()))
            {
                bindings = result;
                return true;
            }
        }
        catch
        {
            // Pattern match failed
        }

        return false;
    }

    private async Task PerformReflectionAsync(string loopId, int depth, CancellationToken ct)
    {
        // Add reflection event
        await hyperonEngine.AddFactAsync($"(Reflection {loopId} {DateTime.UtcNow.Ticks} {depth})", ct);

        // Query for thoughts to reflect on
        Result<string, string> thoughtsResult = await hyperonEngine.ExecuteQueryAsync(
            "(match &self (Thought $content $type) (: $content $type))",
            ct);

        if (thoughtsResult.IsSuccess && !string.IsNullOrEmpty(thoughtsResult.Value))
        {
            // Add meta-cognition atom
            await hyperonEngine.AddFactAsync(
                $"(MetaCognition {loopId} reflecting-on {thoughtsResult.Value})",
                ct);
        }

        // Recursive reflection if depth > 1
        if (depth > 1)
        {
            await hyperonEngine.AddFactAsync(
                $"(DeepReflection {loopId} depth-{depth} {DateTime.UtcNow.Ticks})",
                ct);
        }
    }

    private class PatternSubscription
    {
        public required string Id { get; init; }
        public required string Pattern { get; init; }
        public required Action<PatternMatch> Handler { get; init; }
    }
}