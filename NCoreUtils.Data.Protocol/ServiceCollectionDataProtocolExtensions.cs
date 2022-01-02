using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.CommonFunctions;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolExtensions
{
    /// <summary>
    /// Adds data query parsing services optionally configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="noCommonFunctions">Whether to suppress adding default function recognition.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureFunctions = default)
    {
        var builder = new CompositeFunctionDescriptorResolverBuilder(services);
        if (!noCommonFunctions)
        {
            builder
                .AddAsSingletonService<StringLength>()
                .AddAsSingletonService<StringConatins>()
                .AddAsSingletonService<StringToLower>()
                .AddAsSingletonService<StringToUpper>()
                .AddAsSingletonService<CollectionContains>()
                .AddAsSingletonService<CollectionAny>()
                .AddAsSingletonService<CollectionAll>()
                .AddAsSingletonService<ArrayOf>()
                .AddAsSingletonService<DateTimeOffsetFun>();
        }
        configureFunctions?.Invoke(builder);
        services.TryAddScoped<IDataQueryExpressionBuilder, DefaultDataQueryExpressionBuilder>();
        services.TryAddSingleton<IDataQueryParser, DefaultDataQueryParser>();
        services.TryAddScoped<ITypeInferrer, DefaultTypeInferer>();
        services.AddScoped<IFunctionDescriptorResolver>(serviceProvider =>
        {
            return new CompositeFunctionDescriptorResolver(serviceProvider, builder.FunctionDescriptorResolvers.ToArray());
        });
        return services;
    }

    /// <summary>
    /// Adds data query parsing services with default function recognizers optionally configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        Action<CompositeFunctionDescriptorResolverBuilder> configureFunctions)
        => services.AddDataQueryServices(false, configureFunctions);
}