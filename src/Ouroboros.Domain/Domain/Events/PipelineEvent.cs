using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Events;

/// <summary>
/// Base class for all pipeline events that can occur during execution.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(IngestBatch), typeDiscriminator: "Ingest")]
[JsonDerivedType(typeof(ReasoningStep), typeDiscriminator: "Reasoning")]
[JsonDerivedType(typeof(EnvironmentStepEvent), typeDiscriminator: "EnvironmentStep")]
[JsonDerivedType(typeof(EpisodeEvent), typeDiscriminator: "Episode")]
[JsonDerivedType(typeof(StepExecutionEvent), typeDiscriminator: "StepExecution")]
[ExcludeFromCodeCoverage]
public abstract record PipelineEvent(Guid Id, string Kind, DateTime Timestamp);
