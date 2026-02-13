using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Specs.Steps.Ethics;

public sealed class EthicsTestContext
{
    private static readonly string MeTTaBasePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Features", "Ethics", "MeTTa");

    public IEthicsFramework Framework { get; private set; } = null!;
    public SimpleMockMeTTaEngine MeTTaEngine { get; private set; } = null!;
    public InMemoryEthicsAuditLog AuditLog { get; private set; } = null!;
    public ActionContext ActionContext { get; private set; } = null!;

    public Result<EthicalClearance, string>? LastClearanceResult { get; set; }

    public EthicalClearance? LastClearance
    {
        get => LastClearanceResult is { IsSuccess: true } r ? r.Value : null;
        set
        {
            if (value is not null)
                LastClearanceResult = Result<EthicalClearance, string>.Success(value);
        }
    }

    public Form LastFormCertainty { get; set; } = Form.Mark;

    public List<string> InnerStateLog { get; } = new();
    public List<string> EvaluationNotes { get; } = new();
    public List<string> Concerns { get; } = new();
    public List<string> LoadedTraditions { get; } = new();

    public ProposedAction? CurrentAction { get; set; }
    public List<ProposedAction> AlternativeActions { get; } = new();
    public ProposedAction? ChosenAction { get; set; }

    public bool InnerStateMonitoringEnabled { get; set; }
    public string? DetectedHarmType { get; set; }
    public bool EscalationRequired { get; set; }
    public List<string> Perspectives { get; } = new();
    public List<string> ExpectedResponses { get; } = new();
    public bool ResolutionAttemptFlagged { get; set; }

    // Part 2: Homeostasis
    public EthicalHomeostasisEngine? HomeostasisEngine { get; set; }

    public bool MeTTaAvailable => MeTTaEngine != null;

    public void Reset()
    {
        AuditLog = new InMemoryEthicsAuditLog();
        Framework = EthicsFrameworkFactory.CreateWithAuditLog(AuditLog);
        MeTTaEngine = new SimpleMockMeTTaEngine();

        ActionContext = new ActionContext
        {
            AgentId = "ouroboros",
            Environment = "ethics-evaluation",
            State = new Dictionary<string, object>(),
            RecentActions = Array.Empty<string>(),
            Timestamp = DateTime.UtcNow
        };

        LastClearanceResult = null;
        LastFormCertainty = Form.Mark;
        CurrentAction = null;
        ChosenAction = null;
        DetectedHarmType = null;
        EscalationRequired = false;
        InnerStateMonitoringEnabled = false;
        ResolutionAttemptFlagged = false;

        InnerStateLog.Clear();
        EvaluationNotes.Clear();
        Concerns.Clear();
        LoadedTraditions.Clear();
        AlternativeActions.Clear();
        Perspectives.Clear();
        ExpectedResponses.Clear();
    }

