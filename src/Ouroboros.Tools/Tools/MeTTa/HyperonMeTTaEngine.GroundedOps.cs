// <copyright file="HyperonMeTTaEngine.GroundedOps.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tools.MeTTa;

using Ouroboros.Core.Hyperon;

/// <summary>
/// Grounded operations registration and core atom initialization for HyperonMeTTaEngine.
/// </summary>
public sealed partial class HyperonMeTTaEngine
{
    private static GroundedRegistry CreateDefaultGroundedOps()
    {
        GroundedRegistry registry = new();

        // Basic arithmetic
        registry.Register("+", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a + b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("-", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a - b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("*", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b))
            {
                return new[] { Atom.Sym((a * b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("/", (space, args) =>
        {
            if (args.Children.Count >= 2 &&
                double.TryParse(args.Children[0].ToSExpr(), out double a) &&
                double.TryParse(args.Children[1].ToSExpr(), out double b) &&
                b != 0)
            {
                return new[] { Atom.Sym((a / b).ToString()) };
            }

            return Enumerable.Empty<Atom>();
        });

        // Comparisons
        registry.Register("==", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                bool equal = args.Children[0].ToSExpr() == args.Children[1].ToSExpr();
                return new[] { Atom.Sym(equal ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("!=", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                bool notEqual = args.Children[0].ToSExpr() != args.Children[1].ToSExpr();
                return new[] { Atom.Sym(notEqual ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        // Logic
        registry.Register("and-all", (space, args) =>
        {
            bool result = args.Children.All(a => a.ToSExpr() == "True");
            return new[] { Atom.Sym(result ? "True" : "False") };
        });

        registry.Register("or-any", (space, args) =>
        {
            bool result = args.Children.Any(a => a.ToSExpr() == "True");
            return new[] { Atom.Sym(result ? "True" : "False") };
        });

        registry.Register("negate", (space, args) =>
        {
            if (args.Children.Count >= 1)
            {
                bool result = args.Children[0].ToSExpr() != "True";
                return new[] { Atom.Sym(result ? "True" : "False") };
            }

            return Enumerable.Empty<Atom>();
        });

        // String operations
        registry.Register("concat-str", (space, args) =>
        {
            string result = string.Concat(args.Children.Select(a => a.ToSExpr().Trim('"')));
            return new[] { Atom.Sym($"\"{result}\"") };
        });

        // List operations
        registry.Register("cons", (space, args) =>
        {
            if (args.Children.Count >= 2)
            {
                return new[] { Atom.Expr(args.Children.ToArray()) };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("car", (space, args) =>
        {
            if (args.Children.Count >= 1 && args.Children[0] is Expression expr && expr.Children.Count > 0)
            {
                return new[] { expr.Children[0] };
            }

            return Enumerable.Empty<Atom>();
        });

        registry.Register("cdr", (space, args) =>
        {
            if (args.Children.Count >= 1 && args.Children[0] is Expression expr && expr.Children.Count > 1)
            {
                return new[] { Atom.Expr(expr.Children.Skip(1).ToArray()) };
            }

            return Enumerable.Empty<Atom>();
        });

        // Identity
        registry.Register("identity", (space, args) =>
        {
            return args.Children.Count >= 1 ? new[] { args.Children[0] } : Enumerable.Empty<Atom>();
        });

        // Print (for debugging)
        registry.Register("println", (space, args) =>
        {
            string output = string.Join(" ", args.Children.Select(a => a.ToSExpr()));
            Console.WriteLine($"[MeTTa] {output}");
            return new[] { Atom.Sym("()") };
        });

        return registry;
    }

    private void InitializeCoreAtoms()
    {
        // Core type atoms
        AddAtom(Atom.Sym("Type"));
        AddAtom(Atom.Sym("Atom"));
        AddAtom(Atom.Sym("Symbol"));
        AddAtom(Atom.Sym("Variable"));
        AddAtom(Atom.Sym("Expression"));

        // Boolean constants
        AddAtom(Atom.Sym("True"));
        AddAtom(Atom.Sym("False"));

        // Type declarations
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("True"), Atom.Sym("Bool")));
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("False"), Atom.Sym("Bool")));

        // Function type constructor
        AddAtom(Atom.Expr(Atom.Sym(":"), Atom.Sym("->"), Atom.Expr(Atom.Sym("->"), Atom.Sym("Type"), Atom.Sym("Type"), Atom.Sym("Type"))));

        // Basic inference rule
        AddAtom(Atom.Expr(
            Atom.Sym("="),
            Atom.Expr(Atom.Sym("if"), Atom.Sym("True"), Atom.Var("then"), Atom.Var("else")),
            Atom.Var("then")));
        AddAtom(Atom.Expr(
            Atom.Sym("="),
            Atom.Expr(Atom.Sym("if"), Atom.Sym("False"), Atom.Var("then"), Atom.Var("else")),
            Atom.Var("else")));
    }
}
