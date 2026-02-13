// <copyright file="IChatCompletionModel.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Core;

/// <summary>
/// Represents a chat completion model interface.
/// Provides the minimal contract for text generation used across the pipeline.
/// </summary>
public interface IChatCompletionModel
{
    /// <summary>
    /// Generates text from a prompt.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The generated text response.</returns>
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct = default);
}
