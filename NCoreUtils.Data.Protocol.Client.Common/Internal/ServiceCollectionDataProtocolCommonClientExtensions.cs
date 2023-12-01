using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data.Protocol.CommonClientFunctions;

namespace NCoreUtils.Data.Protocol.Internal;

public static class ServiceCollectionDataProtocolCommonClientExtensions
{
    public static IServiceCollection AddCommonDataQueryClientServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions = default)
    {
        var builder = new CompositeFunctionMatcherBuilder(services);
        if (!noCommonFunctions)
        {
            builder
                .AddSingleton(static _ => new StringLength())
                .AddSingleton(static _ => new StringContains())
                .AddSingleton(static _ => new StringToLower())
                .AddSingleton(static _ => new StringToUpper())
                .AddSingleton(static _ => new CollectionContains())
                .AddSingleton(static _ => new CollectionAny())
                .AddSingleton(static _ => new CollectionAll())
                .AddSingleton(static _ => new ArrayOf())
                .AddSingleton(static _ => new DateTimeOffsetFun());
        }
        configureFunctions?.Invoke(builder);
        services.TryAddScoped<ExpressionParser>();
        services.AddScoped<IFunctionMatcher>(serviceProvider => new CompositeFunctionMatcher(
            serviceProvider.GetServices<IFunctionMatcherWrapper>()
        ));
        return services;
    }

    public static IServiceCollection AddCommonDataQueryClientServices(
        this IServiceCollection services,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions)
        => services.AddCommonDataQueryClientServices(false, configureFunctions);
}