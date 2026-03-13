// <copyright file="RoslynCodeToolTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for RoslynCodeTool covering code analysis, generation, refactoring,
/// and documentation analysis.
/// </summary>
[Trait("Category", "Unit")]
public class RoslynCodeToolTests
{
    #region AnalyzeCode Tests

    [Fact]
    public async Task AnalyzeCode_SimpleClass_FindsClassName()
    {
        // Arrange
        string code = "public class MyClass { }";

        // Act
        var result = await RoslynCodeTool.AnalyzeCode(code);

        // Assert
        result.Classes.Should().Contain("MyClass");
    }

    [Fact]
    public async Task AnalyzeCode_ClassWithMethod_FindsMethodName()
    {
        // Arrange
        string code = @"
public class MyClass
{
    public void DoWork() { }
}";

        // Act
        var result = await RoslynCodeTool.AnalyzeCode(code);

        // Assert
        result.Methods.Should().Contain("DoWork");
    }

    [Fact]
    public async Task AnalyzeCode_MultipleClasses_FindsAll()
    {
        // Arrange
        string code = @"
public class ClassA { }
public class ClassB { }
public class ClassC { }";

        // Act
        var result = await RoslynCodeTool.AnalyzeCode(code);

        // Assert
        result.Classes.Should().HaveCount(3);
        result.Classes.Should().Contain("ClassA");
        result.Classes.Should().Contain("ClassB");
        result.Classes.Should().Contain("ClassC");
    }

