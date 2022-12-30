using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data.Protocol.CommonFunctions;

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
                .AddSingleton(_ => new StringLength())
                .AddSingleton(_ => new StringContains())
                .AddSingleton(_ => new StringToLower())
                .AddSingleton(_ => new StringToUpper())
                .AddSingleton(_ => new CollectionContains())
                .AddSingleton(_ => new CollectionAny())
                .AddSingleton(_ => new CollectionAll())
                .AddSingleton(_ => new ArrayOf())
                .AddSingleton(_ => new DateTimeOffsetFun());
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