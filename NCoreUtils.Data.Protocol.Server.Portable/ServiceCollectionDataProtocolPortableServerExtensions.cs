using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolPortableServerExtensions
{
    /// <summary>
    /// Adds data query parsing services that relies on the generated portable context optionally configuring supported
    /// function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="context">Pregenerated context.</param>
    /// <param name="noCommonFunctions">Whether to suppress adding default function recognition.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServerServices(
        this IServiceCollection services,
        IPortableDataContext context,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureFunctions = default)
        => services
            .AddCommonDataQueryServerServices(noCommonFunctions, configureFunctions)
            .AddSingleton<IDataUtils>(_ => new PortableDataUtils(context));

    /// <summary>
    /// Adds data query parsing services that relies on the generated portable context with default functions optionally
    /// configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="context">Pregenerated context.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    public static IServiceCollection AddDataQueryServerServices(
        this IServiceCollection services,
        IPortableDataContext context,
        Action<CompositeFunctionDescriptorResolverBuilder> configureFunctions)
        => services.AddDataQueryServerServices(context, false, configureFunctions);
}