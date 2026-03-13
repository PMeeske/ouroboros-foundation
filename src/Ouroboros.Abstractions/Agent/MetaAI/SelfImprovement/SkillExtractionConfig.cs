using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Configuration for skill extraction behavior.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SkillExtractionConfig(
    double MinQualityThreshold = 0.8,
    int MinStepsForExtraction = 2,
    int MaxStepsPerSkill = 10,
    bool EnableAutoParameterization = true,
    bool EnableSkillVersioning = true);
