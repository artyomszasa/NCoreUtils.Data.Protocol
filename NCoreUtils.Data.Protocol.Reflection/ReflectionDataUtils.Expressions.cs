using System;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public partial class ReflectionDataUtils : IDataUtils
{
    public Expression CreateBoxedConstant(Type type, object? value)
        => BoxedConstantBuilder.BuildExpression(value, type);

    public Expression CreateAndAlso(Expression left, Expression right)
        => Expression.AndAlso(left, right);

    public Expression CreateOrElse(Expression left, Expression right)
        => Expression.OrElse(left, right);

    public Expression CreateEqual(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.Equal(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.Equal(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.Equal(left, right);
    }

    public Expression CreateNotEqual(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.NotEqual(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.NotEqual(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.NotEqual(left, right);
    }

    public Expression CreateGreaterThan(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.GreaterThan(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.GreaterThan(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.GreaterThan(left, right);
    }

    public Expression CreateGreaterThanOrEqual(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.GreaterThanOrEqual(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.GreaterThanOrEqual(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.GreaterThanOrEqual(left, right);
    }

    public Expression CreateLessThan(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.LessThan(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.LessThan(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.LessThan(left, right);
    }

    public Expression CreateLessThanOrEqual(Expression left, Expression right)
    {
        if (IsNullable(left.Type, out var ltype) && right.Type == ltype)
        {
            return Expression.LessThanOrEqual(
                left,
                Expression.Convert(right, left.Type)
            );
        }
        if (IsNullable(right.Type, out var rtype) && left.Type == rtype)
        {
            return Expression.LessThanOrEqual(
                Expression.Convert(left, right.Type),
                right
            );
        }
        return Expression.LessThanOrEqual(left, right);
    }

    public Expression CreateAdd(Expression left, Expression right)
        => Expression.Add(left, right);

    public Expression CreateSubtract(Expression left, Expression right)
        => Expression.Subtract(left, right);

    public Expression CreateMultiply(Expression left, Expression right)
        => Expression.Multiply(left, right);

    public Expression CreateDivide(Expression left, Expression right)
        => Expression.Divide(left, right);

    public Expression CreateModulo(Expression left, Expression right)
        => Expression.Modulo(left, right);
}