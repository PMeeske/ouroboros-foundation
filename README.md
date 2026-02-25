# Ouroboros Foundation

[![CI](https://github.com/PMeeske/ouroboros-foundation/actions/workflows/ci.yml/badge.svg)](https://github.com/PMeeske/ouroboros-foundation/actions/workflows/ci.yml)
[![Mutation Testing](https://github.com/PMeeske/ouroboros-foundation/actions/workflows/mutation.yml/badge.svg)](https://github.com/PMeeske/ouroboros-foundation/actions/workflows/mutation.yml)

**Core functional programming abstractions, domain primitives, and AI safety infrastructure for the Ouroboros agent system.**

## Overview

`ouroboros-foundation` is the foundational layer of the [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) ecosystem. It provides the mathematical and safety primitives that all higher-level agent components build on:

- **Monadic Abstractions**: `Option<T>`, `Result<TValue, TError>`, `Step<TIn, TOut>` (Kleisli arrows) for composable, type-safe pipelines
- **Laws of Form**: Three-valued logic (Mark/Void/Imaginary) based on Spencer-Brown's calculus, with tool safety gating, human-in-the-loop approval, confidence routing, and contradiction detection
- **Ethics Framework**: Sealed, immutable ethical evaluation system that gates all agent actions with 10 core principles
- **Hyperon/MeTTa Symbolic Reasoning**: Full MeTTa interpreter with S-expression parsing, atom spaces, unification, and grounded C# integration
- **Causal Reasoning**: Pearl's causal inference framework with graph construction, do-calculus, interventions, and MeTTa integration
- **Cognitive Physics Engine**: Multi-branch reasoning with superposition, chaos injection, perspective shifting, and evolutionary adaptation
- **Genetic Algorithm Module**: Functional evolutionary optimization with two API generations and Kleisli pipeline integration
- **Embodied Interaction**: Multi-modal perception (audio, visual, text) with sensor fusion, affordance mapping, and speech synthesis/recognition
- **Domain Types**: Shared domain primitives including vector compression, reinforcement learning, multi-agent coordination, and governance
- **Tool System**: Tool registry, builder patterns, GitHub integration, MeTTa reasoning tools, and Roslyn code analysis
- **Agent Abstractions**: Orchestrator interfaces, MetaAI subsystems (affect, homeostasis, self-improvement, world modeling, theory of mind), and provider abstractions

This repository is part of a multi-repo architecture where the Foundation layer provides shared abstractions consumed by higher-level components in the main [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) repository.

## Repository Structure

```
ouroboros-foundation/
├── src/
│   ├── Ouroboros.Abstractions/    # Shared interfaces, monads, agent & provider abstractions
│   ├── Ouroboros.Core/            # Core logic: Laws of Form, ethics, Hyperon/MeTTa,
│   │                              #   causal reasoning, cognitive physics, memory, learning
│   ├── Ouroboros.Domain/          # Domain types: voice, ingestion, vectors, RL, governance
│   ├── Ouroboros.Genetic/         # Genetic algorithm module (two API generations)
│   ├── Ouroboros.Roslynator/      # Roslyn-based code analysis and fix pipelines
│   └── Ouroboros.Tools/           # Tool registry, MeTTa engines, GitHub tools, utilities
├── tests/
│   ├── Ouroboros.Abstractions.Tests/  # Contract tests for abstraction interfaces
│   ├── Ouroboros.Core.Tests/          # Unit, property-based, and integration tests
│   ├── Ouroboros.Domain.Tests/        # Domain type tests
│   ├── Ouroboros.Genetic.Tests/       # Genetic algorithm tests (49 tests)
│   ├── Ouroboros.Tools.Tests/         # Tool tests
│   └── Ouroboros.Foundation.BDD/      # BDD-style ethics tests (SpecFlow + MeTTa)
├── features/
│   └── ethics/                        # Gherkin feature files and MeTTa logic for ethics
├── docs/                              # Architecture and usage guides
└── Directory.Build.props              # Build configuration
```

## Key Components

### Ouroboros.Abstractions

Shared interfaces and foundational types used across all layers:

- **Monads**: `Option<T>` and `Result<TValue, TError>` with full monadic operations
- **Agent Framework**: `IOrchestrator<TIn, TOut>`, `IModelOrchestrator`, agent capabilities
- **MetaAI Subsystems**: Affect/homeostasis, self-improvement, world modeling, theory of mind, temporal reasoning, uncertainty routing, transfer learning
- **Provider Abstractions**: Docker, Kubernetes, DuckDuckGo, Firecrawl MCP clients
- **Speech/Audio/Vision**: STT, TTS, and vision model interfaces
- **Network/Persistence**: Graph persistence, monad nodes, WAL entries
- **Load Balancing**: Provider selection strategies

### Ouroboros.Core

The heart of the Foundation layer, containing multiple subsystems:

#### Monadic Abstractions
- **`Option<T>`**: Explicit handling of optional values (no null references)
- **`Result<TValue, TError>`**: Railway-oriented error handling
- **`Step<TIn, TOut>`**: Composable async pipeline steps defined as Kleisli arrows (`delegate Task<TB> Step<in TA, TB>(TA input)`)
- **Kleisli Composition**: Full category-theoretic arrow composition with async and reactive variants
- **Property-based tests** verifying monad laws for all abstractions via FsCheck

#### Laws of Form
Three-valued logic system based on G. Spencer-Brown's *Laws of Form* (1969):
- **`Form`**: Mark, Void, Imaginary states with algebraic operations (AND, OR, NOT)
- **`AuditableDecision<T>`**: Decision tracking with evidence trails for regulated domains
- **`DecisionPipeline`**: Composable decision criteria chains
- **`TriState` & `HierarchicalConfig`**: Three-valued configuration with inheritance hierarchies
- **`FormStateMachine<TState>`**: State machines with explicit indeterminate states
- **`DistinctionArrow`**: Category-theoretic distinction operations (gate, branch, allMarked, anyMarked)
- **`Imagination`**: Self-reference modeling, oscillators, waves, and dream states

See: [src/Ouroboros.Core/Core/LawsOfForm/README.md](src/Ouroboros.Core/Core/LawsOfForm/README.md) | [docs/LAWS_OF_FORM.md](docs/LAWS_OF_FORM.md)

#### Tool Safety Pipeline (Laws of Form Integration)
AI tool execution safety built on three-valued certainty logic:
- **`SafeToolExecutor`**: Multi-criterion safety gating (authorization, rate limits, content safety)
- **`ToolApprovalQueue`**: Human-in-the-loop approval workflows for uncertain decisions
- **`ConfidenceGatedPipeline`**: Confidence-based LLM response routing
- **`ContradictionDetector`**: Hallucination detection via claim analysis

#### Ethics Framework
Immutable ethical evaluation system ensuring AI safety:
- **10 core ethical principles** (Do No Harm, Autonomy, Honesty, Privacy, Fairness, Transparency, Human Oversight, Prevent Misuse, Safe Self-Improvement, Corrigibility)
- **`ImmutableEthicsFramework`**: Sealed implementation - cannot be subclassed, disabled, or bypassed
- **`EthicsEnforcementWrapper<TAction, TResult>`**: Generic wrapper ensuring all actions execute with clearance
- **`EthicalClearance`**: Four levels (Permitted, PermittedWithConcerns, RequiresHumanApproval, Denied)
- **Mandatory audit logging** via `IEthicsAuditLog`
- **`EthicalHomeostasisEngine`**: Homeostasis-based ethical tension management
- **BDD feature tests** covering multiple ethical traditions (Ahimsa, Ubuntu, Kantian, Levinas, Nagarjuna)

See: [src/Ouroboros.Core/Ethics/README.md](src/Ouroboros.Core/Ethics/README.md)

#### Hyperon/MeTTa Symbolic Reasoning
Full implementation of the MeTTa specification for symbolic reasoning:
- **`Atom` & `AtomSpace`**: Core symbolic knowledge structures
- **`Expression` & S-expression parsing**: Symbolic logic parsing
- **`Interpreter`**: MeTTa execution engine with grounded operations
- **`Unifier` & `Substitution`**: Logic programming primitives
- **`GroundedRegistry`**: Bridge between MeTTa symbolic layer and C# code
- **`FormMeTTaBridge`**: Integration between Laws of Form distinctions and Hyperon atoms

#### Causal Reasoning
Pearl's causal inference framework:
- **`CausalReasoningEngine`**: Causal graph construction and inference
- **Causal discovery algorithms**: Automated causal structure learning
- **Interventions & observations**: Do-calculus and counterfactual reasoning
- **`CausalMeTTaIntegration`**: Symbolic causal reasoning via MeTTa

#### Cognitive Physics Engine
Multi-branch reasoning with ethics gates:
- **`SuperpositionEngine`**: Quantum-inspired state superposition
- **`ChaosInjector`**: Controlled variation injection for exploration
- **`ZeroShiftOperator`**: Perspective shifting transformations
- **`EvolutionaryAdapter`**: Genetic algorithm-based adaptation
- **`SemanticDistance`**: Semantic metric calculations

#### Memory & Conversation
- **`ConversationMemory`**: Turn-based conversation state
- **`ConversationBuilder`**: Fluent memory construction
- **`MemoryArrows`**: Kleisli composition for memory operations

#### Embodied Interaction
- **`EmbodimentAggregate`**: Multi-sensor integration
- **`AudioSensor` / `VisualSensor`**: Sensor abstractions
- **`UnifiedPerception`**: Fusion of multi-modal perception
- **Affordance mapping**: Environment action capabilities
- **`VoiceActuator`**: Voice output control

#### Additional Subsystems
- **Distinction Learning**: Learning distinctions from observations with weight persistence
- **Program Synthesis**: `AbstractSyntaxTree`, `ProgramSynthesisEngine`, MeTTa DSL bridge
- **Resilience**: `CircuitBreaker`, health checks, metrics, distributed tracing
- **Security**: `InputValidator`, authentication/authorization providers
- **Vector Operations**: Thought vector extensions, vector convolution

### Ouroboros.Genetic

Functional programming-based genetic algorithm module with two parallel API generations:

- **Classic API** (`Abstractions/` + `Core/`): `IChromosome<TGene>`, `IFitnessFunction<TGene>`, `IGeneticAlgorithm<TGene>`
- **Evolution API** (`Genetic/Core/`): `IEvolutionEngine<TChromosome>`, `IEvolutionFitnessFunction<TChromosome>`, `EvolutionEngine<TChromosome>`
- **Pipeline Extensions**: `Evolve()`, `EvolvePopulation()`, `EvolveWith()` for Kleisli arrow integration
- **Operators**: Roulette wheel selection, uniform crossover, mutation, elitism preservation
- **Monadic error handling** via `Result<T>`, immutable data structures, optional seeding for reproducibility

See: [src/Ouroboros.Genetic/README.md](src/Ouroboros.Genetic/README.md) | [docs/GENETIC_ALGORITHM_MODULE.md](docs/GENETIC_ALGORITHM_MODULE.md)

### Ouroboros.Domain

Shared domain types and primitives:
- **Voice/Audio**: Control events, response types, barge-in, agent presence, thinking phases
- **Ingestion**: Batch processing types
- **Vector Compression**: DCT, Fourier, and quantization-based compression
- **Learning**: Adapter engines, distinction storage, embodied and reinforcement learning
- **Multi-Agent**: Coordination primitives
- **Governance**: Authorization and policy types
- **Autonomous Agents**: Neuron-based architecture primitives
- **Benchmarks**: Performance measurement types (ARC-AGI-2, MMLU, cognitive dimensions, continual learning)

### Ouroboros.Roslynator

Roslyn-based code analysis and transformation:
- **`FixChain` & `FixChainArrows`**: Composable code fix pipelines
- **`FixState` & `Future<T>`**: Pipeline state management
- **Standard Steps**: Resolve, format, throttle operations
- **`UniversalCodeFixProvider`**: Roslyn code fix integration

### Ouroboros.Tools

Tool registry, MeTTa engines, and utility helpers:
- **Tool System**: `ToolRegistry`, `ToolBuilder`, `AdvancedToolBuilder`, schema generation
- **MeTTa Engines**: `HyperonMeTTaEngine` (native C#), `HttpMeTTaEngine`, `SubprocessMeTTaEngine`, `AdvancedMeTTaEngine` with 50+ supporting files for reasoning, proof strategies, pattern matching, and form-based inference
- **GitHub Integration**: Issues, pull requests, comments, labels, search via Octokit
- **Code Tools**: `RoslynCodeTool`, `SafeCalculatorTool`, `RetrievalTool`, `MathTool`
- **DSL Support**: `DslAssistant` with suggestion engine

## Building and Testing

### Prerequisites

- **.NET 10.0** SDK or later
- This repository depends on external build configuration from the main Ouroboros-v2 monorepo

### Independent Build

The build system supports multiple layouts. If the external `ouroboros-build` submodule or parent monorepo is not present, the `Directory.Build.props` falls back to default settings (net10.0, C# 14, nullable enabled).

```bash
# Build all projects
dotnet build

# Build a specific project
dotnet build src/Ouroboros.Core/Ouroboros.Core.csproj
```

For full build support with shared configuration, clone within the [Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2) monorepo structure.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/Ouroboros.Core.Tests/

# Run property-based tests only (monad law verification via FsCheck)
dotnet test --filter "Category=Property"

# Run Laws of Form tests
dotnet test --filter "FullyQualifiedName~LawsOfForm"

# Run Ethics Framework tests
dotnet test --filter "FullyQualifiedName~Ethics"

# Run Genetic Algorithm tests
dotnet test --filter "FullyQualifiedName~Genetic"

# Run Benchmark Suite tests
dotnet test --filter "FullyQualifiedName~BenchmarkSuiteTests"
```

The test suite includes:
- **Unit tests** with xUnit
- **Property-based tests** with FsCheck (verifying monad laws, algebraic properties)
- **Contract tests** for abstraction interfaces
- **BDD tests** with SpecFlow (ethical scenario testing across multiple philosophical traditions)
- **Integration tests** for complex cross-cutting scenarios

See: [tests/Ouroboros.Core.Tests/PropertyBased/README.md](tests/Ouroboros.Core.Tests/PropertyBased/README.md)

## Dependencies

### Internal Dependencies (Ouroboros Ecosystem)
- **[Ouroboros-v2](https://github.com/PMeeske/Ouroboros-v2)**: Main repository consuming Foundation abstractions
- **[ouroboros-build](https://github.com/PMeeske/ouroboros-build)**: Shared build configuration (git submodule)

### External Dependencies
- **LangChain** (0.17.0): LLM integration primitives (partial; two integration files currently excluded from compilation pending API alignment)
- **System.Reactive** (6.1.0): Reactive extensions for async Kleisli composition
- **Microsoft.Extensions.\*** (10.0+): Configuration, hosting, dependency injection
- **Microsoft.CodeAnalysis.CSharp** (5.0.0): Roslyn compiler APIs (used by Roslynator and Tools)
- **Octokit** (14.0.0): GitHub API integration (used by Tools)
- **Serilog** (4.3.1): Structured logging
- **SonarAnalyzer.CSharp**: Static code quality analysis
- **FsCheck**: Property-based testing
- **xUnit**: Unit testing framework
- **SpecFlow**: BDD testing

## Documentation

- **[LAWS_OF_FORM.md](docs/LAWS_OF_FORM.md)**: Three-valued logic implementation and philosophical foundations
- **[GENETIC_ALGORITHM_MODULE.md](docs/GENETIC_ALGORITHM_MODULE.md)**: Genetic algorithm design, usage, and optimization
- **[ARROW_COMPOSITION_EXAMPLES.md](docs/ARROW_COMPOSITION_EXAMPLES.md)**: Kleisli arrow composition patterns (includes patterns used in Ouroboros-v2)
- **[ARROW_PARAMETERIZATION.md](docs/ARROW_PARAMETERIZATION.md)**: Arrow parameterization as an alternative to constructor DI
- **[ARROW_PARAMETERIZATION_SUMMARY.md](docs/ARROW_PARAMETERIZATION_SUMMARY.md)**: Summary of arrow parameterization transformations
- **[BENCHMARK_SUITE.md](docs/BENCHMARK_SUITE.md)**: Benchmark suite domain types and evaluation framework
- **[RECURSIVE_CHUNKING.md](docs/RECURSIVE_CHUNKING.md)**: Large context processing via recursive chunking

### Inline Documentation
- **[Ethics Framework README](src/Ouroboros.Core/Ethics/README.md)**: Ethics framework deep dive
- **[Laws of Form README](src/Ouroboros.Core/Core/LawsOfForm/README.md)**: Laws of Form integration layer
- **[Genetic Module README](src/Ouroboros.Genetic/README.md)**: Genetic algorithm module guide

## Contributing

When contributing to ouroboros-foundation:

1. **Maintain functional programming principles**: Immutability, monadic composition, no exceptions for control flow
2. **Add tests**: Unit tests + property-based tests for new monadic types, contract tests for new abstractions
3. **Preserve Laws of Form properties**: Verify algebraic laws for logic operations
4. **Respect ethics framework guarantees**: Sealed implementations, non-bypassable enforcement
5. **Follow naming conventions**: Match existing patterns (`Ouroboros.*` namespaces)

## License

Copyright (c) 2025 PMeeske

MIT License - See [LICENSE](LICENSE) file for details.

---

**Questions or Issues?** Please open an issue in the [Ouroboros-v2 main repository](https://github.com/PMeeske/Ouroboros-v2/issues) or this repository as appropriate.
