using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Data.Protocol.Internal;

public class CompositeFunctionDescriptorResolverBuilder
{
    public List<IFunctionDescriptorResolverDescriptor> FunctionDescriptorResolvers { get; } = new();

    public IServiceCollection Services { get; }

    public CompositeFunctionDescriptorResolverBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    public CompositeFunctionDescriptorResolverBuilder AddRegisteredService<T>()
        where T : class, IFunctionDescriptorResolver
    {
        FunctionDescriptorResolvers.Add(new ServiceFunctionDescriptorResolverDescriptor<T>());
        return this;
    }

    public CompositeFunctionDescriptorResolverBuilder AddAsService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : class, IFunctionDescriptorResolver
    {
        Services.Add(ServiceDescriptor.Describe(typeof(T), typeof(T), serviceLifetime));
        return AddRegisteredService<T>();
    }

    public CompositeFunctionDescriptorResolverBuilder AddAsScopedService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionDescriptorResolver
        => AddAsService<T>(ServiceLifetime.Scoped);

    public CompositeFunctionDescriptorResolverBuilder AddAsSingletonService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionDescriptorResolver
        => AddAsService<T>(ServiceLifetime.Singleton);

    public CompositeFunctionDescriptorResolverBuilder Add(IFunctionDescriptorResolver instance)
    {
        FunctionDescriptorResolvers.Add(new SingletonFunctionDescriptorResolverDescriptor(instance));
        return this;
    }

    public CompositeFunctionDescriptorResolverBuilder Add(Func<IServiceProvider, IFunctionDescriptorResolver> factory)
    {
        FunctionDescriptorResolvers.Add(new FactoryFunctionDescriptorResolverDescriptor(factory));
        return this;
    }

    private sealed class ServiceActivator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>
        where T : class, IFunctionDescriptorResolver
    {
        public static IFunctionDescriptorResolver ActivateService(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<T>(serviceProvider);
    }

    public CompositeFunctionDescriptorResolverBuilder Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionDescriptorResolver
        => Add(ServiceActivator<T>.ActivateService);
}