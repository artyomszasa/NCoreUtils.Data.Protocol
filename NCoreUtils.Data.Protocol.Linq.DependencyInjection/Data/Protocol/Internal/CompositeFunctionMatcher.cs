using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

public class CompositeFunctionMatcher : IFunctionMatcher
{
    private IServiceProvider ServiceProvider { get; }

    public IReadOnlyList<IFunctionMatcherDescriptor> FunctionMatchers { get; }

    public CompositeFunctionMatcher(IServiceProvider serviceProvider, IReadOnlyList<IFunctionMatcherDescriptor> functionMatchers)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        FunctionMatchers = functionMatchers ?? throw new ArgumentNullException(nameof(functionMatchers));
    }

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
    {
        foreach (var functionMatcherDescriptor in FunctionMatchers)
        {
            var functionMatcher = functionMatcherDescriptor.GetOrCreate(ServiceProvider);
            try
            {
                var match = functionMatcher.MatchFunction(utils, expression);
                if (match.IsSuccess)
                {
                    return match;
                }
            }
            finally
            {
                (functionMatcher as IDisposable)?.Dispose();
            }
        }
        return default;
    }
}