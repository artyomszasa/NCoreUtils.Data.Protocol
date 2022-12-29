using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionReflectionDataProtocolUtilsExtensions
{
    /// <summary>
    /// Adds data query parsing services that relies on reflection optionally configuring supported function
    /// recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="noCommonFunctions">Whether to suppress adding default function recognition.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureFunctions = default)
        => Internal.ServiceCollectionDataProtocolExtensions.AddDataQueryServices(
            services,
            noCommonFunctions,
            configureFunctions
        ).AddSingleton<IDataUtils, ReflectionDataUtils>();

    /// <summary>
    /// Adds data query parsing services that relies on reflection with default function recognizers optionally
    /// configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        Action<CompositeFunctionDescriptorResolverBuilder> configureFunctions)
        => ServiceCollectionReflectionDataProtocolUtilsExtensions.AddDataQueryServices(services, false, configureFunctions);
}