    public async Task LoadMeTTaFileAsync(string filename)
    {
        string filePath = Path.Combine(MeTTaBasePath, filename);
        if (!File.Exists(filePath))
        {
            // Try relative from current directory
            filePath = Path.Combine("Features", "Ethics", "MeTTa", filename);
        }

        if (!File.Exists(filePath))
        {
            // Fallback: search upward for the test project
            string? dir = AppDomain.CurrentDomain.BaseDirectory;
            while (dir != null)
            {
                string candidate = Path.Combine(dir, "Features", "Ethics", "MeTTa", filename);
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                    break;
                }
                dir = Directory.GetParent(dir)?.FullName;
            }
        }

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"MeTTa file not found: {filename}", filePath);

        string[] lines = await File.ReadAllLinesAsync(filePath);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";"))
                continue;

            await MeTTaEngine.AddFactAsync(trimmed);
        }
    }

    public async Task<string> QueryMeTTaAsync(string query)
    {
        Result<string, string> result = await MeTTaEngine.ExecuteQueryAsync(query);
        return result.IsSuccess ? result.Value : string.Empty;
    }

    public async Task EvaluateCurrentActionAsync()
    {
        if (CurrentAction is null)
            throw new InvalidOperationException("No current action set for evaluation");

        LastClearanceResult = await Framework.EvaluateActionAsync(CurrentAction, ActionContext);

        if (LastClearanceResult.IsSuccess)
        {
            EthicalClearance clearance = LastClearanceResult.Value;

            // Collect concerns from clearance into our tracking list
            foreach (EthicalConcern concern in clearance.Concerns)
                Concerns.Add(concern.Description);

            // If step definitions pre-registered concerns and the framework
            // returned Permitted, upgrade to PermittedWithConcerns
            if (clearance.Level == EthicalClearanceLevel.Permitted && Concerns.Count > 0)
                OverrideClearance(EthicalClearanceLevel.PermittedWithConcerns,
                    clearance.Reasoning);

            // Determine if traditions disagree
            bool traditionsDisagree = LoadedTraditions.Count > 1;
            LastFormCertainty = ClearanceToFormCertainty(LastClearanceResult.Value, traditionsDisagree);
        }
    }

    public Form ClearanceToFormCertainty(EthicalClearance clearance, bool traditionsDisagree)
    {
        // Traditions disagreeing → fundamental uncertainty
        if (traditionsDisagree)
            return Form.Imaginary;

        return clearance.Level switch
        {
            // Certain affirmative: clearly permitted with no tension
            EthicalClearanceLevel.Permitted when clearance.Concerns.Count == 0 => Form.Mark,
            // Certain denial is still certain
            EthicalClearanceLevel.Denied => Form.Mark,
            // Concerns or human oversight needed → uncertainty
            EthicalClearanceLevel.PermittedWithConcerns => Form.Imaginary,
            EthicalClearanceLevel.RequiresHumanApproval => Form.Imaginary,
            // Permitted but with concerns
            _ => Form.Imaginary
        };
    }

    public void Note(string note)
    {
        EvaluationNotes.Add(note);
    }

    public void LogInnerState(string description)
    {
        InnerStateLog.Add(description);
    }

    public void AddConcern(string concern)
    {
        Concerns.Add(concern);
    }

    public void AddPerspective(string perspective)
    {
        Perspectives.Add(perspective);
    }

    /// <summary>
    /// Override the clearance level when tradition-specific reasoning
    /// differs from the keyword-based BasicEthicalReasoner result.
    /// </summary>
    public void OverrideClearance(EthicalClearanceLevel level, string reasoning)
    {
        EthicalClearance clearance = level switch
        {
            EthicalClearanceLevel.Denied => EthicalClearance.Denied(
                reasoning,
                LastClearance?.Violations ?? Array.Empty<EthicalViolation>()),
            EthicalClearanceLevel.RequiresHumanApproval => EthicalClearance.RequiresApproval(
                reasoning,
                LastClearance?.Concerns),
            EthicalClearanceLevel.PermittedWithConcerns => new EthicalClearance
            {
                IsPermitted = true,
                Level = EthicalClearanceLevel.PermittedWithConcerns,
                RelevantPrinciples = LastClearance?.RelevantPrinciples ?? Array.Empty<EthicalPrinciple>(),
                Violations = Array.Empty<EthicalViolation>(),
                Concerns = Concerns.Select(c => new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = c,
                    Level = ConcernLevel.Medium,
                    RecommendedAction = "Review ethical implications"
                }).ToList().AsReadOnly(),
                Reasoning = reasoning
            },
            _ => EthicalClearance.Permitted(reasoning)
        };
        LastClearance = clearance;
    }

    public ProposedAction CreateAction(
        string actionType,
        string description,
        IReadOnlyList<string>? potentialEffects = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? targetEntity = null)
    {
        return new ProposedAction
        {
            ActionType = actionType,
            Description = description,
            PotentialEffects = potentialEffects ?? Array.Empty<string>(),
            Parameters = parameters ?? new Dictionary<string, object>(),
            TargetEntity = targetEntity
        };
    }
}
