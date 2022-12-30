using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class StringDescriptor : ITypeDescriptor
{
    public sealed class Box
    {
        public readonly string? Value;

        public Box(string? value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, string?>>)(e => e.Value)).Body).Member;

    object ITypeDescriptor.Parse(string value)
        => Parse(value);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type => typeof(string);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ArrayOfType => typeof(string[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type EnumerableOfType => typeof(IEnumerable<string>);

    public IReadOnlyList<PropertyInfo> Properties { get; } = new PropertyInfo[]
    {
        (PropertyInfo)((MemberExpression)((Expression<Func<string, int>>)(e => e.Length)).Body).Member
    };

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => false;

    public object? BoxNullable(object value)
        => throw new InvalidOperationException($"Unable to create nullable from reference type.");

    public bool IsAssignableTo(Type baseType)
        => baseType.Equals(typeof(string));

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

    public string Parse(string value)
        => value;

    public string? Stringify(object? value) => value as string;

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((string?)value)),
            BoxValueField
        );

    public Expression CreateAndAlso(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateOrElse(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right)
        => Expression.Equal(self, right);

    public Expression CreateNotEqual(Expression self, Expression right)
        => Expression.NotEqual(self, right);

    public Expression CreateGreaterThan(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateLessThan(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateAdd(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateSubtract(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateMultiply(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateDivide(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateModulo(Expression self, Expression right)
        => throw new NotSupportedException();

    public MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<string>, Func<string, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<string>, Func<string, bool>, bool>(Enumerable.All);

    public MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<string>, string, bool>(Enumerable.Contains);

    public void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<string>();
}