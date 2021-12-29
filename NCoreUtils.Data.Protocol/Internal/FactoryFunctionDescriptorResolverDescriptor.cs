using System;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class FactoryFunctionDescriptorResolverDescriptor : IFunctionDescriptorResolverDescriptor
{
    public Func<IServiceProvider, IFunctionDescriptorResolver> Factory;

    public FactoryFunctionDescriptorResolverDescriptor(Func<IServiceProvider, IFunctionDescriptorResolver> factory)
        => Factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public IFunctionDescriptorResolver GetOrCreate(IServiceProvider serviceProvider)
        => Factory(serviceProvider);
}