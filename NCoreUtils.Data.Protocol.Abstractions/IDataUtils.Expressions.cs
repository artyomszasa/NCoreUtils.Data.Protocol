using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol;

public partial interface IDataUtils
{
    Expression CreateBoxedConstant(Type type, object? value);

    Expression CreateAndAlso(Expression left, Expression right);

    Expression CreateOrElse(Expression left, Expression right);

    Expression CreateEqual(Expression left, Expression right);

    Expression CreateNotEqual(Expression left, Expression right);

    Expression CreateGreaterThan(Expression left, Expression right);

    Expression CreateGreaterThanOrEqual(Expression left, Expression right);

    Expression CreateLessThan(Expression left, Expression right);

    Expression CreateLessThanOrEqual(Expression left, Expression right);

    Expression CreateAdd(Expression left, Expression right);

    Expression CreateSubtract(Expression left, Expression right);

    Expression CreateMultiply(Expression left, Expression right);

    Expression CreateDivide(Expression left, Expression right);

    Expression CreateModulo(Expression left, Expression right);
}