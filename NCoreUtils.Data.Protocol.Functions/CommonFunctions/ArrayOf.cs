using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public class ArrayOf : IFunction
{
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Only types passed by user can appear here therefore they are preserved anyway.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070",
        Justification = "Only types passed by user can appear here therefore they are preserved anyway.")]
    public static ArrayOfDescriptor CreateDescritor(Type elementType, int size)
        => (ArrayOfDescriptor)Activator.CreateInstance(
            typeof(ArrayOfDescriptor<>).MakeGenericType(elementType),
            new object[] { size }
        )!;

    public FunctionMatch MatchFunction(Expression expression)
    {
        if (expression is NewArrayExpression newArrayExpression && expression.NodeType == ExpressionType.NewArrayInit)
        {
            return new(Names.Array, newArrayExpression.Expressions);
        }
        if (expression.TryExtractConstant(out var boxed) && boxed is not null && boxed is Array array)
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
        return default;
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
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(Names.Array, name))
        {
            if (Helpers.TryGetElementType(resultTypeConstraints, out var elementType)
                || TryGetArgumentType(argumentTypeConstraints, out elementType))
            {
                descriptor = CreateDescritor(elementType, argumentTypeConstraints.Count);
                return true;
            }
        }
        descriptor = default;
        return false;
    }
}