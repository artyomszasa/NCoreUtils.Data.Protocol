using System;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Data.Protocol.Internal;

public sealed class ServiceFunctionMatcherDescriptor<T> : IFunctionMatcherDescriptor
    where T : class, IFunctionMatcher
{
    public IFunctionMatcher GetOrCreate(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<T>();
        if (service is IDisposable)
        {
            // disposing is handled by the DI container thus dispose method(s) should be hidden.
            return new SuppressDisposeFunctionMatcher<T>(service);
        }
        return service;
    }
}