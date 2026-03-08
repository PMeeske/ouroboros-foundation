using Microsoft.CodeAnalysis;

namespace Ouroboros.Tools;

/// <summary>
/// Result of code analysis.
/// </summary>
public sealed class CodeAnalysisResult
{
    /// <summary>
    /// Gets the names of all classes discovered during analysis.
    /// </summary>
    public string[] Classes { get; }

    /// <summary>
    /// Gets the names of all methods discovered during analysis.
    /// </summary>
    public string[] Methods { get; }

    /// <summary>
    /// Gets the Roslyn diagnostics (errors, warnings, and informational messages) produced by the analysis.
    /// </summary>
    public Diagnostic[] Diagnostics { get; }

    /// <summary>
    /// Gets additional analysis findings or observations beyond compiler diagnostics.
    /// </summary>
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