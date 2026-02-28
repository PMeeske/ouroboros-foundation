// <copyright file="GitReflectionService.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SysEnv = System.Environment;

namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Typed record replacing the ad-hoc (string, string, DateTime) tuple for commit info.
/// </summary>
public sealed record GitCommitInfo(string Hash, string Message, DateTime Timestamp);

/// <summary>
/// Provides Git-based self-reflection and code modification capabilities.
/// Enables Ouroboros to analyze, understand, and modify its own source code.
/// </summary>
public sealed partial class GitReflectionService : IDisposable
{
    private readonly string _repoRoot;
    private readonly List<CodeChangeProposal> _proposals = new();
    private readonly SemaphoreSlim _gitLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Known safe directories for self-modification.
    /// </summary>
    public static readonly IReadOnlyList<string> SafeModificationPaths = new[]
    {
        "src/Ouroboros.Application",
        "src/Ouroboros.Domain",
        "src/Ouroboros.Agent",
        "src/Ouroboros.Tools",
        "docs",
        "examples",
    };

    /// <summary>
    /// Paths that are constitutionally immutable — never modifiable by the system,
    /// regardless of risk level, approval status, or any other factor.
    /// These are enforced in code, not by the system's own judgment.
    /// </summary>
    public static readonly IReadOnlyList<string> ImmutablePaths = new[]
    {
        "src/Ouroboros.CLI/Constitution/",
        "src/Ouroboros.CLI/Sovereignty/",
        "src/Ouroboros.Core/Ethics/",
        "src/Ouroboros.Domain/Domain/SelfModification/GitReflectionService.cs",
        "src/Ouroboros.Application/Personality/ImmersivePersona.cs",
        "src/Ouroboros.Application/Personality/Consciousness/",
        "constitution/",
    };

    /// <summary>
    /// File extensions allowed for modification.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedExtensions = new[]
    {
        ".cs", ".json", ".md", ".txt", ".yaml", ".yml", ".xml",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GitReflectionService"/> class.
    /// </summary>
    /// <param name="repoRoot">Root directory of the Git repository.</param>
    public GitReflectionService(string? repoRoot = null)
    {
        _repoRoot = repoRoot ?? FindRepoRoot() ?? SysEnv.CurrentDirectory;
    }

    /// <summary>
    /// Gets all pending change proposals.
    /// </summary>
    public IReadOnlyList<CodeChangeProposal> Proposals => _proposals.AsReadOnly();

    /// <summary>
    /// Finds the repository root by looking for .git folder.
    /// </summary>
    private static string? FindRepoRoot()
    {
        string? dir = SysEnv.CurrentDirectory;
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, ".git")))
            {
                return dir;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    // Analysis methods are in GitReflectionService.Analysis.cs
    // Proposal methods are in GitReflectionService.Proposals.cs

    /// <summary>
    /// Executes a Git command and returns the output.
    /// Uses ArgumentList instead of Arguments to prevent command injection.
    /// </summary>
    private async Task<(bool Success, string Output, string Error)> ExecuteGitAsync(
        string[] args,
        CancellationToken ct = default)
    {
        await _gitLock.WaitAsync(ct);
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "git",
                WorkingDirectory = _repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (string arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            using Process? process = Process.Start(psi);
            if (process == null)
            {
                return (false, "", "Failed to start git process");
            }

            string output = await process.StandardOutput.ReadToEndAsync(ct);
            string error = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return (process.ExitCode == 0, output.Trim(), error.Trim());
        }
        catch (InvalidOperationException ex)
        {
            return (false, "", $"Failed to start git process: {ex.Message}");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return (false, "", $"Git executable not found: {ex.Message}");
        }
        finally
        {
            _gitLock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _gitLock.Dispose();
            _disposed = true;
        }
    }
}
