using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Ouroboros.Roslynator.Pipeline;

/// <summary>
/// Arrow-based factory functions for creating fix chains.
/// Replaces inheritance-based FixChain with composable arrow patterns.
/// </summary>
public static class FixChainArrows
{
    /// <summary>
    /// Creates a fix chain from a pipeline definition arrow.
    /// </summary>
    /// <param name="title">The title for the code fix.</param>
    /// <param name="equivalenceKey">The equivalence key for the code fix.</param>
    /// <param name="pipelineBuilder">Function that builds the pipeline from input Future.</param>
    /// <returns>A function that registers the fix in a CodeFixContext.</returns>
    public static Func<CodeFixContext, Task> CreateFixChain(
        string title,
        string equivalenceKey,
        Func<Future<FixState>, Future<FixState>> pipelineBuilder)
    {
        return async context =>
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => ExecutePipelineAsync(
                        context.Document,
                        context.Diagnostics[0],
                        pipelineBuilder,
                        ct),
                    equivalenceKey: equivalenceKey
                ),
                context.Diagnostics);

            await Task.CompletedTask;
        };
    }

    /// <summary>
    /// Executes a fix pipeline and returns the changed document.
    /// </summary>
    private static async Task<Document> ExecutePipelineAsync(
        Document document,
        Diagnostic diagnostic,
        Func<Future<FixState>, Future<FixState>> pipelineBuilder,
        CancellationToken token)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        FixState initial = new FixState(document, diagnostic, root!).WithCancellation(token);

        Future<FixState> future = new Future<FixState>(_ => Task.FromResult(initial));
        Future<FixState> pipeline = pipelineBuilder(future);

        FixState final = await pipeline.RunAsync(token).ConfigureAwait(false);

        return final.Changes.IsEmpty ? document : document.WithSyntaxRoot(final.CurrentRoot);
    }

    /// <summary>
    /// Creates a simple fix chain registration function from a title, key, and pipeline builder.
    /// This is a convenience method for the most common use case.
    /// </summary>
    /// <param name="title">The title for the code fix.</param>
    /// <param name="equivalenceKey">The equivalence key for the code fix.</param>
    /// <param name="pipelineBuilder">Function that builds the pipeline from input Future.</param>
    /// <returns>Registration configuration that can be used with code fix providers.</returns>
    public static (string Title, string EquivalenceKey, Func<CodeFixContext, Task> Register) CreateFixConfiguration(
        string title,
        string equivalenceKey,
        Func<Future<FixState>, Future<FixState>> pipelineBuilder)
    {
        return (title, equivalenceKey, CreateFixChain(title, equivalenceKey, pipelineBuilder));
    }
}
