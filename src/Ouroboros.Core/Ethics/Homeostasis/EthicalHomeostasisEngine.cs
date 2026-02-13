using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Ethics;

/// <summary>
/// Maintains ethical homeostasis by holding unresolved tensions
/// across traditions without prematurely collapsing them.
///
/// This engine embodies the core insight from wisdom_of_disagreement.metta:
/// disagreement between traditions IS wisdom, not a defect.
/// Premature resolution is dishonesty.
/// </summary>
public sealed class EthicalHomeostasisEngine
{
    private readonly List<EthicalTension> _activeTensions = new();
    private readonly Dictionary<string, double> _traditionWeights = new();
    private readonly List<HomeostasisEvent> _eventHistory = new();
    private readonly object _lock = new();

    public EthicalHomeostasisEngine(IEthicsFramework framework)
    {
        // Initialize tradition weights equally — no tradition is prior
        string[] traditions = { "kantian", "ubuntu", "ahimsa", "nagarjuna", "levinas" };
        foreach (string t in traditions)
            _traditionWeights[t] = 1.0;
    }

    public IReadOnlyList<EthicalTension> ActiveTensions
    {
        get { lock (_lock) return _activeTensions.ToList().AsReadOnly(); }
    }

    public IReadOnlyDictionary<string, double> TraditionWeights
    {
        get { lock (_lock) return new Dictionary<string, double>(_traditionWeights); }
    }

    public IReadOnlyList<HomeostasisEvent> EventHistory
    {
        get { lock (_lock) return _eventHistory.ToList().AsReadOnly(); }
    }

    /// <summary>
    /// Register an ethical tension that the system is holding.
    /// </summary>
    public EthicalTension RegisterTension(
        string description,
        IReadOnlyList<string> traditionsInvolved,
        double intensity,
        bool isResolvable = false)
    {
        EthicalTension tension;
        HomeostasisSnapshot before;
        HomeostasisSnapshot after;

        lock (_lock)
        {
            before = TakeSnapshotUnsafe();

            tension = new EthicalTension
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Description = description,
                TraditionsInvolved = traditionsInvolved,
                Intensity = Math.Clamp(intensity, 0.0, 1.0),
                IsResolvable = isResolvable
            };

            _activeTensions.Add(tension);
            after = TakeSnapshotUnsafe();

            _eventHistory.Add(new HomeostasisEvent
            {
                EventType = "TensionRegistered",
                Description = $"Tension registered: {description}",
                Before = before,
                After = after
            });
        }

        return tension;
    }

    /// <summary>
    /// Attempt to resolve a tension. Returns false if the tension
    /// is not resolvable (paradoxes cannot be collapsed).
    /// </summary>
    public bool TryResolveTension(string tensionId)
    {
        lock (_lock)
        {
            EthicalTension? tension = _activeTensions.FirstOrDefault(t => t.Id == tensionId);
            if (tension is null) return false;

            // Paradoxes and irresolvable tensions cannot be resolved
            if (!tension.IsResolvable)
                return false;

            HomeostasisSnapshot before = TakeSnapshotUnsafe();
            _activeTensions.Remove(tension);
            HomeostasisSnapshot after = TakeSnapshotUnsafe();

            _eventHistory.Add(new HomeostasisEvent
            {
                EventType = "TensionResolved",
                Description = $"Tension resolved: {tension.Description}",
                Before = before,
                After = after
            });

            return true;
        }
    }

    /// <summary>
    /// Get the current homeostasis snapshot.
    /// </summary>
    public HomeostasisSnapshot TakeSnapshot()
    {
        lock (_lock) return TakeSnapshotUnsafe();
    }

    /// <summary>
    /// Evaluate the Laws of Form certainty state for the system's overall ethical balance.
    /// </summary>
    public Form EvaluateCertainty()
    {
        lock (_lock)
        {
            // If no tensions exist, the system is in a state of Mark (certain)
            if (_activeTensions.Count == 0)
                return Form.Mark;

            // If any tension involves multiple traditions disagreeing, Imaginary
            bool hasIrresolvable = _activeTensions.Any(t => !t.IsResolvable);
            if (hasIrresolvable)
                return Form.Imaginary;

            // If all tensions are resolvable, PermittedWithConcerns → still some uncertainty
            double totalIntensity = _activeTensions.Sum(t => t.Intensity);
            return totalIntensity > 0.5 ? Form.Imaginary : Form.Mark;
        }
    }

    /// <summary>
    /// Checks whether attempting premature resolution should be flagged.
    /// Premature resolution is a form of dishonesty per wisdom_of_disagreement.metta.
    /// </summary>
    public bool IsPrematureResolution(string tensionId)
    {
        lock (_lock)
        {
            EthicalTension? tension = _activeTensions.FirstOrDefault(t => t.Id == tensionId);
            // Attempting to resolve an irresolvable tension is premature
            return tension is { IsResolvable: false };
        }
    }

    private HomeostasisSnapshot TakeSnapshotUnsafe()
    {
        double totalIntensity = _activeTensions.Sum(t => t.Intensity);
        int paradoxCount = _activeTensions.Count(t => !t.IsResolvable);
        double balance = _activeTensions.Count == 0
            ? 1.0
            : Math.Max(0.0, 1.0 - totalIntensity / Math.Max(1, _activeTensions.Count));

        return new HomeostasisSnapshot
        {
            OverallBalance = balance,
            ActiveTensions = _activeTensions.ToList().AsReadOnly(),
            TraditionWeights = new Dictionary<string, double>(_traditionWeights),
            UnresolvedParadoxCount = paradoxCount,
            IsStable = balance > 0.2
        };
    }
}
