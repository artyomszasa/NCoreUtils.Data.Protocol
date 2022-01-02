using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class DateTimeOffsetFun : IFunction
{
    public FunctionMatch MatchFunction(Expression expression)
    {
        if (expression.Type != typeof(DateTimeOffset))
        {
            return default;
        }
        if (expression.TryExtractConstant(out var boxed) && boxed is DateTimeOffset value)
        {
            return new(
                Names.DateTimeOffset,
                new Expression[] {
                    Expression.Constant(value.UtcTicks, typeof(long))
                }
            );
        }
        if (expression is NewExpression newExpression)
        {
            if (newExpression.Arguments.Count == 2
                && newExpression.Arguments[0].Type == typeof(long)
                && newExpression.Arguments[1].Type == typeof(TimeSpan)
                && newExpression.Arguments[1].TryExtractConstant(out var boxedOffset)
                && boxedOffset is TimeSpan offset
                && offset == TimeSpan.Zero)
            {
                return new(
                    Names.DateTimeOffset,
                    new Expression[] {
                        newExpression.Arguments[0]
                    }
                );
            }
            // expression created from constants
            if (newExpression.Constructor is not null)
            {
                var arguments = new object?[newExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; ++i)
                {
                    if (!newExpression.Arguments[i].TryExtractConstant(out var argValue))
                    {
                        return default;
                    }
                    arguments[i] = argValue;
                }
                var constantValue = (DateTimeOffset)newExpression.Constructor.Invoke(arguments);
                return new(
                    Names.DateTimeOffset,
                    new Expression[] {
                        Expression.Constant(constantValue.UtcTicks, typeof(long))
                    }
                );
            }
        }
        return default;
    }

    public bool TryResolveFunction(
        string name,
        TypeVariable resultTypeConstraints,
        IReadOnlyList<TypeVariable> argumentTypeConstraints,
        [MaybeNullWhen(false)] out IFunctionDescriptor descriptor)
    {
        if (StringComparer.InvariantCultureIgnoreCase.Equals(name, Names.DateTimeOffset)
            && argumentTypeConstraints.Count == 1
            && argumentTypeConstraints[0].IsCompatible<long>()
            && resultTypeConstraints.IsCompatible<DateTimeOffset>())
        {
            descriptor = DateTimeOffsetDescriptor.Singleton;
            return true;
        }
        descriptor = default;
        return false;
    }
}