using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Internal;

public abstract class ArithmeticTypeDescriptor : ITypeDescriptor
{
    public virtual IReadOnlyList<PropertyInfo> Properties => Array.Empty<PropertyInfo>();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public abstract Type Type { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public abstract Type ArrayOfType { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public abstract Type EnumerableOfType { get; }

    public bool IsArithmetic => true;

    public bool IsEnum => false;

    public bool IsValue => true;

    public abstract object? BoxNullable(object value);

    public abstract bool IsAssignableTo(Type baseType);

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

    public virtual bool IsNullable([MaybeNullWhen(false)] out Type elementType)
    {
        elementType = default;
        return false;
    }

    public abstract object Parse(string value);

    public abstract string? Stringify(object? value);

    public bool TryGetEnumFactory([MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        enumFactory = default;
        return false;
    }

    public abstract Expression CreateBoxedConstant(object? value);

    public Expression CreateAndAlso(Expression self, Expression right)
        => throw new NotSupportedException();

    public Expression CreateOrElse(Expression self, Expression right)
        => throw new NotSupportedException();

    protected abstract (Expression Left, Expression Right) UnifyExpressionTypes(Expression self, Expression right);

    public Expression CreateEqual(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Equal(l, r);
    }

    public Expression CreateNotEqual(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.NotEqual(l, r);
    }

    public Expression CreateGreaterThan(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.GreaterThan(l, r);
    }

    public Expression CreateGreaterThanOrEqual(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.GreaterThanOrEqual(l, r);
    }

    public Expression CreateLessThan(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.LessThan(l, r);
    }

    public Expression CreateLessThanOrEqual(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.LessThanOrEqual(l, r);
    }

    public Expression CreateAdd(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Add(l, r);
    }

    public Expression CreateSubtract(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Subtract(l, r);
    }

    public Expression CreateMultiply(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Multiply(l, r);
    }

    public Expression CreateDivide(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Divide(l, r);
    }

    public Expression CreateModulo(Expression self, Expression right)
    {
        var (l, r) = UnifyExpressionTypes(self, right);
        return Expression.Modulo(l, r);
    }

    public abstract MethodInfo EnumerableAnyMethod { get; }

    public abstract MethodInfo EnumerableAllMethod { get; }
}