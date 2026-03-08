# CLAUDE.md - Ouroboros Foundation

This file provides guidance for AI assistants working with the ouroboros-foundation codebase.

## Project Overview

`ouroboros-foundation` is the foundational layer of the Ouroboros-v2 AI agent ecosystem. It provides mathematical and safety primitives (monadic abstractions, three-valued logic, ethics framework, symbolic reasoning) that all higher-level agent components build on.

- **Language**: C# 14.0 targeting .NET 10.0
- **License**: MIT (Copyright 2025 PMeeske)
- **Part of**: [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) multi-repo architecture

## Build & Test Commands

```bash
# Build everything
dotnet build

# Build a specific project
dotnet build src/Ouroboros.Core/Ouroboros.Core.csproj

# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/Ouroboros.Core.Tests/

# Run property-based tests only (monad law verification)
dotnet test --filter "Category=Property"

# Run BDD ethics tests
dotnet test tests/Ouroboros.Foundation.BDD/

# Run mutation testing (requires dotnet-stryker tool)
dotnet stryker
```

There is no separate lint command. Static analysis is handled by SonarAnalyzer.CSharp at build time.

## Repository Structure

```
ouroboros-foundation/
├── src/
│   ├── Ouroboros.Abstractions/    # Shared interfaces, monads, agent & provider abstractions
│   ├── Ouroboros.Core/            # Core: Laws of Form, ethics, MeTTa, causal reasoning, cognition
│   ├── Ouroboros.Domain/          # Domain types: voice, vectors, RL, governance, benchmarks
│   ├── Ouroboros.Genetic/         # Genetic algorithm module (two API generations)
│   ├── Ouroboros.Roslynator/      # Roslyn-based code analysis and fix pipelines
│   └── Ouroboros.Tools/           # Tool registry, MeTTa engines, GitHub integration
├── tests/
│   ├── Ouroboros.Abstractions.Tests/
│   ├── Ouroboros.Core.Tests/
│   ├── Ouroboros.Domain.Tests/
│   ├── Ouroboros.Genetic.Tests/
│   ├── Ouroboros.Tools.Tests/
│   └── Ouroboros.Foundation.BDD/  # BDD ethics tests (Reqnroll + MeTTa)
├── features/                      # Gherkin feature files for ethics
├── docs/                          # Architecture and usage guides
├── .build/                        # Git submodule: ouroboros-build (shared build config)
├── .github/workflows/             # CI/CD pipelines
└── Directory.Build.props          # Build configuration with fallback defaults
```

## Project Dependency Graph

```
Ouroboros.Abstractions  (no internal dependencies)
       ↓
Ouroboros.Core          (depends on Abstractions)
       ↓
Ouroboros.Domain        (depends on Core)
Ouroboros.Genetic       (depends on Core)
Ouroboros.Roslynator    (depends on Core)
Ouroboros.Tools         (depends on Core)
```

## Coding Conventions

### General Style
- **Namespacing**: `Ouroboros.*` hierarchical namespaces matching folder structure
- **Nullable**: Enabled globally — use nullable reference types, avoid `null` where possible
- **Implicit usings**: Enabled via `GlobalUsings.cs` in each project
- **Naming**: PascalCase for types/methods/properties, camelCase for parameters/locals
- **XML docs**: Use `<summary>`, `<typeparam>`, `<param>`, `<returns>` tags on public APIs
- **Suppressed warnings**: CS1591 (missing XML comments), SA0001

### Functional Programming Principles
This codebase follows functional programming principles throughout:
- **Immutability**: Prefer immutable data structures and records
- **Monadic composition**: Use `Option<T>`, `Result<TValue, TError>`, and Kleisli arrows (`Step<TIn, TOut>`) instead of exceptions for control flow
- **No exceptions for control flow**: Use `Result<T>` for expected errors
- **Pipeline composition**: Chain operations using Kleisli arrow composition

