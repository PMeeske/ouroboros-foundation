namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// A chainable flow builder for Hyperon reasoning.
/// </summary>
public sealed class HyperonFlow
{
    private readonly HyperonMeTTaEngine hyperonEngine;
    private readonly string flowName;
    private readonly string flowDescription;
    private readonly List<Func<CancellationToken, Task>> steps = new();

    internal HyperonFlow(HyperonMeTTaEngine engine, string name, string description)
    {
        hyperonEngine = engine;
        flowName = name;
        flowDescription = description;
    }

    /// <summary>
    /// Gets the flow name.
    /// </summary>
    public string Name => flowName;

    /// <summary>
    /// Gets the flow description.
    /// </summary>
    public string Description => flowDescription;

    /// <summary>
    /// Loads facts into the AtomSpace.
    /// </summary>
    /// <param name="facts">MeTTa facts to load.</param>
    /// <returns>This flow for chaining.</returns>
    public HyperonFlow LoadFacts(params string[] facts)
    {
        steps.Add(async ct =>
        {
            foreach (string fact in facts)
            {
                await hyperonEngine.AddFactAsync(fact, ct);
            }
        });
        return this;
    }

    /// <summary>
    /// Applies a rule to the AtomSpace.
    /// </summary>
    /// <param name="rule">The MeTTa rule to apply.</param>
    /// <returns>This flow for chaining.</returns>
    public HyperonFlow ApplyRule(string rule)
    {
        steps.Add(async ct =>
        {
            await hyperonEngine.ApplyRuleAsync(rule, ct);
        });
        return this;
    }

    /// <summary>
    /// Executes a query and stores results.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="resultHandler">Handler for query results.</param>
    /// <returns>This flow for chaining.</returns>
    public HyperonFlow Query(string query, Action<string>? resultHandler = null)
    {
        steps.Add(async ct =>
        {
            Result<string, string> result = await hyperonEngine.ExecuteQueryAsync(query, ct);
            if (result.IsSuccess)
            {
                resultHandler?.Invoke(result.Value);
            }
        });
        return this;
    }

    /// <summary>
    /// Adds a custom transformation step.
    /// </summary>
    /// <param name="transform">The transformation function.</param>
    /// <returns>This flow for chaining.</returns>
    public HyperonFlow Transform(Func<HyperonMeTTaEngine, CancellationToken, Task> transform)
    {
        steps.Add(ct => transform(hyperonEngine, ct));
        return this;
    }

    /// <summary>
    /// Adds a side effect step.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>This flow for chaining.</returns>
    public HyperonFlow SideEffect(Action<HyperonMeTTaEngine> action)
    {
        steps.Add(_ =>
        {
            action(hyperonEngine);
            return Task.CompletedTask;
        });
        return this;
    }

    /// <summary>
    /// Executes the flow.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task completing when flow is done.</returns>
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        foreach (Func<CancellationToken, Task> step in steps)
        {
            ct.ThrowIfCancellationRequested();
            await step(ct);
        }
    }
}