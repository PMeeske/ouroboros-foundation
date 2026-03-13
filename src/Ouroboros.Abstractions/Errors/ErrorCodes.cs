// <copyright file="ErrorCodes.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Errors;

/// <summary>
/// Well-known error codes for the Ouroboros pipeline system.
/// Each constant follows the pattern <c>CATEGORY_NNN</c> where the category
/// identifies the subsystem and the number uniquely identifies the error within that category.
/// </summary>
public static class ErrorCodes
{
    /// <summary>An action was denied by the ethics framework.</summary>
    public const string EthicsViolation = "ETHICS_001";

    /// <summary>A governance policy denied the operation.</summary>
    public const string GovernanceDenied = "GOV_001";

    /// <summary>Goal decomposition could not produce valid sub-goals.</summary>
    public const string GoalDecompositionFailed = "GOAL_001";

    /// <summary>The LLM response could not be parsed into the expected format.</summary>
    public const string LlmParseFailure = "LLM_001";

    /// <summary>A consistency check across pipeline stages failed.</summary>
    public const string ConsistencyCheckFailed = "CONSISTENCY_001";

    /// <summary>A reasoning step produced an invalid or incomplete result.</summary>
    public const string ReasoningFailed = "REASONING_001";

    /// <summary>A security boundary was violated.</summary>
    public const string SecurityViolation = "SEC_001";

    /// <summary>Input validation failed at a system boundary.</summary>
    public const string ValidationFailed = "VAL_001";

    /// <summary>An operation exceeded its allotted time.</summary>
    public const string TimeoutExpired = "TIMEOUT_001";

    /// <summary>A required resource (model, memory entry, tool, etc.) was not found.</summary>
    public const string ResourceNotFound = "NOTFOUND_001";

    /// <summary>A configuration value is missing or invalid.</summary>
    public const string ConfigurationError = "CONFIG_001";

    /// <summary>Serialization or deserialization of a value failed.</summary>
    public const string SerializationFailed = "SERIAL_001";

    /// <summary>A tool execution failed.</summary>
    public const string ToolExecutionFailed = "TOOL_001";

    /// <summary>A tool was not authorized by the ethics framework.</summary>
    public const string ToolNotAuthorized = "TOOL_002";

    /// <summary>An I/O operation failed.</summary>
    public const string IoOperationFailed = "IO_001";

    /// <summary>A network operation failed.</summary>
    public const string NetworkOperationFailed = "NET_001";

    /// <summary>A memory operation failed.</summary>
    public const string MemoryOperationFailed = "MEM_001";

    /// <summary>A parse operation failed.</summary>
    public const string ParseOperationFailed = "PARSE_001";

    /// <summary>An embedding operation failed.</summary>
    public const string EmbeddingOperationFailed = "EMBED_001";

    /// <summary>A pipeline step failed.</summary>
    public const string PipelineStepFailed = "PIPE_001";

    /// <summary>A Qdrant vector database operation failed.</summary>
    public const string QdrantOperationFailed = "QDRANT_001";
}
