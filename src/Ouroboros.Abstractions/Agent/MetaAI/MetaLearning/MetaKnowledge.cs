namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Represents transferable meta-knowledge extracted from learning history.
/// </summary>
public sealed record MetaKnowledge(
    string Domain,
    string Insight,
    double Confidence,
    int SupportingExamples,
    List<string> ApplicableTaskTypes,
    DateTime DiscoveredAt);