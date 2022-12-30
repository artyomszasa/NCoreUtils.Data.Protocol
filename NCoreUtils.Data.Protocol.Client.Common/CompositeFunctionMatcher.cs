using System.Collections.Generic;
using System.Linq.Expressions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public class CompositeFunctionMatcher : IFunctionMatcher
{
    private IEnumerable<IFunctionMatcherWrapper> Matchers { get; }

    internal CompositeFunctionMatcher(IEnumerable<IFunctionMatcherWrapper> matchers)
        => Matchers = matchers;

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        foreach (var matcher in Matchers)
        {
            var res = matcher.MatchFunction(utils, expression);
            if (res.IsSuccess)
            {
                return res;
            }
        }
        return default;
    }
}