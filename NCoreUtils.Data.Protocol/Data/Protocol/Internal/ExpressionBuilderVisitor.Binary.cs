using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

public partial class ExpressionBuilderVisitor
{
    private static BinaryExpression CreateEqual(Expression left, Expression right)
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

    private static BinaryExpression CreateNotEqual(Expression left, Expression right)
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

    private static BinaryExpression CreateGreaterThan(Expression left, Expression right)
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

    private static BinaryExpression CreateGreaterThanOrEqual(Expression left, Expression right)
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

    private static BinaryExpression CreateLessThan(Expression left, Expression right)
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

    private static BinaryExpression CreateLessThanOrEqual(Expression left, Expression right)
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
}