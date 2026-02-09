# Ouroboros Foundation

**Core functional programming abstractions and domain primitives for the Ouroboros AI agent system.**

## Overview

`ouroboros-foundation` is the foundational layer of the [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) ecosystem, providing: 

- **Core Monadic Abstractions**: `Option<T>`, `Result<T>`, `Step<TIn, TOut>` for composable, type-safe error handling
- **Laws of Form Integration**: Three-valued logic (Mark/Void/Imaginary) for AI safety and uncertainty modeling
- **Ethics Framework**: Immutable ethical evaluation system that gates all agent actions
- **Genetic Algorithm Module**: Functional programming-based evolutionary optimization
- **Domain Types**: Shared domain primitives used across the Ouroboros platform
- **Roslyn Tools**: Code analysis and transformation utilities

This repository is part of a multi-repo architecture where the Foundation layer provides shared abstractions consumed by higher-level components in the main [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) repository.

## Repository Structure

```
ouroboros-foundation/
├── src/
│   ├── Ouroboros.Core/          # Core monadic abstractions and Laws of Form
│   ├── Ouroboros.Domain/         # Shared domain types and primitives
│   ├── Ouroboros.Genetic/        # Genetic algorithm module
│   ├── Ouroboros.Roslynator/     # Roslyn-based code analysis tools
│   └── Ouroboros.Tools/          # Utility tools and helpers
├── tests/
│   ├── Ouroboros.Core.Tests/     # Unit and property-based tests for Core
│   ├── Ouroboros.Domain.Tests/   # Domain type tests
│   ├── Ouroboros.Genetic.Tests/  # Genetic algorithm tests
│   ├── Ouroboros.Tools.Tests/    # Tools tests
│   └── Ouroboros.Foundation.BDD/ # BDD-style integration tests
└── docs/
    ├── ARROW_COMPOSITION_EXAMPLES.md      # Kleisli arrow composition patterns
    ├── ARROW_PARAMETERIZATION.md          # Parameterization strategies
    ├── BENCHMARK_SUITE.md                 # Performance benchmarking guide
    ├── GENETIC_ALGORITHM_MODULE.md        # Genetic algorithm documentation
    ├── LAWS_OF_FORM.md                    # Laws of Form implementation
    └── RECURSIVE_CHUNKING.md              # Large context processing guide
```

## Key Components

### Ouroboros.Core

The heart of the Foundation layer, providing:

#### Monadic Abstractions
- **`Option<T>`**: Explicit handling of optional values (no null references)
- **`Result<T>`**: Railway-oriented error handling
- **`Step<TIn, TOut>`**: Composable async pipeline steps (Kleisli arrows)
- **Property-based tests** verifying monad laws for all abstractions

#### Laws of Form
Three-valued logic system based on G. Spencer-Brown's Laws of Form:
- **`Form`**: Mark (⊤), Void (⊥), Imaginary (i) states
- **`AuditableDecision<T>`**: Decision tracking with audit trails
- **`DecisionPipeline`**: Composable decision criteria
- **`TriState`**: Three-valued configuration for hierarchies
- **`FormStateMachine<TState>`**: State machines with indeterminate states

See: [src/Ouroboros.Core/Core/LawsOfForm/README.md](src/Ouroboros.Core/Core/LawsOfForm/README.md)

#### Ethics Framework
Immutable ethical evaluation system ensuring AI safety:
- 10 core ethical principles (Do No Harm, Autonomy, Privacy, etc.)
- Mandatory evaluation for all agent actions
- Non-bypassable enforcement via sealed implementations
- Complete audit trail for compliance

See: [src/Ouroboros.Core/Ethics/README.md](src/Ouroboros.Core/Ethics/README.md)

### Ouroboros.Genetic

Functional programming-based genetic algorithm module with:
- Monadic error handling via `Result<T>`
- Immutable data structures
- Composable evolution pipelines
- Integration with `Step<TIn, TOut>` architecture

See: [src/Ouroboros.Genetic/README.md](src/Ouroboros.Genetic/README.md)

### Ouroboros.Domain

Shared domain types and primitives used across the Ouroboros ecosystem.

### Ouroboros.Tools

Utility tools and helper functions for common operations.

### Ouroboros.Roslynator

