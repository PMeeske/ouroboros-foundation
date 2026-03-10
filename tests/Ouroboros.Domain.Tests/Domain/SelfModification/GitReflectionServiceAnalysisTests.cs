// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.SelfModification;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ouroboros.Domain.SelfModification;
using Xunit;

/// <summary>
/// Tests for GitReflectionService.Analysis.cs — code analysis, file listing,
/// codebase overview, status, branch, commits, and diff methods.
/// </summary>
[Trait("Category", "Unit")]
public class GitReflectionServiceAnalysisTests : IDisposable
{
    private readonly string _tempDir;
    private readonly GitReflectionService _sut;

    public GitReflectionServiceAnalysisTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-analysis-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _sut = new GitReflectionService(_tempDir);
    }

    public void Dispose()
    {
        _sut.Dispose();
        try { Directory.Delete(_tempDir, true); }
        catch { /* best-effort cleanup */ }
    }

    // ----------------------------------------------------------------
    // ListSourceFilesAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ListSourceFilesAsync_EmptyDirectory_ReturnsEmpty()
    {
        var files = await _sut.ListSourceFilesAsync();

        files.Should().BeEmpty();
    }

    [Fact]
    public async Task ListSourceFilesAsync_WithCsFiles_ReturnsFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "Test.cs"), "public class Test {}");

        var files = await _sut.ListSourceFilesAsync();

        files.Should().HaveCount(1);
        files[0].Language.Should().Be("C#");
        files[0].LineCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ListSourceFilesAsync_WithFilter_FiltersByName()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "Foo.cs"), "class Foo {}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "Bar.cs"), "class Bar {}");

        var files = await _sut.ListSourceFilesAsync(filter: "Foo");

        files.Should().HaveCount(1);
        files[0].RelativePath.Should().Contain("Foo");
    }

    [Fact]
    public async Task ListSourceFilesAsync_ExcludesBinAndObjDirs()
    {
        var binDir = Path.Combine(_tempDir, "bin");
        Directory.CreateDirectory(binDir);
        await File.WriteAllTextAsync(Path.Combine(binDir, "Output.cs"), "class X {}");

        var files = await _sut.ListSourceFilesAsync();

        files.Should().BeEmpty();
    }

    [Fact]
    public async Task ListSourceFilesAsync_IncludesJsonFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "config.json"), "{}");

        var files = await _sut.ListSourceFilesAsync();

        files.Should().HaveCount(1);
        files[0].Language.Should().Be("JSON");
    }

    [Fact]
    public async Task ListSourceFilesAsync_IncludesMarkdownFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "README.md"), "# Title");

        var files = await _sut.ListSourceFilesAsync();

        files.Should().HaveCount(1);
        files[0].Language.Should().Be("Markdown");
    }

    [Fact]
    public async Task ListSourceFilesAsync_IncludesYamlFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "config.yaml"), "key: value");

        var files = await _sut.ListSourceFilesAsync();

        files.Should().HaveCount(1);
        files[0].Language.Should().Be("YAML");
    }

    // ----------------------------------------------------------------
    // AnalyzeFileAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task AnalyzeFileAsync_ValidCsFile_ReturnsAnalysis()
    {
        string code = """
            using System;

            // TODO: Implement this
            public class MyClass
            {
                public void MyMethod()
                {
                    Console.WriteLine("Hello");
                }
            }
            """;
        string filePath = Path.Combine(_tempDir, "MyClass.cs");
        await File.WriteAllTextAsync(filePath, code);

        var analysis = await _sut.AnalyzeFileAsync(filePath);

        analysis.Should().NotBeNull();
        analysis.FilePath.Should().Be(filePath);
        analysis.Classes.Should().Contain("MyClass");
        analysis.Methods.Should().Contain("MyMethod");
        analysis.Usings.Should().Contain("System");
        analysis.Todos.Should().ContainSingle(t => t.Contains("Implement this"));
        analysis.TotalLines.Should().BeGreaterThan(0);
        analysis.CodeLines.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AnalyzeFileAsync_FileWithNotImplementedException_DetectsIssue()
    {
        string code = """
            public class Test
            {
                public void MyMethod() => throw new NotImplementedException();
            }
            """;
        string filePath = Path.Combine(_tempDir, "Test.cs");
        await File.WriteAllTextAsync(filePath, code);

        var analysis = await _sut.AnalyzeFileAsync(filePath);

        analysis.Issues.Should().Contain(i => i.Contains("NotImplementedException"));
    }

    [Fact]
    public async Task AnalyzeFileAsync_FileWithHack_DetectsIssue()
    {
        string code = """
            public class Test
            {
                // HACK this is a workaround
                public void MyMethod() { }
            }
            """;
        string filePath = Path.Combine(_tempDir, "Test.cs");
        await File.WriteAllTextAsync(filePath, code);

        var analysis = await _sut.AnalyzeFileAsync(filePath);

        analysis.Issues.Should().Contain(i => i.Contains("HACK"));
    }

    [Fact]
    public async Task AnalyzeFileAsync_NonexistentFile_ThrowsFileNotFoundException()
    {
        Func<Task> act = async () => await _sut.AnalyzeFileAsync("nonexistent.cs");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AnalyzeFileAsync_RelativePath_ResolvesFromRepoRoot()
    {
        string code = "public class Rel {}";
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "Relative.cs"), code);

        var analysis = await _sut.AnalyzeFileAsync("Relative.cs");

        analysis.Classes.Should().Contain("Rel");
    }

    // ----------------------------------------------------------------
    // GetCodebaseOverviewAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetCodebaseOverviewAsync_ReturnsFormattedOverview()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "Test.cs"),
            string.Join("\n", Enumerable.Repeat("// line", 200)));

        string overview = await _sut.GetCodebaseOverviewAsync();

        overview.Should().Contain("CODEBASE OVERVIEW");
        overview.Should().Contain("TOTAL:");
    }

    // ----------------------------------------------------------------
    // Static path lists
    // ----------------------------------------------------------------

    [Fact]
    public void SafeModificationPaths_ContainsExpectedPaths()
    {
        GitReflectionService.SafeModificationPaths.Should().Contain("src/Ouroboros.Domain");
        GitReflectionService.SafeModificationPaths.Should().Contain("docs");
    }

    [Fact]
    public void ImmutablePaths_ContainsEthicsCore()
    {
        GitReflectionService.ImmutablePaths.Should().Contain("src/Ouroboros.Core/Ethics/");
    }

    [Fact]
    public void ImmutablePaths_ContainsConstitution()
    {
        GitReflectionService.ImmutablePaths.Should().Contain("constitution/");
    }

    [Fact]
    public void AllowedExtensions_ContainsCommonExtensions()
    {
        GitReflectionService.AllowedExtensions.Should().Contain(".cs");
        GitReflectionService.AllowedExtensions.Should().Contain(".json");
        GitReflectionService.AllowedExtensions.Should().Contain(".md");
    }

    // ----------------------------------------------------------------
    // Constructor
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullRepoRoot_UsesCurrentDirectory()
    {
        // Should not throw
        using var service = new GitReflectionService(null);
        service.Should().NotBeNull();
    }

    [Fact]
    public void Proposals_InitiallyEmpty()
    {
        _sut.Proposals.Should().BeEmpty();
    }
}
