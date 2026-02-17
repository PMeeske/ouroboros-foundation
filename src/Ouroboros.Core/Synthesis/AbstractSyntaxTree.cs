namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents the abstract syntax tree of a program.
/// </summary>
/// <param name="Root">The root node of the AST.</param>
/// <param name="Depth">The maximum depth of the tree.</param>
/// <param name="NodeCount">The total number of nodes in the tree.</param>
public sealed record AbstractSyntaxTree(
    ASTNode Root,
    int Depth,
    int NodeCount);