using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

internal sealed class FunctionMatcherWrapper(IFunctionMatcher matcher) : IFunctionMatcherWrapper
{
    private IFunctionMatcher Matcher { get; } = matcher;

    public void Dispose()
        => (Matcher as IDisposable)?.Dispose();

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
        => Matcher.MatchFunction(utils, expression);
}