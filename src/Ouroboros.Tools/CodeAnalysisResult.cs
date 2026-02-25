using Microsoft.CodeAnalysis;

namespace Ouroboros.Tools;

/// <summary>
/// Result of code analysis.
/// </summary>
public class CodeAnalysisResult
{
    public string[] Classes { get; }
    public string[] Methods { get; }
    public Diagnostic[] Diagnostics { get; }
    public string[] Findings { get; }
    public bool IsValid => !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    public CodeAnalysisResult(string[] classes, string[] methods, Diagnostic[] diagnostics, string[]? findings = null)
    {
        Classes = classes;
        Methods = methods;
        Diagnostics = diagnostics;
        Findings = findings ?? Array.Empty<string>();
    }
}