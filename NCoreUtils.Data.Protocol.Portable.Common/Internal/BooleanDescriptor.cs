using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class BooleanDescriptor : ITypeDescriptor
{
    public sealed class Box
    {
        public readonly bool Value;

        public Box(bool value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private static IReadOnlyList<string> Truthy { get; } = new [] { "true", "on", "1" };

    private static IReadOnlyList<string> Falsy { get; } = new [] { "false", "off", "0" };

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, bool>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type => typeof(bool);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ArrayOfType => typeof(bool[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type EnumerableOfType => typeof(IEnumerable<bool>);

    public IReadOnlyList<PropertyInfo> Properties => Array.Empty<PropertyInfo>();

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => true;

    public object? BoxNullable(object value)
        => (bool?)(bool)value;

    public Expression CreateAdd(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateAndAlso(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.AndAlso(self, right)
            : throw new InvalidOperationException($"Cannot create AndAlso expression from bool and {right.Type}.");

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((bool)value!)),
            BoxValueField
        );

    public Expression CreateDivide(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.Equal(self, right)
            : right.Type == typeof(bool?)
                ? Expression.Equal(Expression.Convert(self, typeof(bool?)), right)
                : throw new InvalidOperationException($"Cannot create AndAlso expression from bool and {right.Type}.");

    public Expression CreateGreaterThan(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateLessThan(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateModulo(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateMultiply(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateNotEqual(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(bool?)
                ? Expression.NotEqual(Expression.Convert(self, typeof(bool?)), right)
                : throw new InvalidOperationException($"Cannot create AndAlso expression from bool and {right.Type}.");

    public Expression CreateOrElse(Expression self, Expression right)
        => right.Type == typeof(bool)
            ? Expression.OrElse(self, right)
            : throw new InvalidOperationException($"Cannot create OrElse expression from bool and {right.Type}.");

    public Expression CreateSubtract(Expression self, Expression right)
        => throw new NotSupportedException();

    public bool IsAssignableTo(Type baseType)
        => baseType == typeof(bool);

    public bool IsEnumerable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsArray([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsLambda([MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
    {
        argType = default;
        resType = default;
        return false;
    }

    public bool IsMaybe([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public object Parse(string value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to boolean."),
        _ when Truthy.Contains(value, StringComparer.InvariantCultureIgnoreCase) => true,
        _ when Falsy.Contains(value, StringComparer.InvariantCultureIgnoreCase) => false,
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to boolean.")
    };

    public string Stringify(object? value) => value switch
    {
        null => throw new InvalidOperationException("Unable to convert null to boolean."),
        bool b => b ? "true" : "false",
        _ => throw new InvalidOperationException($"Unable to convert \"{value}\" to boolean.")
    };


    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<bool>, Func<bool, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<bool>, Func<bool, bool>, bool>(Enumerable.All);

    public MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<bool>, bool, bool>(Enumerable.Contains);

    public void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<bool>();
}