Code analysis and transformation tools built on Roslyn.

## Building and Testing

### Prerequisites

- **.NET 10.0** SDK or later
- This repository depends on external build configuration from the main Ouroboros-v2 monorepo

### Independent Build (Limited)

⚠️ **Note**: This repository references build configuration from `../.build/build/Directory.Build.props` which is part of the parent [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) monorepo. For full build support, clone the entire Ouroboros-v2 repository structure.

To build individual projects:

```bash
# Build a specific project
dotnet build src/Ouroboros.Core/Ouroboros.Core.csproj

# Build all projects (may require full monorepo)
dotnet build
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/Ouroboros.Core.Tests/

# Run property-based tests only
dotnet test --filter "Category=Property"

# Run Laws of Form tests
dotnet test --filter "FullyQualifiedName~LawsOfForm"

# Run Ethics Framework tests
dotnet test --filter "FullyQualifiedName~Ethics"

# Run Genetic Algorithm tests
dotnet test --filter "FullyQualifiedName~Genetic"
```

The test suite includes:
- **Unit tests** with xUnit
- **Property-based tests** with FsCheck (verifying monad laws)
- **BDD tests** with SpecFlow
- **Integration tests** for complex scenarios

See: [tests/Ouroboros.Core.Tests/PropertyBased/README.md](tests/Ouroboros.Core.Tests/PropertyBased/README.md)

## Dependencies

### Internal Dependencies (Ouroboros Ecosystem)
- **Ouroboros-v2 (main repo)**: https://github.com/PMeeske/Ouroboros-v2
  - Consumes Foundation abstractions in higher-level components
  - Provides build configuration and shared infrastructure

### External Dependencies
- **LangChain** (0.17.0): LLM integration primitives
- **System.Reactive** (6.1.0): Reactive extensions
- **Microsoft.Extensions.*** (8.0+): Configuration, hosting, DI
- **Serilog** (4.3.0): Structured logging
- **FsCheck** (latest): Property-based testing
- **xUnit** (latest): Unit testing framework
- **SpecFlow** (latest): BDD testing

## Related Repositories

- **[Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2)**: Main repository with AI agent engine, pipeline orchestration, and application layer
- **[ouroboros-build](https://github.com/PMeeske/ouroboros-build)** *(if exists)*: Shared build configuration and tooling

## Documentation

- **[LAWS_OF_FORM.md](docs/LAWS_OF_FORM.md)**: Comprehensive guide to three-valued logic implementation
- **[GENETIC_ALGORITHM_MODULE.md](docs/GENETIC_ALGORITHM_MODULE.md)**: Genetic algorithm design and usage
- **[ARROW_COMPOSITION_EXAMPLES.md](docs/ARROW_COMPOSITION_EXAMPLES.md)**: Kleisli arrow composition patterns
- **[ARROW_PARAMETERIZATION.md](docs/ARROW_PARAMETERIZATION.md)**: Parameter injection strategies
- **[BENCHMARK_SUITE.md](docs/BENCHMARK_SUITE.md)**: Performance testing guide
- **[RECURSIVE_CHUNKING.md](docs/RECURSIVE_CHUNKING.md)**: Processing large contexts

## Contributing

When contributing to ouroboros-foundation:

1. **Maintain functional programming principles**: Immutability, monadic composition, no exceptions
2. **Add tests**: Unit tests + property-based tests for new monadic types
3. **Preserve Laws of Form properties**: Verify algebraic laws for logic operations
4. **Update documentation**: Keep READMEs and inline docs current
5. **Follow naming conventions**: Match existing patterns

## License

Copyright (c) 2025 PMeeske

MIT License - See [LICENSE](LICENSE) file for details.

## GitHub Topics

**Suggested topics for this repository:**
- `functional-programming`
- `monads`
- `category-theory`
- `csharp`
- `dotnet`
- `ai-safety`
- `laws-of-form`
- `genetic-algorithms`
- `ethics-framework`
- `ouroboros`

*Repository description suggestion*: "Core functional programming abstractions, Laws of Form three-valued logic, and ethics framework for the Ouroboros AI agent system"

---

**Questions or Issues?** Please open an issue in the [Ouroboros-v2 main repository](https://github.com/PMeeske/Ouroboros-v2/issues) or this repository as appropriate.
