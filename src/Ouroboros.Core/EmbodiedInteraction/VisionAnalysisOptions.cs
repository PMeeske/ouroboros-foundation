namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Options for vision analysis.
/// </summary>
/// <param name="IncludeDescription">Generate natural language description.</param>
/// <param name="DetectObjects">Detect objects.</param>
/// <param name="DetectFaces">Detect faces.</param>
/// <param name="ClassifyScene">Classify scene type.</param>
/// <param name="ExtractText">Extract text via OCR.</param>
/// <param name="AnalyzeColors">Analyze dominant colors.</param>
/// <param name="MaxObjects">Maximum objects to detect.</param>
/// <param name="ConfidenceThreshold">Minimum confidence threshold.</param>
public sealed record VisionAnalysisOptions(
    bool IncludeDescription = true,
    bool DetectObjects = true,
    bool DetectFaces = true,
    bool ClassifyScene = true,
    bool ExtractText = false,
    bool AnalyzeColors = false,
    int MaxObjects = 20,
    double ConfidenceThreshold = 0.5);