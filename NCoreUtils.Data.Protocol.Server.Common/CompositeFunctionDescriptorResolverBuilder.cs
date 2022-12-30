using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public class CompositeFunctionDescriptorResolverBuilder
{
    public IServiceCollection Services { get; }

    public CompositeFunctionDescriptorResolverBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    public CompositeFunctionDescriptorResolverBuilder AddRegisteredService<T>()
        where T : IFunctionDescriptorResolver
        => AddTransient(serviceProvider => serviceProvider.GetRequiredService<T>());

    public CompositeFunctionDescriptorResolverBuilder Add(ServiceLifetime lifetime, Func<IServiceProvider, IFunctionDescriptorResolver> factory)
    {
        Services.Add(ServiceDescriptor.Describe(typeof(IFunctionDescriptorResolverWrapper), Factory, lifetime));
        return this;

        IFunctionDescriptorResolverWrapper Factory(IServiceProvider serviceProvider)
            => new FunctionDescriptorResolverWrapper(factory(serviceProvider));
    }

    public CompositeFunctionDescriptorResolverBuilder AddSingleton(Func<IServiceProvider, IFunctionDescriptorResolver> factory)
        => Add(ServiceLifetime.Singleton, factory);

    public CompositeFunctionDescriptorResolverBuilder AddScoped(Func<IServiceProvider, IFunctionDescriptorResolver> factory)
        => Add(ServiceLifetime.Scoped, factory);

    public CompositeFunctionDescriptorResolverBuilder AddTransient(Func<IServiceProvider, IFunctionDescriptorResolver> factory)
        => Add(ServiceLifetime.Transient, factory);
}