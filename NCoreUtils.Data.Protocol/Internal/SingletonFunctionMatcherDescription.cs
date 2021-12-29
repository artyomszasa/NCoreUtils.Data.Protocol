using System;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class SingletonFunctionMatcherDescriptor : IFunctionMatcherDescriptor
{
    public IFunctionMatcher Instance { get; }

    public SingletonFunctionMatcherDescriptor(IFunctionMatcher instance)
        => Instance = instance switch
        {
            null => throw new ArgumentNullException(nameof(instance)),
            IDisposable _ => new SuppressDisposeFunctionMatcher<IFunctionMatcher>(instance),
            _ => instance
        };

    public IFunctionMatcher GetOrCreate(IServiceProvider serviceProvider)
        => Instance;
}