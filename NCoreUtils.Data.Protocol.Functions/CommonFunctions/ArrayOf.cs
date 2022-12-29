using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public class ArrayOf : IFunction
{
    public static ArrayOfDescriptor CreateDescritor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type arrayType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type elementType,
        int size)
        => new(arrayType, elementType, size);

    public FunctionMatch MatchEnumerableOrArray(IDataUtils utils, Expression expression)
    {
        if (utils.IsArray(expression.Type, out var elementType) && expression.TryExtractConstant(out var boxed) && boxed is Array array)
        {
            var length = array.GetLength(0);
            var arrayExpression = Expression.Constant(array, elementType);
            var arguments = new Expression[length];
            for (var i = 0; i < arguments.Length; ++i)
            {
                arguments[i] = Expression.ArrayIndex(arrayExpression, Expression.Constant(i, typeof(int)));
            }
            return new(Names.Array, arguments);
        }
        if (utils.IsEnumerable(expression.Type, out elementType) && expression.TryExtractConstant(out boxed) && boxed is System.Collections.IEnumerable enumerable)
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

    private static bool TryGetArgumentType(
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out Type elementType)
    {
        foreach (var argTypeConstraints in argumentTypeConstraints)
        {
            if (argTypeConstraints.TryGetExactType(out var type))
            {
                elementType = type;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public bool TryResolveFunction(
        IDataUtils util,
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(Names.Array, name))
        {
            if (resultTypeConstraints.TryGetElementType(util, out var elementType)
                || TryGetArgumentType(argumentTypeConstraints, out elementType))
            {
                descriptor = CreateDescritor(
                    util.Ensure(util.GetArrayOfType(elementType)),
                    util.Ensure(elementType),
                    argumentTypeConstraints.Count
                );
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}