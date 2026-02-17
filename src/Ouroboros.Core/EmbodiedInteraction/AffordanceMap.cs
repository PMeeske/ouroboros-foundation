namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Collection of affordances detected in the environment.
/// </summary>
public sealed class AffordanceMap
{
    private readonly Dictionary<string, List<Affordance>> _objectAffordances = new();
    private readonly Dictionary<AffordanceType, List<Affordance>> _typeIndex = new();

    /// <summary>
    /// Gets all affordances in the map.
    /// </summary>
    public IEnumerable<Affordance> All => _objectAffordances.Values.SelectMany(x => x);

    /// <summary>
    /// Gets the count of affordances.
    /// </summary>
    public int Count => _objectAffordances.Values.Sum(x => x.Count);

    /// <summary>
    /// Adds an affordance to the map.
    /// </summary>
    public void Add(Affordance affordance)
    {
        if (!_objectAffordances.TryGetValue(affordance.TargetObjectId, out var objectList))
        {
            objectList = new List<Affordance>();
            _objectAffordances[affordance.TargetObjectId] = objectList;
        }

        objectList.Add(affordance);

        if (!_typeIndex.TryGetValue(affordance.Type, out var typeList))
        {
            typeList = new List<Affordance>();
            _typeIndex[affordance.Type] = typeList;
        }

        typeList.Add(affordance);
    }

    /// <summary>
    /// Gets affordances for a specific object.
    /// </summary>
    public Option<IReadOnlyList<Affordance>> GetForObject(string objectId) =>
        _objectAffordances.TryGetValue(objectId, out var list)
            ? Option<IReadOnlyList<Affordance>>.Some(list)
            : Option<IReadOnlyList<Affordance>>.None();

    /// <summary>
    /// Gets affordances of a specific type.
    /// </summary>
    public IReadOnlyList<Affordance> GetByType(AffordanceType type) =>
        _typeIndex.TryGetValue(type, out var list) ? list : Array.Empty<Affordance>();

    /// <summary>
    /// Finds affordances matching criteria.
    /// </summary>
    public IEnumerable<Affordance> Find(
        Func<Affordance, bool> predicate,
        int? limit = null)
    {
        var query = All.Where(predicate).OrderByDescending(a => a.RiskAdjustedConfidence);
        return limit.HasValue ? query.Take(limit.Value) : query;
    }

    /// <summary>
    /// Gets the most confident affordance for an action verb.
    /// </summary>
    public Option<Affordance> GetBestForAction(string actionVerb) =>
        All.Where(a => a.ActionVerb.Equals(actionVerb, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.RiskAdjustedConfidence)
            .FirstOrDefault()
            ?.Let(a => Option<Affordance>.Some(a)) ?? Option<Affordance>.None();

    /// <summary>
    /// Clears all affordances.
    /// </summary>
    public void Clear()
    {
        _objectAffordances.Clear();
        _typeIndex.Clear();
    }

    /// <summary>
    /// Removes stale affordances older than the specified age.
    /// </summary>
    public int RemoveStale(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        var removed = 0;

        foreach (var list in _objectAffordances.Values)
        {
            removed += list.RemoveAll(a => a.DetectedAt < cutoff);
        }

        foreach (var list in _typeIndex.Values)
        {
            list.RemoveAll(a => a.DetectedAt < cutoff);
        }

        return removed;
    }
}