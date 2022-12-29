using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol;

public partial class PortableDataUtils
{
    public Expression CreateAdd(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateAdd(left, right);

    public Expression CreateAndAlso(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateAndAlso(left, right);

    public Expression CreateBoxedConstant(Type type, object? value)
        => GetDescriptor(type).CreateBoxedConstant(value);

    public Expression CreateDivide(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateDivide(left, right);

    public Expression CreateEqual(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateEqual(left, right);

    public Expression CreateGreaterThan(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateGreaterThan(left, right);

    public Expression CreateGreaterThanOrEqual(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateGreaterThanOrEqual(left, right);

    public Expression CreateLessThan(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateLessThan(left, right);

    public Expression CreateLessThanOrEqual(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateLessThanOrEqual(left, right);

    public Expression CreateModulo(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateModulo(left, right);

    public Expression CreateMultiply(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateMultiply(left, right);

    public Expression CreateNotEqual(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateNotEqual(left, right);

    public Expression CreateOrElse(Expression left, Expression right)
        => GetDescriptor(left.Type).CreateOrElse(left, right);

    public Expression CreateSubtract(Expression left, Expression right)
    => GetDescriptor(left.Type).CreateSubtract(left, right);
}