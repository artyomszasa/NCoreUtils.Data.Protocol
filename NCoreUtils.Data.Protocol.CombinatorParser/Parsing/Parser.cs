using System;
using System.Collections.Immutable;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Lexing;

namespace NCoreUtils.Data.Protocol.Parsing;

public ref struct Parser(string input)
{
    private static BinaryOperation MapBinOp(TokenType tokenType) => tokenType switch
    {
        TokenType.And => BinaryOperation.AndAlso,
        TokenType.Div => BinaryOperation.Divide,
        TokenType.Eq => BinaryOperation.Equal,
        TokenType.Ge => BinaryOperation.GreaterThanOrEqual,
        TokenType.Gt => BinaryOperation.GreaterThan,
        TokenType.Le => BinaryOperation.LessThanOrEqual,
        TokenType.Lt => BinaryOperation.LessThan,
        TokenType.Minus => BinaryOperation.Subtract,
        TokenType.Mod => BinaryOperation.Modulo,
        TokenType.Mul => BinaryOperation.Multiply,
        TokenType.Neq => BinaryOperation.NotEqual,
        TokenType.Plus => BinaryOperation.Add,
        var tt => throw new ParserException($"Non-operator token: {tt}.")
    };

    private Preload Input = new(input);

    #region Ident

    private Node IdentDot(Node instance)
    {
        switch (Input.Next)
        {
            case { TokenType: TokenType.Ident } tok:
                Input.Advance();
                var member = Node.Member(instance, tok.Value!);
                if (Input.Next.TokenType == TokenType.Dot)
                {
                    Input.Advance();
                    return IdentDot(member);
                }
                return member;
            case var tok:
                throw new ParserException($"Expected Ident at {tok.StartPosition}.");
        }
    }

    #endregion

    #region Expr

    private Node ExprCallArg(Token ident, in ParserContext ctx, ImmutableList<Node> args)
    {
        var arg = Expr(in ctx);
        switch (Input.Next)
        {
            case { TokenType: TokenType.Rparen }:
                Input.Advance();
                return Node.Call(ident.Value!, args.Add(arg));
            case { TokenType: TokenType.Comma }:
                Input.Advance();
                return ExprCallArg(ident, in ctx, args.Add(arg));
            case var tok:
                throw new ParserException($"Unexpected {tok.TokenType} at {tok.StartPosition}.");
        }
    }

    private Node ExprCall(Token ident, in ParserContext ctx)
    {
        switch (Input.Next)
        {
            case { TokenType: TokenType.Rparen }:
                Input.Advance();
                return Node.Call(ident.Value!, Array.Empty<Node>());
            default:
                return ExprCallArg(ident, ctx.WithPrecedence(default), ImmutableList<Node>.Empty);
        }
    }

    private Node ExprParen(in ParserContext ctx)
    {
        var expr = Expr(in ctx);
        if (Input.Next.TokenType != TokenType.Rparen)
        {
            throw new ParserException($"Unexpected {Input.Next.TokenType} at {Input.Next.StartPosition}.");
        }
        Input.Advance();
        return expr;
    }

    private Node ExprIdent(Token ident, in ParserContext ctx)
    {
        switch (Input.Next)
        {
            case { TokenType: TokenType.Arrow }:
                // lambda
                var bodyCtx = ctx.AddParameter(ident.Value!, out var uname).WithPrecedence(default);
                Input.Advance();
                var body = Expr(in bodyCtx);
                return Node.Lambda(Node.Identifier(uname), body);
            case { TokenType: TokenType.Lparen }:
                // call
                Input.Advance();
                return ExprCall(ident, in ctx);
            case { TokenType: TokenType.Dot }:
                // composite indent
                Input.Advance();
                return IdentDot(Node.Identifier(ctx.GetParameter(ident.Value!)));
            default:
                // simple ident or "null"
                return ident.Value == "null"
                    ? Node.Constant(null)
                    : Node.Identifier(ctx.GetParameter(ident.Value!));
        }
    }

    private Node ExprFollow(Node left, in ParserContext ctx)
    {
        Node node;
        switch (Input.Next)
        {
            case { TokenType: TokenType.Mul or TokenType.Div or TokenType.Mod } tok:
                Input.Advance();
                node = Node.Binary(left, MapBinOp(tok.TokenType), Expr(ctx.WithPrecedence(PrecedenceContext.MulDivRem)));
                break;
            case { TokenType: TokenType.Plus or TokenType.Minus } tok when ctx.Precedence <= PrecedenceContext.PlusMinus:
                Input.Advance();
                node = Node.Binary(left, MapBinOp(tok.TokenType), Expr(ctx.WithPrecedence(PrecedenceContext.PlusMinus)));
                break;
            case { TokenType: TokenType.Eq or TokenType.Neq or TokenType.Gt or TokenType.Ge or TokenType.Lt or TokenType.Le } tok when ctx.Precedence <= PrecedenceContext.Boolean:
                Input.Advance();
                node = Node.Binary(left, MapBinOp(tok.TokenType), Expr(ctx.WithPrecedence(PrecedenceContext.Boolean)));
                break;
            case { TokenType: TokenType.And } when ctx.Precedence <= PrecedenceContext.And:
                Input.Advance();
                node = Node.Binary(left, BinaryOperation.AndAlso, Expr(ctx.WithPrecedence(PrecedenceContext.Boolean)));
                break;
            case { TokenType: TokenType.Or } when ctx.Precedence <= PrecedenceContext.Or:
                Input.Advance();
                node = Node.Binary(left, BinaryOperation.OrElse, Expr(ctx.WithPrecedence(PrecedenceContext.Boolean)));
                break;
            default:
                return left;
        }
        return ExprFollow(node, in ctx);
    }

    public Node Expr(in ParserContext ctx)
    {
        Node left;
        switch (Input.Next)
        {
            case { TokenType: TokenType.Lparen }:
                Input.Advance();
                left = ExprParen(ctx.WithPrecedence(default));
                break;
            case { TokenType: TokenType.Ident } tok:
                Input.Advance();
                left = ExprIdent(tok, in ctx);
                break;
            case { TokenType: TokenType.Num or TokenType.String } tok:
                Input.Advance();
                left = Node.Constant(tok.Value);
                break;
            case var tok:
                throw new ParserException($"Unexpected {tok.TokenType} at {tok.StartPosition}.");
        }
        return ExprFollow(left, in ctx);
    }

    #endregion

    public Node Start()
    {
        var ctx = ParserContext.Initial();
        var node = Expr(ctx);
        if (Input.Next.TokenType != TokenType.Eos)
        {
            throw new InvalidOperationException("Should never happen: parsing exited before EOS.");
        }
        return node;
    }
}