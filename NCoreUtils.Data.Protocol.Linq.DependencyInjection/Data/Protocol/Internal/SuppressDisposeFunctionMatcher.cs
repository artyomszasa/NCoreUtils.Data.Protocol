using System;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Internal;

/// <summary>
/// Hides disposablity of the underlying function matcher. Used when function matcher is disposable but disposing
/// handled by the DI container.
/// </summary>
/// <typeparam name="T">Type of the underlying function matcher.</typeparam>
internal sealed class SuppressDisposeFunctionMatcher<T> : IFunctionMatcher
    where T : class, IFunctionMatcher
{
    private T FunctionMatcherService { get; }

    public SuppressDisposeFunctionMatcher(T functionMatcherService)
        => FunctionMatcherService = functionMatcherService ?? throw new ArgumentNullException(nameof(functionMatcherService));

    public FunctionMatch MatchFunction(IDataUtils utils, Expression expression)
        => FunctionMatcherService.MatchFunction(utils, expression);
}
