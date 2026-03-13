// <copyright file="LangChainIntegration.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using LangChain.Abstractions.Schema;
using LangChain.Chains.HelperChains;
using LangChain.Chains.LLM;
using LangChain.Prompts;
using LangChain.Prompts.Base;
using LangChain.Providers;
using LangChain.Schema;

namespace Ouroboros.Core.LangChain;
/// <summary>
/// Integration layer that bridges LangChain chains with the monadic pipeline system.
/// </summary>
public static class LangChainIntegration
{
    /// <summary>
    /// Converts a LangChain BaseStackableChain into a monadic KleisliResult.
    /// </summary>
    /// <returns></returns>
    public static KleisliResult<Dictionary<string, object>, Dictionary<string, object>, string> ToMonadicKleisli(
        this BaseStackableChain chain)
    {
        return async input =>
        {
            try
            {
                ChainValues chainValues = new ChainValues(input);
                IChainValues result = await chain.CallAsync(chainValues).ConfigureAwait(false);
                return Result<Dictionary<string, object>, string>.Success(result.Value);
            }
            catch (OperationCanceledException) { throw; }
            catch (HttpRequestException ex)
            {
                return Result<Dictionary<string, object>, string>.Failure($"LangChain execution failed: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return Result<Dictionary<string, object>, string>.Failure($"LangChain execution failed: {ex.Message}");
            }
        };
    }

    /// <summary>
    /// Converts a LangChain LlmChain into a monadic KleisliResult.
    /// </summary>
    /// <returns></returns>
    public static KleisliResult<Dictionary<string, object>, Dictionary<string, object>, string> ToMonadicKleisli(
        this LlmChain chain)
    {
        return async input =>
        {
            try
            {
                ChainValues chainValues = new ChainValues(input);
                IChainValues result = await chain.CallAsync(chainValues).ConfigureAwait(false);
                return Result<Dictionary<string, object>, string>.Success(result.Value);
            }
            catch (OperationCanceledException) { throw; }
            catch (HttpRequestException ex)
            {
                return Result<Dictionary<string, object>, string>.Failure($"LangChain LLM execution failed: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return Result<Dictionary<string, object>, string>.Failure($"LangChain LLM execution failed: {ex.Message}");
            }
        };
    }

    /// <summary>
    /// Converts a LangChain BaseStackableChain into a plain Step (throws on error).
    /// </summary>
    /// <returns></returns>
    public static Step<Dictionary<string, object>, Dictionary<string, object>> ToStep(
        this BaseStackableChain chain)
    {
        return async input =>
        {
            ChainValues chainValues = new ChainValues(input);
            IChainValues result = await chain.CallAsync(chainValues).ConfigureAwait(false);
            return result.Value;
        };
    }

    /// <summary>
    /// Converts a LangChain LlmChain into a plain Step (throws on error).
    /// </summary>
    /// <returns></returns>
    public static Step<Dictionary<string, object>, Dictionary<string, object>> ToStep(
        this LlmChain chain)
    {
        return async input =>
        {
            ChainValues chainValues = new ChainValues(input);
            IChainValues result = await chain.CallAsync(chainValues).ConfigureAwait(false);
            return result.Value;
        };
    }

    /// <summary>
    /// Creates a LangChain SetChain wrapped as a monadic KleisliResult.
    /// </summary>
    /// <returns></returns>
    public static KleisliResult<Dictionary<string, object>, Dictionary<string, object>, string> CreateSetKleisli(
        object value, string outputKey = "query")
    {
        SetChain setChain = new SetChain(value, outputKey);
        return setChain.ToMonadicKleisli();
    }

    /// <summary>
    /// Creates a LangChain LLMChain wrapped as a monadic KleisliResult.
    /// </summary>
    /// <returns></returns>
    public static KleisliResult<Dictionary<string, object>, Dictionary<string, object>, string> CreateLlmKleisli(
        IChatModel llm,
        PromptTemplate prompt,
        string outputKey = "text")
    {
        LlmChain llmChain = new LlmChain(new LlmChainInput(llm, prompt)
        {
            OutputKey = outputKey,
        });

        return llmChain.ToMonadicKleisli();
    }

    /// <summary>
    /// Creates a LangChain SetChain wrapped as a Step.
    /// </summary>
    /// <returns></returns>
    public static Step<Dictionary<string, object>, Dictionary<string, object>> CreateSetStep(
        object value, string outputKey = "query")
    {
        SetChain setChain = new SetChain(value, outputKey);
        return setChain.ToStep();
    }

    /// <summary>
    /// Creates a LangChain LLMChain wrapped as a Step.
    /// </summary>
    /// <returns></returns>
    public static Step<Dictionary<string, object>, Dictionary<string, object>> CreateLlmStep(
        IChatModel llm,
        PromptTemplate prompt,
        string outputKey = "text")
    {
        LlmChain llmChain = new LlmChain(new LlmChainInput(llm, prompt)
        {
            OutputKey = outputKey,
        });

        return llmChain.ToStep();
    }
}

/// <summary>
/// Extensions for integrating LangChain chains with the conversation system.
/// </summary>
public static class LangChainConversationIntegration
{
    /// <summary>
    /// Updates the LangChain conversation pipeline to use proper LangChain chains.
    /// </summary>
    /// <returns></returns>
    public static LangChainConversationPipeline AddLangChainLlm(
        this LangChainConversationPipeline pipeline,
        IChatModel llm,
        BasePromptTemplate prompt,
        string outputKey = "text")
    {
        return pipeline.AddStep(async context =>
        {
            try
            {
                // Create LLM chain
                LlmChain llmChain = new LlmChain(new LlmChainInput(llm, prompt)
                {
                    OutputKey = outputKey,
                });

                // Convert context properties to chain input
                ChainValues chainInput = new ChainValues(context.GetProperties());

                // Execute LangChain
                IChainValues result = await llmChain.CallAsync(chainInput).ConfigureAwait(false);

                // Update context with results
                foreach (KeyValuePair<string, object> kvp in result.Value)
                {
                    context.SetProperty(kvp.Key, kvp.Value);
                }

                return context;
            }
            catch (OperationCanceledException) { throw; }
            catch (HttpRequestException ex)
            {
                // Handle errors gracefully in the monadic pipeline
                context.SetProperty("error", $"LangChain LLM execution failed: {ex.Message}");
                return context;
            }
            catch (InvalidOperationException ex)
            {
                context.SetProperty("error", $"LangChain LLM execution failed: {ex.Message}");
                return context;
            }
        });
    }

    /// <summary>
    /// Adds a LangChain set operation to the pipeline.
    /// </summary>
    /// <returns></returns>
    public static LangChainConversationPipeline AddLangChainSet(
        this LangChainConversationPipeline pipeline,
        object value,
        string outputKey = "query")
    {
        return pipeline.AddStep(async context =>
        {
            try
            {
                // Create SetChain
                SetChain setChain = new SetChain(value, outputKey);

                // Convert context to chain input
                ChainValues chainInput = new ChainValues(context.GetProperties());

                // Execute chain
                IChainValues result = await setChain.CallAsync(chainInput).ConfigureAwait(false);

                // Update context
                foreach (KeyValuePair<string, object> kvp in result.Value)
                {
                    context.SetProperty(kvp.Key, kvp.Value);
                }

                return context;
            }
            catch (OperationCanceledException) { throw; }
            catch (InvalidOperationException ex)
            {
                context.SetProperty("error", $"LangChain Set operation failed: {ex.Message}");
                return context;
            }
        });
    }

    /// <summary>
    /// Adds a generic LangChain stackable chain to the pipeline.
    /// </summary>
    /// <returns></returns>
    public static LangChainConversationPipeline AddLangChainStep(
        this LangChainConversationPipeline pipeline,
        BaseStackableChain chain)
    {
        return pipeline.AddStep(async context =>
        {
            try
            {
                // Convert context to chain input
                ChainValues chainInput = new ChainValues(context.GetProperties());

                // Execute chain
                IChainValues result = await chain.CallAsync(chainInput).ConfigureAwait(false);

                // Update context
                foreach (KeyValuePair<string, object> kvp in result.Value)
                {
                    context.SetProperty(kvp.Key, kvp.Value);
                }

                return context;
            }
            catch (OperationCanceledException) { throw; }
            catch (HttpRequestException ex)
            {
                context.SetProperty("error", $"LangChain chain execution failed: {ex.Message}");
                return context;
            }
            catch (InvalidOperationException ex)
            {
                context.SetProperty("error", $"LangChain chain execution failed: {ex.Message}");
                return context;
            }
        });
    }
}