### Ethics Framework Rules
The ethics framework has strict guarantees that must be preserved:
- `ImmutableEthicsFramework` is **sealed** — never subclass, disable, or bypass it
- All agent actions must execute through `EthicsEnforcementWrapper<TAction, TResult>`
- Audit logging via `IEthicsAuditLog` is mandatory and must never be suppressed
- Four clearance levels: Permitted, PermittedWithConcerns, RequiresHumanApproval, Denied

### Laws of Form
Three-valued logic uses Mark/Void/Imaginary states (not true/false/null):
- Verify algebraic laws for any new logic operations
- Use `AuditableDecision<T>` for decision tracking with evidence trails

## Testing Conventions

### Frameworks
- **xUnit** (v2.9.3): Primary test framework
- **FsCheck** (v3.3.2): Property-based testing for monad laws and algebraic properties
- **Reqnroll** (v3.3.3): BDD tests with Gherkin feature files (ethics focus)
- **FluentAssertions** (v8.8.0): Assertion library
- **Moq** (v4.20.72): Mocking
- **coverlet.collector**: Code coverage

### Test Organization
- Property-based tests use trait `[Trait("Category", "Property")]` with 1000 iterations
- BDD feature files live in `features/ethics/` with step definitions in `tests/Ouroboros.Foundation.BDD/`
- Each `src/` project has a corresponding `tests/` project

### Coverage
- CI coverage threshold: **60%** for unit tests
- BDD tests have a 0% coverage threshold (tested for pass/fail only)
- Current overall coverage: ~40.9%

## CI/CD

Three GitHub Actions workflows in `.github/workflows/`:

1. **ci.yml**: Build → Test → BDD → Coverage Badge
   - Triggers on push to main/develop, PRs, and `dependency-updated` dispatch
   - Uses reusable workflows from `PMeeske/ouroboros-build`
2. **mutation.yml**: Weekly Stryker mutation testing (Sunday 3am UTC)
3. **notify-downstream.yml**: Triggers downstream repos (ouroboros-engine, ouroboros-app) after successful CI

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| System.Reactive | 6.1.0 | Reactive extensions for async Kleisli composition |
| Microsoft.CodeAnalysis.CSharp | 5.0.0 | Roslyn compiler APIs |
| Octokit | 14.0.0 | GitHub API integration |
| Qdrant.Client | 1.17.0 | Vector database |
| Serilog | 4.3.1 | Structured logging |
| LangChain | 0.17.0 | LLM integration |
| OllamaSharp | 5.4.18 | Local LLM integration |

## Git Conventions

- **Branches**: `main`, `develop`, feature branches
- **Commit messages**: Conventional commits (`feat:`, `fix:`, `chore:`)
- **Build submodule**: `.build/` points to `PMeeske/ouroboros-build` (develop branch)

## Common Tasks

### Adding a new monadic type
1. Implement in `Ouroboros.Core` following `Option<T>`/`Result<T>` patterns
2. Add property-based tests in `tests/Ouroboros.Core.Tests/PropertyBased/` verifying monad laws (left identity, right identity, associativity)
3. Use `[Trait("Category", "Property")]` and 1000 FsCheck iterations

### Adding a new tool
1. Implement in `Ouroboros.Tools` using `ToolBuilder` or `AdvancedToolBuilder`
2. Register in `ToolRegistry`
3. Add tests in `tests/Ouroboros.Tools.Tests/`

### Adding ethical principles
1. Add to `ImmutableEthicsFramework` (sealed, cannot be subclassed)
2. Add BDD scenarios in `features/ethics/` with Gherkin syntax
3. Implement step definitions in `tests/Ouroboros.Foundation.BDD/`
4. Ensure audit logging is maintained

### Adding MeTTa reasoning rules
1. Add atoms/expressions in `Ouroboros.Core/Hyperon/`
2. Register grounded operations via `GroundedRegistry`
3. Test symbolic evaluation in `tests/Ouroboros.Core.Tests/`
