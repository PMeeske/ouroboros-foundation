// <copyright file="StandaloneCompilationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Reflection;

namespace Ouroboros.Abstractions.Tests;

/// <summary>
/// Integration tests verifying that Ouroboros.Abstractions can be compiled
/// and consumed as a standalone package without referencing Ouroboros.Core or
/// other Engine dependencies.
/// </summary>
[Trait("Category", "Integration")]
public class StandaloneCompilationTests
{
    [Fact]
    public void AbstractionsAssembly_CanBeLoadedStandalone()
    {
        // Verify the Abstractions assembly exists and can be loaded
        var assembly = typeof(Ouroboros.Abstractions.Unit).Assembly;
        
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("Ouroboros.Abstractions");
    }

    [Fact]
    public void AbstractionsAssembly_DoesNotReferenceCoreOrEngine()
    {
        // Verify that Abstractions does not have dependencies on Core, Domain, or other non-foundation assemblies
        var assembly = typeof(Ouroboros.Abstractions.Unit).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();
        
        var coreReferences = referencedAssemblies
            .Where(a => a.Name != null && 
                       (a.Name.Contains("Ouroboros.Core", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Ouroboros.Domain", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Ouroboros.Tools", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Ouroboros.Agent", StringComparison.OrdinalIgnoreCase) ||
                        a.Name.Contains("Ouroboros.Engine", StringComparison.OrdinalIgnoreCase)))
            .ToList();
        
        coreReferences.Should().BeEmpty(
            "Ouroboros.Abstractions should not reference Core, Domain, Tools, Agent, or Engine assemblies");
    }

    [Fact]
    public void AbstractionsMonads_CanBeUsedStandalone()
    {
        // Verify that Abstractions monads can be used without Core dependencies
        
        // Test Option monad
        var someOption = Ouroboros.Abstractions.Monads.Option<int>.Some(42);
        someOption.HasValue.Should().BeTrue();
        
        var noneOption = Ouroboros.Abstractions.Monads.Option<int>.None();
        noneOption.HasValue.Should().BeFalse();
        
        // Test Result monad
        var successResult = Ouroboros.Abstractions.Monads.Result<int, string>.Success(100);
        successResult.IsSuccess.Should().BeTrue();
        
        var failureResult = Ouroboros.Abstractions.Monads.Result<int, string>.Failure("error");
        failureResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AbstractionsUnit_CanBeUsedStandalone()
    {
        // Verify that the Unit type can be used
        var unit = Ouroboros.Abstractions.Unit.Value;
        
        unit.Should().NotBeNull();
    }

    [Fact]
    public void AbstractionsInterfaces_AreAccessible()
    {
        // Verify that key interfaces from Abstractions are accessible
        var assembly = typeof(Ouroboros.Abstractions.Unit).Assembly;
        
        var interfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.IsPublic)
            .ToList();
        
        interfaces.Should().NotBeEmpty();
        
        // Verify specific important interfaces exist
        interfaces.Should().Contain(t => t.Name == "IChatCompletionModel");
        interfaces.Should().Contain(t => t.Name.StartsWith("IOrchestrator"));
    }

    [Fact]
    public void AbstractionsNamespaces_AreWellOrganized()
    {
        // Verify that Abstractions namespaces follow expected structure
        var assembly = typeof(Ouroboros.Abstractions.Unit).Assembly;
        var namespaces = assembly.GetTypes()
            .Where(t => t.Namespace != null)
            .Select(t => t.Namespace)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
        
        namespaces.Should().NotBeEmpty();
        
        // Verify key namespaces exist
        namespaces.Should().Contain(n => n!.StartsWith("Ouroboros.Abstractions"));
        namespaces.Should().Contain(n => n == "Ouroboros.Abstractions.Monads");
        namespaces.Should().Contain(n => n!.StartsWith("Ouroboros.Agent"));
    }

    [Fact]
    public void AbstractionsTypes_DoNotLeakEngineDependencies()
    {
        // Verify that public types in Abstractions don't expose Engine-specific types
        var assembly = typeof(Ouroboros.Abstractions.Unit).Assembly;
        var publicTypes = assembly.GetTypes()
            .Where(t => t.IsPublic)
            .ToList();
        
        foreach (var type in publicTypes)
        {
            // Check properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;
                if (propType.Assembly.GetName().Name?.Contains("Ouroboros") == true)
                {
                    propType.Assembly.GetName().Name.Should().Be("Ouroboros.Abstractions",
                        $"Property {type.Name}.{prop.Name} should not expose types from other Ouroboros assemblies");
                }
            }
            
            // Check methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName); // Exclude property accessors
            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                if (returnType.Assembly.GetName().Name?.Contains("Ouroboros") == true)
                {
                    returnType.Assembly.GetName().Name.Should().Be("Ouroboros.Abstractions",
                        $"Method {type.Name}.{method.Name} return type should not expose types from other Ouroboros assemblies");
                }
            }
        }
    }

    [Fact]
    public void AbstractionsTestProject_OnlyReferencesAbstractions()
    {
        // Verify that this test project only references Abstractions (and test frameworks)
        var assembly = typeof(StandaloneCompilationTests).Assembly;
        var referencedAssemblies = assembly.GetReferencedAssemblies();
        
        var ouroborosReferences = referencedAssemblies
            .Where(a => a.Name != null && a.Name.StartsWith("Ouroboros", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        ouroborosReferences.Should().HaveCount(1);
        ouroborosReferences.Single().Name.Should().Be("Ouroboros.Abstractions");
    }
}
