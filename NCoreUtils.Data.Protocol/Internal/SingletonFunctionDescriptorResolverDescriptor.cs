using System;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class SingletonFunctionDescriptorResolverDescriptor : IFunctionDescriptorResolverDescriptor
{
    public IFunctionDescriptorResolver Instance { get; }

    public SingletonFunctionDescriptorResolverDescriptor(IFunctionDescriptorResolver instance)
        => Instance = instance switch
        {
            null => throw new ArgumentNullException(nameof(instance)),
            IDisposable _ => new SuppressDisposeFunctionDescriptorResolver<IFunctionDescriptorResolver>(instance),
            _ => instance
        };

    public IFunctionDescriptorResolver GetOrCreate(IServiceProvider serviceProvider)
        => Instance;

}