using System;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq;

internal static class ExpressionParserExtensions
{
    public static Lambda ParseLambdaExpression(this ExpressionParser parser, Expression source)
        => parser.ParseExpression(source) switch
        {
            Lambda l => l,
            var n => throw new InvalidOperationException($"Parsing {source} resulted in non-lambda expression {n}.")
        };
}