namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a node in an abstract syntax tree.
/// </summary>
/// <param name="NodeType">The type of this node (e.g., "Apply", "Lambda", "Variable").</param>
/// <param name="Value">The value associated with this node (e.g., primitive name, variable name).</param>
/// <param name="Children">The child nodes of this node.</param>
public sealed record ASTNode(
    string NodeType,
    string Value,
    List<ASTNode> Children);