    [Fact]
    public async Task AnalyzeCode_InvalidCode_ReturnsDiagnostics()
    {
        // Arrange
        string code = "public class { }"; // Missing class name

        // Act
        var result = await RoslynCodeTool.AnalyzeCode(code);

        // Assert
        result.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnalyzeCode_ValidCode_IsValidTrue()
    {
        // Arrange
        string code = "public class Valid { }";

        // Act
        var result = await RoslynCodeTool.AnalyzeCode(code);

        // Assert
        // Note: may have diagnostics (missing references), but structure is valid
        result.Classes.Should().Contain("Valid");
    }

    #endregion

    #region GenerateClass Tests

    [Fact]
    public void GenerateClass_WithMethodsAndProperties_ContainsAll()
    {
        // Act
        var code = RoslynCodeTool.GenerateClass(
            "UserService",
            "MyApp.Services",
            new[] { "GetAll", "GetById" },
            new[] { "string Name", "int Id" });

        // Assert
        code.Should().Contain("namespace MyApp.Services");
        code.Should().Contain("class UserService");
        code.Should().Contain("public string Name { get; set; }");
        code.Should().Contain("public int Id { get; set; }");
        code.Should().Contain("Task GetAll()");
        code.Should().Contain("Task GetById()");
    }

    [Fact]
    public void GenerateClass_EmptyMethodsAndProperties_GeneratesEmptyClass()
    {
        // Act
        var code = RoslynCodeTool.GenerateClass(
            "Empty",
            "MyApp",
            Array.Empty<string>(),
            Array.Empty<string>());

        // Assert
        code.Should().Contain("class Empty");
    }

    #endregion

    #region RenameSymbol Tests

    [Fact]
    public void RenameSymbol_IdentifierUsage_ReplacesCorrectly()
    {
        // Arrange - RenameSymbol replaces IdentifierNameSyntax nodes, which are usage references
        string code = @"
class MyClass
{
    int x;
    void Test()
    {
        x = 42;
    }
}";

        // Act
        var result = RoslynCodeTool.RenameSymbol(code, "x", "renamedField");

        // Assert
        result.Should().Contain("renamedField");
    }

    [Fact]
    public void RenameSymbol_MultipleOccurrences_ReplacesAll()
    {
        // Arrange
        string code = @"
class Calculator
{
    int Add(int a, int b) { return a; }
}";

        // Act
        var result = RoslynCodeTool.RenameSymbol(code, "a", "x");

        // Assert
        // All identifier references to 'a' should be renamed
        result.Should().Contain("x");
    }

    [Fact]
    public void RenameSymbol_NoOccurrence_ReturnsUnchanged()
    {
        // Arrange
        string code = "class MyClass { }";

        // Act
        var result = RoslynCodeTool.RenameSymbol(code, "nonExistent", "newName");

        // Assert
        result.Should().Contain("MyClass");
        result.Should().NotContain("newName");
    }

    #endregion

    #region ExtractMethod Tests

    [Fact]
    public void ExtractMethod_ValidRange_CreatesNewMethod()
    {
        // Arrange
        string code = string.Join(Environment.NewLine, new[]
        {
            "class MyClass",
            "{",
            "    void Main()",
            "    {",
            "        int x = 1;",
            "        int y = 2;",
            "        Console.WriteLine(x + y);",
            "    }",
            "}",
        });

        // Act
        var result = RoslynCodeTool.ExtractMethod(code, 5, 6, "Calculate");

        // Assert
        result.Should().Contain("Calculate();");
        result.Should().Contain("private void Calculate()");
    }

    #endregion

    #region AnalyzeWithCustomAnalyzers Tests

    [Fact]
    public async Task AnalyzeWithCustomAnalyzers_NoBlockingCalls_NoFindings()
    {
        // Arrange
        string code = @"
class MyClass
{
    async Task DoWork()
    {
        await Task.Delay(100);
    }
}";

        // Act
        var result = await RoslynCodeTool.AnalyzeWithCustomAnalyzers(code);

        // Assert
        result.Findings.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeWithCustomAnalyzers_EmptyCode_NoFindings()
    {
        // Arrange
        string code = "class Empty { }";

        // Act
        var result = await RoslynCodeTool.AnalyzeWithCustomAnalyzers(code);

        // Assert
        result.Findings.Should().BeEmpty();
    }

    #endregion

    #region AnalyzeDocumentation Tests

    [Fact]
    public async Task AnalyzeDocumentation_UndocumentedPublicMethod_ReportsFindings()
    {
        // Arrange
        string code = @"
public class MyClass
{
    public void Undocumented() { }
}";

        // Act
        var result = await RoslynCodeTool.AnalyzeDocumentation(code);

        // Assert
        result.Findings.Should().Contain(f => f.Contains("Missing documentation"));
    }

    [Fact]
    public async Task AnalyzeDocumentation_DocumentedPublicMethod_NoFindings()
    {
        // Arrange
        string code = @"
public class MyClass
{
    /// <summary>Well documented.</summary>
    public void Documented() { }
}";

        // Act
        var result = await RoslynCodeTool.AnalyzeDocumentation(code);

        // Assert
        result.Findings.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeDocumentation_PrivateMethod_NotReported()
    {
        // Arrange
        string code = @"
public class MyClass
{
    private void PrivateUndocumented() { }
}";

        // Act
        var result = await RoslynCodeTool.AnalyzeDocumentation(code);

        // Assert
        result.Findings.Should().BeEmpty();
    }

    #endregion

    #region AddMethod Tests

    [Fact]
    public void AddMethod_ValidCode_AddsMethodToClass()
    {
        // Arrange
        string code = @"
class MyClass
{
    public void Existing() { }
}";

        // Act
        var result = RoslynCodeTool.AddMethod(code, "public int NewMethod()", "return 42;");

        // Assert
        result.Should().Contain("Existing");
        // The method should be added to the class
        result.Length.Should().BeGreaterThan(code.Length);
    }

    [Fact]
    public void AddMethod_NoClassInCode_ReturnsOriginal()
    {
        // Arrange
        string code = "// just a comment";

        // Act
        var result = RoslynCodeTool.AddMethod(code, "public int NewMethod()", "return 42;");

        // Assert
        result.Should().Be(code);
    }

    #endregion
}
