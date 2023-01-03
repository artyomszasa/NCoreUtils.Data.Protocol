using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public class ArrayOf : IFunctionMatcher
{
    public static FunctionMatch MatchEnumerableOrArray(IDataUtils utils, Expression expression)
    {
        if (utils.IsArray(expression.Type) && expression.TryExtractConstant(out var boxed) && boxed is Array array)
        {
            var length = array.GetLength(0);
            var arrayExpression = Expression.Constant(array, expression.Type);
            var arguments = new Expression[length];
            for (var i = 0; i < arguments.Length; ++i)
            {
                arguments[i] = Expression.ArrayIndex(arrayExpression, Expression.Constant(i, typeof(int)));
            }
            return new(Names.Array, arguments);
        }
        if (utils.IsEnumerable(expression.Type, out var elementType)
            && expression.TryExtractConstant(out boxed)
            && boxed is System.Collections.IEnumerable enumerable)
        {
            var arguments = new List<Expression>();
            foreach (var item in enumerable)
            {
                arguments.Add(utils.CreateBoxedConstant(elementType, item));
            }
            return new(Names.Array, arguments);
        }
        return default;
    }

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        if (expression is NewArrayExpression newArrayExpression && expression.NodeType == ExpressionType.NewArrayInit)
        {
            return new(Names.Array, newArrayExpression.Expressions);
        }
        return MatchEnumerableOrArray(utils, expression);
    }
}