// <copyright file="GitReflectionService.Analysis.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using System.Text.RegularExpressions;

namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Code reflection, analysis, and diff reading operations.
/// </summary>
public sealed partial class GitReflectionService
{
    /// <summary>
    /// Lists all source files in the repository.
    /// </summary>
    public async Task<IReadOnlyList<RepoFileInfo>> ListSourceFilesAsync(
        string? filter = null,
        CancellationToken ct = default)
    {
        List<RepoFileInfo> files = new();
        string[] extensions = new[] { "*.cs", "*.json", "*.md", "*.yaml", "*.yml" };

        foreach (string ext in extensions)
        {
            foreach (string file in Directory.EnumerateFiles(_repoRoot, ext, SearchOption.AllDirectories))
            {
                if (file.Contains("bin") || file.Contains("obj") || file.Contains(".git"))
                    continue;

                if (filter != null && !file.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                FileInfo fi = new(file);
                int lineCount = 0;
                try
                {
                    lineCount = (await File.ReadAllLinesAsync(file, ct)).Length;
                }
                catch (IOException)
                {
                    // Ignore read errors
                }

                string language = Path.GetExtension(file).ToLowerInvariant() switch
                {
                    ".cs" => "C#",
                    ".json" => "JSON",
                    ".md" => "Markdown",
                    ".yaml" or ".yml" => "YAML",
                    _ => "Unknown",
                };

                files.Add(new RepoFileInfo(
                    Path.GetRelativePath(_repoRoot, file),
                    file,
                    fi.Length,
                    fi.LastWriteTimeUtc,
                    lineCount,
                    language));
            }
        }

        return files.OrderBy(f => f.RelativePath).ToList();
    }

    /// <summary>
    /// Analyzes a C# source file for reflection.
    /// </summary>
    public async Task<CodeAnalysis> AnalyzeFileAsync(string filePath, CancellationToken ct = default)
    {
        string fullPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_repoRoot, filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        string content = await File.ReadAllTextAsync(fullPath, ct);
        string[] lines = content.Split('\n');

        // Extract classes
        List<string> classes = ClassDeclarationRegex().Matches(content)
            .Select(m => m.Groups[1].Value)
            .ToList();

        // Extract methods
        List<string> methods = MethodDeclarationRegex().Matches(content)
            .Select(m => m.Groups[1].Value)
            .Where(m => !new[] { "if", "for", "while", "switch", "catch" }.Contains(m))
            .Distinct()
            .ToList();

        // Extract usings
        List<string> usings = UsingDirectiveRegex().Matches(content)
            .Select(m => m.Groups[1].Value)
            .ToList();

        // Count lines
        int commentLines = lines.Count(l => l.Trim().StartsWith("//") || l.Trim().StartsWith("/*") || l.Trim().StartsWith('*'));
        int codeLines = lines.Count(l => !string.IsNullOrWhiteSpace(l) && !l.Trim().StartsWith("//"));

        // Find TODOs
        List<string> todos = TodoCommentRegex().Matches(content)
            .Select(m => m.Groups[1].Value.Trim())
            .ToList();

        // Find potential issues
        List<string> issues = new();

        if (content.Contains("throw new NotImplementedException"))
            issues.Add("Contains NotImplementedException - incomplete implementation");
        if (EmptyCatchBlockRegex().IsMatch(content))
            issues.Add("Empty catch block - swallowing exceptions");
        if (content.Contains("// HACK"))
            issues.Add("Contains HACK comment");
        if (lines.Any(l => l.Length > 200))
            issues.Add("Contains very long lines (>200 chars)");
        if (methods.Count > 30)
            issues.Add($"Large file with {methods.Count} methods - consider splitting");

        return new CodeAnalysis(
            filePath,
            classes,
            methods,
            usings,
            lines.Length,
            codeLines,
            commentLines,
            commentLines / (double)Math.Max(1, lines.Length),
            todos,
            issues);
    }

    /// <summary>
    /// Searches code for a pattern across all files.
    /// </summary>
    public async Task<IReadOnlyList<(string File, int Line, string Content)>> SearchCodeAsync(
        string pattern,
        bool isRegex = false,
        CancellationToken ct = default)
    {
        List<(string, int, string)> results = new();
        IReadOnlyList<RepoFileInfo> files = await ListSourceFilesAsync(ct: ct);

        Regex? regex = isRegex ? new Regex(pattern, RegexOptions.IgnoreCase) : null;

        foreach (RepoFileInfo file in files)
        {
            if (file.Language != "C#") continue;

            try
            {
                string[] lines = await File.ReadAllLinesAsync(file.FullPath, ct);
                for (int i = 0; i < lines.Length; i++)
                {
                    bool match = regex != null
                        ? regex.IsMatch(lines[i])
                        : lines[i].Contains(pattern, StringComparison.OrdinalIgnoreCase);

                    if (match)
                    {
                        results.Add((file.RelativePath, i + 1, lines[i].Trim()));
                    }
                }
            }
            catch (IOException)
            {
                // Skip files that can't be read
            }
        }

        return results;
    }

    /// <summary>
    /// Gets the structure overview of the codebase.
    /// </summary>
    public async Task<string> GetCodebaseOverviewAsync(CancellationToken ct = default)
    {
        IReadOnlyList<RepoFileInfo> files = await ListSourceFilesAsync(ct: ct);
        StringBuilder sb = new();

        sb.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║               OUROBOROS CODEBASE OVERVIEW                     ║");
        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");

        // Group by directory
        IOrderedEnumerable<IGrouping<string, RepoFileInfo>> byDir = files
            .GroupBy(f => Path.GetDirectoryName(f.RelativePath) ?? "root")
            .OrderBy(g => g.Key);

        int totalLines = 0;
        int totalFiles = 0;

        foreach (IGrouping<string, RepoFileInfo>? group in byDir)
        {
            int dirLines = group.Sum(f => f.LineCount);
            totalLines += dirLines;
            totalFiles += group.Count();

            if (dirLines > 100) // Only show significant directories
            {
                sb.AppendLine($"║  📁 {group.Key,-40} {group.Count(),4} files {dirLines,6} lines ║");
            }
        }

        sb.AppendLine("╠═══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║  TOTAL: {totalFiles,5} files, {totalLines,7} lines                       ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the current Git status.
    /// </summary>
    public async Task<string> GetStatusAsync(CancellationToken ct = default)
    {
        (bool success, string output, string error) = await ExecuteGitAsync(["status", "--porcelain"], ct);
        if (!success)
        {
            return $"Git status failed: {error}";
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            return "Working tree clean - no uncommitted changes";
        }

        StringBuilder sb = new();
        sb.AppendLine("📊 Git Status:");
        foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string status = line[..2].Trim();
            string file = line[3..];
            string emoji = status switch
            {
                "M" => "📝",
                "A" => "➕",
                "D" => "🗑️",
                "??" => "❓",
                _ => "•",
            };
            sb.AppendLine($"  {emoji} {file}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the current branch name.
    /// </summary>
    public async Task<string> GetCurrentBranchAsync(CancellationToken ct = default)
    {
        (bool success, string output, _) = await ExecuteGitAsync(["branch", "--show-current"], ct);
        return success ? output : "unknown";
    }

    /// <summary>
    /// Gets recent commits.
    /// </summary>
    public async Task<IReadOnlyList<GitCommitInfo>> GetRecentCommitsAsync(
        int count = 10,
        CancellationToken ct = default)
    {
        (bool success, string output, _) = await ExecuteGitAsync(
            ["log", "--oneline", "--date=iso", $"-n{count}", "--format=%h|%s|%ai"], ct);

        if (!success || string.IsNullOrWhiteSpace(output))
        {
            return Array.Empty<GitCommitInfo>();
        }

        List<GitCommitInfo> commits = new();
        foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split('|');
            if (parts.Length >= 3 && DateTime.TryParse(parts[2], out DateTime date))
            {
                commits.Add(new GitCommitInfo(parts[0], parts[1], date));
            }
        }

        return commits;
    }

    /// <summary>
    /// Gets the diff for a file.
    /// </summary>
    public async Task<string> GetFileDiffAsync(string filePath, CancellationToken ct = default)
    {
        (bool success, string output, _) = await ExecuteGitAsync(["diff", "--", filePath], ct);
        return success ? output : "No changes or file not tracked";
    }

    // GeneratedRegex patterns for code analysis

    [GeneratedRegex(@"(?:public|internal|private|protected)\s+(?:sealed|abstract|static|partial)?\s*class\s+(\w+)")]
    private static partial Regex ClassDeclarationRegex();

    [GeneratedRegex(@"(?:public|internal|private|protected)\s+(?:static|virtual|override|async)?\s*(?:\w+(?:<[\w,\s]+>)?)\s+(\w+)\s*\(")]
    private static partial Regex MethodDeclarationRegex();

    [GeneratedRegex(@"using\s+([\w.]+);")]
    private static partial Regex UsingDirectiveRegex();

    [GeneratedRegex(@"//\s*TODO:?\s*(.+)$", RegexOptions.Multiline)]
    private static partial Regex TodoCommentRegex();

    [GeneratedRegex(@"catch\s*\(\s*Exception\s+\w+\s*\)\s*\{\s*\}")]
    private static partial Regex EmptyCatchBlockRegex();
}
