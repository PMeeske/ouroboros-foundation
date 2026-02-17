namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a rewrite rule for AST optimization.
/// </summary>
/// <param name="Name">The name of the rewrite rule.</param>
/// <param name="Pattern">The AST pattern to match.</param>
/// <param name="Replacement">The replacement AST pattern.</param>
public sealed record RewriteRule(
    string Name,
    ASTNode Pattern,
    ASTNode Replacement);