using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Antlr4.Runtime.Misc;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class Antrl4NodeVisitor : ProtocolBaseVisitor<Node>
{
    private static bool IsExprInParens(
        ProtocolParser.ExprContext context,
        [MaybeNullWhen(false)] out ProtocolParser.ExprContext exprInParens)
    {
        switch (context.expr())
        {
            case null:
                exprInParens = default;
                return false;
            case ProtocolParser.ExprContext[] expr when expr.Length == 1:
                exprInParens = expr[0];
                return true;
            default:
                exprInParens = default;
                return false;
        }
    }

    private static bool IsBinOp(
        ProtocolParser.ExprContext context,
        [MaybeNullWhen(false)] out ProtocolParser.ExprContext left,
        [MaybeNullWhen(false)] out string op,
        [MaybeNullWhen(false)] out ProtocolParser.ExprContext right)
    {
        switch (context.expr())
        {
            case null:
                left = default;
                op = default;
                right = default;
                return false;
            case ProtocolParser.ExprContext[] exprs when exprs.Length == 2:
                switch (context.binOp)
                {
                    case null:
                        left = default;
                        op = default;
                        right = default;
                        return false;
                    default:
                        left = exprs[0];
                        op = context.binOp.Text;
                        right = exprs[1];
                        return true;
                }
            default:
                left = default;
                op = default;
                right = default;
                return false;
        }
    }

    private static BinaryOperation ParseBinaryOperation(string raw) => raw switch
    {
        "&&" => BinaryOperation.AndAlso,
        "||" => BinaryOperation.OrElse,
        "="  => BinaryOperation.Equal,
        "!=" => BinaryOperation.NotEqual,
        "<"  => BinaryOperation.LessThan,
        ">"  => BinaryOperation.GreaterThan,
        "<=" => BinaryOperation.LessThanOrEqual,
        ">=" => BinaryOperation.GreaterThanOrEqual,
        "+"  => BinaryOperation.Add,
        "-"  => BinaryOperation.Substract,
        "/"  => BinaryOperation.Divide,
        "*"  => BinaryOperation.Multiply,
        "%"  => BinaryOperation.Modulo,
        _ => throw new ProtocolSyntaxException($"Invalid binary operator \"{raw}\".")
    };

    private static string? UnquoteString(string? source)
    {
        if (source is null)
        {
            return default;
        }
        if (!source.Contains('\\'))
        {
            return source[1..^1];
        }
        var sourceSpan = source.AsSpan();
        var buffer = ArrayPool<char>.Shared.Rent(source.Length);
        try
        {
            var builder = new SpanBuilder(buffer);
            var i = 1;
            var last = source.Length - 1;
            var beforeLast = last - 1;
            while (i < last)
            {
                var ch = sourceSpan[i];
                if (ch == '\\' && i < beforeLast)
                {
                    builder.Append(sourceSpan[i + 1]);
                    i += 2;
                }
                else
                {
                    builder.Append(ch);
                    ++i;
                }
            }
            return builder.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    private LambdaArgumentLookup LambdaArguments { get; } = new();

    private UniqueString GetLambdaArgument(string raw)
        => LambdaArguments.TryGetValue(raw, out var arg)
            ? arg
            : throw new InvalidOperationException($"Ident \"{raw}\" is not valid in this context.");

    public override Node VisitStart(ProtocolParser.StartContext context)
        => context.lambda() switch
        {
            null => VisitExpr(context.expr()),
            var lambda => VisitLambda(lambda)
        };

    public override Node VisitLambda(ProtocolParser.LambdaContext context)
    {
        var argString = context.IDENT().Symbol.Text;
        var arg = new UniqueString(argString);
        LambdaArguments.Add(argString, arg);
        var body = VisitExpr(context.expr());
        LambdaArguments.Remove(argString);
        return Node.Lambda(Node.Identifier(arg), body);
    }

    public override Node VisitExpr(ProtocolParser.ExprContext context)
    {
        if (IsExprInParens(context, out var inner))
        {
            return VisitExpr(inner);
        }
        if (IsBinOp(context, out var left, out var op, out var right))
        {
            return Node.Binary(
                VisitExpr(left),
                ParseBinaryOperation(op),
                VisitExpr(right)
            );
        }
        return base.VisitExpr(context);
    }

    public override Node VisitCall(ProtocolParser.CallContext context)
    {
        var name = context.IDENT().GetText();
        var args = context.args() switch
        {
            null => (IReadOnlyList<Node>)Array.Empty<Node>(),
            var argsContext => argsContext.expr()
                .Where(e => e is not null)
                .Select(VisitExpr)
                .ToList()
        };
        return Node.Call(name, args);
    }

    public override Node VisitIdent(ProtocolParser.IdentContext context)
    {
        var idents = context.IDENT();
        if (idents is null || idents.Length == 0)
        {
            throw new ProtocolSyntaxException($"Zero length ident at {context.SourceInterval.a} (\"{context.GetText()}\")");
        }
        if (idents.Length == 1)
        {
            return idents[0].GetText() switch
            {
                "true" => Node.Constant("true"),
                "false" => Node.Constant("false"),
                "null" => Node.Constant(null),
                var raw => Node.Identifier(GetLambdaArgument(raw))
            };
        }
        Node node = Node.Identifier(GetLambdaArgument(idents[0].GetText()));
        for (var i = 1; i < idents.Length; ++i)
        {
            node = Node.Member(node, idents[i].GetText());
        }
        return node;
    }

    public override Node VisitNumValue(ProtocolParser.NumValueContext context)
        => Node.Constant(context.NUMVALUE().Symbol.Text);

    public override Node VisitStringValue(ProtocolParser.StringValueContext context)
        => Node.Constant(UnquoteString(context.STRINGVALUE().Symbol.Text));
}