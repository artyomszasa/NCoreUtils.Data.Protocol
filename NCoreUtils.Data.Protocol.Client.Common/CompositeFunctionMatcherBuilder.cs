using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public sealed class CompositeFunctionMatcherBuilder
{
    public IServiceCollection Services { get; }

    public CompositeFunctionMatcherBuilder(IServiceCollection services)
        => Services = services ?? throw new ArgumentNullException(nameof(services));

    public CompositeFunctionMatcherBuilder AddRegisteredService<T>()
        where T : IFunctionMatcher
        => AddTransient(serviceProvider => serviceProvider.GetRequiredService<T>());

    public CompositeFunctionMatcherBuilder Add(ServiceLifetime lifetime, Func<IServiceProvider, IFunctionMatcher> factory)
    {
        Services.Add(ServiceDescriptor.Describe(typeof(IFunctionMatcherWrapper), Factory, lifetime));
        return this;

        IFunctionMatcherWrapper Factory(IServiceProvider serviceProvider)
            => new FunctionMatcherWrapper(factory(serviceProvider));
    }

    public CompositeFunctionMatcherBuilder AddSingleton(Func<IServiceProvider, IFunctionMatcher> factory)
        => Add(ServiceLifetime.Singleton, factory);

    public CompositeFunctionMatcherBuilder AddScoped(Func<IServiceProvider, IFunctionMatcher> factory)
        => Add(ServiceLifetime.Scoped, factory);

    public CompositeFunctionMatcherBuilder AddTransient(Func<IServiceProvider, IFunctionMatcher> factory)
        => Add(ServiceLifetime.Transient, factory);
}