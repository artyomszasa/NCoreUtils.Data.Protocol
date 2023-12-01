using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public class ExpressionParser(IDataUtils utils, IFunctionMatcher functionMatcher)
{
    private static Dictionary<ExpressionType, BinaryOperation> BinaryOperationMap { get; } = new()
    {
        { ExpressionType.Equal,              BinaryOperation.Equal },
        { ExpressionType.NotEqual,           BinaryOperation.NotEqual },
        { ExpressionType.LessThan,           BinaryOperation.LessThan },
        { ExpressionType.LessThanOrEqual,    BinaryOperation.LessThanOrEqual },
        { ExpressionType.GreaterThan,        BinaryOperation.GreaterThan },
        { ExpressionType.GreaterThanOrEqual, BinaryOperation.GreaterThanOrEqual },
        { ExpressionType.OrElse,             BinaryOperation.OrElse },
        { ExpressionType.AndAlso,            BinaryOperation.AndAlso },
        { ExpressionType.Add,                BinaryOperation.Add },
        { ExpressionType.Subtract,           BinaryOperation.Subtract },
        { ExpressionType.Multiply,           BinaryOperation.Multiply },
        { ExpressionType.Divide,             BinaryOperation.Divide },
        { ExpressionType.Modulo,             BinaryOperation.Modulo }
    };

    protected IDataUtils Utils { get; } = utils ?? throw new ArgumentNullException(nameof(utils));

    protected IFunctionMatcher FunctionMatcher { get; } = functionMatcher ?? throw new ArgumentNullException(nameof(functionMatcher));

    protected bool IsNullableHasValue(MemberExpression expression)
        => expression.Member.Name == nameof(Nullable<int>.HasValue)
            && expression.Expression is not null
            && Utils.IsNullable(expression.Type);

    protected Constant CreateConstant(Type type, object? value)
        => value switch
        {
            null => Node.Constant(null),
            string s => Node.Constant(s),
            object any => Node.Constant(Utils.Stringify(type, any))
        };

    protected virtual Node VisitMember(MemberExpression node, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
    {
        if (node.Expression is null)
        {
            throw new InvalidOperationException("Static members are not supported.");
        }
        if (IsNullableHasValue(node))
        {
            return Node.Binary(
                left: Visit(node.Expression, ctx, functionMatcher),
                operation: BinaryOperation.Equal,
                right: Node.Constant(null)
            );
        }
        return Node.Member(
            instance: Visit(node.Expression, ctx, functionMatcher),
            memberName: node.Member.Name
        );
    }

    protected virtual Node VisitBinary(BinaryExpression expression, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
    {
        if (BinaryOperationMap.TryGetValue(expression.NodeType, out var operation))
        {
            return Node.Binary(
                left: Visit(expression.Left, ctx, functionMatcher),
                operation: operation,
                right: Visit(expression.Right, ctx, functionMatcher)
            );
        }
        if (expression.NodeType == ExpressionType.ArrayIndex
            && expression.Left.TryExtractConstant(out var boxedArray) && boxedArray is Array array
            && expression.Right.TryExtractConstant(out var boxedIndex) && boxedIndex is int index)
        {
            var itemValue = array.GetValue(index);
            // string? rawItemValue;
            // if (itemValue is null)
            // {
            //     rawItemValue = default;
            // }
            // else
            // {
            //     var type = itemValue.GetType();
            //     if (type.IsEnum)
            //     {
            //         rawItemValue = Convert.ChangeType(itemValue, Enum.GetUnderlyingType(type)).ToString();
            //     }
            //     else
            //     {
            //         rawItemValue = itemValue.ToString();
            //     }
            // }
            var rawItemValue = Utils.Stringify(expression.Type, itemValue);
            return Node.Constant(rawItemValue);
        }
        throw new NotSupportedException($"Unsupported binary operation {expression.NodeType}.");
    }

    protected virtual Node VisitParameter(ParameterExpression parameter, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
        => Node.Identifier(ctx.GetParameterName(parameter));

    protected virtual Node VisitLambda(LambdaExpression lambda, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
    {
        if (lambda.Parameters.Count != 1)
        {
            throw new NotSupportedException($"Only lambdas with single parameter are supported.");
        }
        var parameter = lambda.Parameters[0];
        var uname = ctx.Add(parameter);
        return Node.Lambda(
            Node.Identifier(uname),
            Visit(lambda.Body, ctx, functionMatcher)
        );
    }

    protected virtual Node VisitUnary(UnaryExpression unary, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
        => unary.NodeType switch
        {
            ExpressionType.Not => Node.Binary(
                left: Visit(unary.Operand, ctx, functionMatcher),
                operation: BinaryOperation.Equal,
                Node.Constant("false")
            ),
            ExpressionType.Quote => Visit(unary.Operand, ctx, functionMatcher),
            ExpressionType.Convert => Visit(unary.Operand, ctx, functionMatcher),
            _ => throw new NotSupportedException($"Unsupported unary expression {unary}")
        };


    protected virtual Node Visit(Expression expression, UniqueStringMap ctx, IFunctionMatcher functionMatcher)
    {
        // function call may match any node.
        var match = functionMatcher.MatchFunction(Utils, expression);
        if (match.IsSuccess)
        {
            return Node.Call(match.Name, match.Arguments.MapToArray(node => Visit(node, ctx, functionMatcher)));
        }
        // check if can be handled as constant
        if (expression.TryExtractInstance(out var instance))
        {
            return CreateConstant(expression.Type, instance);
        }
        // visit by type
        return expression switch
        {
            MemberExpression memberExpression => VisitMember(memberExpression, ctx, functionMatcher),
            BinaryExpression binaryExpression => VisitBinary(binaryExpression, ctx, functionMatcher),
            ParameterExpression parameterExpression => VisitParameter(parameterExpression, ctx, functionMatcher),
            LambdaExpression lambdaExpression => VisitLambda(lambdaExpression, ctx, functionMatcher),
            UnaryExpression unaryExpression => VisitUnary(unaryExpression, ctx, functionMatcher),
            _ => throw new NotSupportedException($"Unsupported expression: {expression}.")
        };
    }

    public Node ParseExpression(Expression expression)
        => Visit(expression, new UniqueStringMap(), FunctionMatcher);
}