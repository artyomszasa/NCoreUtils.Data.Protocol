using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data.Protocol.CommonServerFunctions;

namespace NCoreUtils.Data.Protocol.Internal;

public static class ServiceCollectionDataProtocolCommonServerExtensions
{
    /// <summary>
    /// Adds data query parsing services optionally configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="noCommonFunctions">Whether to suppress adding default function recognition.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddCommonDataQueryServerServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureFunctions = default)
    {
        var builder = new CompositeFunctionDescriptorResolverBuilder(services);
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
        services.TryAddScoped<IDataQueryExpressionBuilder, DefaultDataQueryExpressionBuilder>();
        services.TryAddSingleton<IDataQueryParser, DefaultDataQueryParser>();
        services.TryAddScoped<ITypeInferrer, DefaultTypeInferer>();
        services.AddScoped<IFunctionDescriptorResolver>(serviceProvider => new CompositeFunctionDescriptorResolver(
            serviceProvider.GetServices<IFunctionDescriptorResolverWrapper>()
        ));
        return services;
    }

    /// <summary>
    /// Adds data query parsing services with default function recognizers optionally configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddCommonDataQueryServerServices(
        this IServiceCollection services,
        Action<CompositeFunctionDescriptorResolverBuilder> configureFunctions)
        => services.AddCommonDataQueryServerServices(false, configureFunctions);
}