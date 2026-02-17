namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Configuration for skill extraction behavior.
/// </summary>
public sealed record SkillExtractionConfig(
    double MinQualityThreshold = 0.8,
    int MinStepsForExtraction = 2,
    int MaxStepsPerSkill = 10,
    bool EnableAutoParameterization = true,
    bool EnableSkillVersioning = true);