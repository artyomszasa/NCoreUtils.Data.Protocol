using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public class ArrayOf : IFunction
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static readonly Type _gReadOnlyCollection = typeof(IReadOnlyCollection<>);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static readonly Type _gEnumerable = typeof(IEnumerable<>);

    private static bool IsInterfaceEnumerable(Type @interface, [MaybeNullWhen(false)] out Type elementType)
    {
        if (@interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == _gEnumerable)
        {
            elementType = @interface.GetGenericArguments()[0];
            return true;
        }
        elementType = default;
        return false;
    }

    private static bool IsEnumerable(Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsInterface)
        {
            return IsInterfaceEnumerable(type, out elementType);
        }
        foreach (var @interface in type.GetInterfaces())
        {
            if (IsInterfaceEnumerable(@interface, out var etype))
            {
                elementType = etype;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    private static bool IsInterfaceCollection(Type @interface, [MaybeNullWhen(false)] out Type elementType, [MaybeNullWhen(false)] out PropertyInfo countProperty)
    {
        if (@interface.IsConstructedGenericType && @interface.GetGenericTypeDefinition() == _gReadOnlyCollection)
        {
            elementType = @interface.GetGenericArguments()[0];
            countProperty = @interface.GetProperty(nameof(IReadOnlyCollection<int>.Count), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Unable to get Count property for {@interface}.");
            return true;
        }
        elementType = default;
        countProperty = default;
        return false;
    }

    private static bool IsCollection(Type type, [MaybeNullWhen(false)] out Type elementType, [MaybeNullWhen(false)] out PropertyInfo countProperty)
    {
        if (type.IsInterface)
        {
            return IsInterfaceCollection(type, out elementType, out countProperty);
        }
        foreach (var @interface in type.GetInterfaces())
        {
            if (IsInterfaceCollection(@interface, out var etype, out var pcount))
            {
                elementType = etype;
                countProperty = pcount;
                return true;
            }
        }
        elementType = default;
        countProperty = default;
        return false;
    }

    private static bool IsCollectionLike([NotNullWhen(true)] object? source, [MaybeNullWhen(false)] out Type elementType, out int? length)
    {
        if (source is null)
        {
            elementType = default;
            length = default;
            return false;
        }
        var type = source.GetType();
        if (IsCollection(type, out var etype, out var countProperty))
        {
            elementType = etype;
            length = (int)countProperty.GetValue(source, null)!;
            return true;
        }
        if (IsEnumerable(type, out etype))
        {
            elementType = etype;
            length = default;
            return true;
        }
        elementType = default;
        length = default;
        return false;
    }

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
        if (expression.TryExtractConstant(out var boxed) && boxed is not string)
        {
            if (boxed is Array array)
            {
                var length = array.GetLength(0);
                var arrayExpression = Expression.Constant(array, array.GetType());
                var arguments = new Expression[length];
                for (var i = 0; i < arguments.Length; ++i)
                {
                    arguments[i] = Expression.ArrayIndex(arrayExpression, Expression.Constant(i, typeof(int)));
                }
                return new(Names.Array, arguments);
            }
            else if (IsCollectionLike(boxed, out var elementType, out var nlength))
            {
                var arguments = nlength is int length ? new List<Expression>(length) : new List<Expression>();
                foreach (var item in (System.Collections.IEnumerable)boxed)
                {
                    arguments.Add(Expression.Constant(item));
                }
                return new(Names.Array, arguments);
            }
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