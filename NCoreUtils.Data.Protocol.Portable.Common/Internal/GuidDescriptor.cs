using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class GuidDescriptor : ITypeDescriptor
{
    public sealed class Box
    {
        public readonly Guid Value;

        public Box(Guid value) => Value = value;

        public override string ToString() => $"{{{Value}}}";
    }

    private FieldInfo BoxValueField { get; } = (FieldInfo)((MemberExpression)((Expression<Func<Box, Guid>>)(e => e.Value)).Body).Member;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type => typeof(Guid);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ArrayOfType => typeof(Guid[]);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type EnumerableOfType => typeof(IEnumerable<Guid>);

    public IReadOnlyList<PropertyInfo> Properties => Array.Empty<PropertyInfo>();

    public bool IsArithmetic => false;

    public bool IsEnum => false;

    public bool IsValue => true;

    public object? BoxNullable(object value)
        => (Guid?)(Guid)value;

    public Expression CreateAdd(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateAndAlso(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateBoxedConstant(object? value)
        => Expression.Field(
            Expression.Constant(new Box((Guid)value!)),
            BoxValueField
        );

    public Expression CreateDivide(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateEqual(Expression self, Expression right)
        => right.Type == typeof(Guid)
            ? Expression.Equal(self, right)
            : right.Type == typeof(Guid?)
                ? Expression.Equal(Expression.Convert(self, typeof(Guid?)), right)
                : throw new InvalidOperationException($"Cannot create Equal expression from Guid and {right.Type}.");

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
        => right.Type == typeof(Guid)
            ? Expression.NotEqual(self, right)
            : right.Type == typeof(Guid?)
                ? Expression.NotEqual(Expression.Convert(self, typeof(Guid?)), right)
                : throw new InvalidOperationException($"Cannot create NotEqual expression from Guid and {right.Type}.");

    public Expression CreateOrElse(Expression self, Expression right)
        => throw new NotSupportedException();

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

    public object Parse(string value) => string.IsNullOrEmpty(value) ? default : Guid.Parse(value);

    public string Stringify(object? value) => ((Guid)value!).ToString();


    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public MethodInfo EnumerableAnyMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<Guid>, Func<Guid, bool>, bool>(Enumerable.Any);

    public MethodInfo EnumerableAllMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<Guid>, Func<Guid, bool>, bool>(Enumerable.All);

    public MethodInfo EnumerableContainsMethod { get; } = ReflectionHelpers.GetMethod<IEnumerable<Guid>, Guid, bool>(Enumerable.Contains);

    public void Accept(IDataTypeVisitor visitor)
        => visitor.Visit<Guid>();
}