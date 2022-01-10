using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Data.Protocol.Internal;

public class CompositeFunctionMatcherBuilder
{
    public List<IFunctionMatcherDescriptor> FunctionMatchers { get; } = new List<IFunctionMatcherDescriptor>();

    public IServiceCollection Services { get; }

    public CompositeFunctionMatcherBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    public CompositeFunctionMatcherBuilder AddRegisteredService<T>()
        where T : class, IFunctionMatcher
    {
        FunctionMatchers.Add(new ServiceFunctionMatcherDescriptor<T>());
        return this;
    }

    public CompositeFunctionMatcherBuilder AddAsService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where T : class, IFunctionMatcher
    {
        Services.Add(ServiceDescriptor.Describe(typeof(T), typeof(T), serviceLifetime));
        return AddRegisteredService<T>();
    }

    public CompositeFunctionMatcherBuilder AddAsScopedService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionMatcher
        => AddAsService<T>(ServiceLifetime.Scoped);

    public CompositeFunctionMatcherBuilder AddAsSingletonService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionMatcher
        => AddAsService<T>(ServiceLifetime.Singleton);

    public CompositeFunctionMatcherBuilder Add(IFunctionMatcher instance)
    {
        FunctionMatchers.Add(new SingletonFunctionMatcherDescriptor(instance));
        return this;
    }

    public CompositeFunctionMatcherBuilder Add(Func<IServiceProvider, IFunctionMatcher> factory)
    {
        FunctionMatchers.Add(new FactoryFunctionMatcherDescriptor(factory));
        return this;
    }

    private sealed class ServiceActivator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>
        where T : class, IFunctionMatcher
    {
        public static IFunctionMatcher ActivateService(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<T>(serviceProvider);
    }

    public CompositeFunctionMatcherBuilder Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IFunctionMatcher
        => Add(ServiceActivator<T>.ActivateService);
}