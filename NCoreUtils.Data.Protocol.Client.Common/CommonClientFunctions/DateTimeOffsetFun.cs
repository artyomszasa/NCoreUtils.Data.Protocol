using System;
using System.Linq.Expressions;
using Names = NCoreUtils.Data.Protocol.CommonFunctionNames;

namespace NCoreUtils.Data.Protocol.CommonClientFunctions;

public sealed class DateTimeOffsetFun : IFunctionMatcher
{
    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
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
                    utils.CreateBoxedConstant(typeof(long), value.UtcTicks)
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
                        utils.CreateBoxedConstant(typeof(long), constantValue.UtcTicks)
                    }
                );
            }
        }
        return default;
    }
}