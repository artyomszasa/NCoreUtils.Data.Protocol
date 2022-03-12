using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class ServiceFunctionDescriptorResolverDescriptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IFunctionDescriptorResolverDescriptor
    where T : class, IFunctionDescriptorResolver
{
    public IFunctionDescriptorResolver GetOrCreate(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<T>();
        if (service is IDisposable)
        {
            // disposing is handled by the DI container thus dispose method(s) should be hidden.
            return new SuppressDisposeFunctionDescriptorResolver<T>(service);
        }
        return service;
    }
}