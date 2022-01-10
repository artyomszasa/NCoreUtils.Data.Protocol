using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.CommonFunctions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolLinqExtensions
{
    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions = default)
    {
        var builder = new CompositeFunctionMatcherBuilder(services);
        if (!noCommonFunctions)
        {
            builder
                .AddAsSingletonService<StringLength>()
                .AddAsSingletonService<StringContains>()
                .AddAsSingletonService<StringToLower>()
                .AddAsSingletonService<StringToUpper>()
                .AddAsSingletonService<CollectionContains>()
                .AddAsSingletonService<CollectionAny>()
                .AddAsSingletonService<CollectionAll>()
                .AddAsSingletonService<ArrayOf>()
                .AddAsSingletonService<DateTimeOffsetFun>();
        }
        configureFunctions?.Invoke(builder);
        services.TryAddScoped<ExpressionParser>();
        services.AddScoped<IFunctionMatcher>(serviceProvider =>
        {
            return new CompositeFunctionMatcher(serviceProvider, builder.FunctionMatchers.ToArray());
        });
        return services;
    }

    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions)
        => services.AddDataQueryClientServices(false, configureFunctions);
}