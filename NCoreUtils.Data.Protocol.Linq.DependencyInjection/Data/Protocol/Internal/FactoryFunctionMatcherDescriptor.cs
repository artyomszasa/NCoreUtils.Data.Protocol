using System;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class FactoryFunctionMatcherDescriptor : IFunctionMatcherDescriptor
{
    public Func<IServiceProvider, IFunctionMatcher> Factory { get; }

    public FactoryFunctionMatcherDescriptor(Func<IServiceProvider, IFunctionMatcher> factory)
        => Factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public IFunctionMatcher GetOrCreate(IServiceProvider serviceProvider)
        => Factory(serviceProvider);
}