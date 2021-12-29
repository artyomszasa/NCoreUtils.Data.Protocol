using System;

namespace NCoreUtils.Data.Protocol.Internal;

public interface IFunctionDescriptorResolverDescriptor
{
    IFunctionDescriptorResolver GetOrCreate(IServiceProvider serviceProvider);
}