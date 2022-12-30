using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolReflectionServerExtensions
{
    /// <summary>
    /// Adds data query parsing services that relies on reflection optionally configuring supported
    /// function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="context">Pregenerated context.</param>
    /// <param name="noCommonFunctions">Whether to suppress adding default function recognition.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryServerServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureFunctions = default)
        => services
            .AddCommonDataQueryServerServices(noCommonFunctions, configureFunctions)
            .AddSingleton<IDataUtils, ReflectionDataUtils>();

    /// <summary>
    /// Adds data query parsing services that relies on reflection with default functions optionally
    /// configuring supported function recognition.
    /// </summary>
    /// <param name="services">Service collection to operate on.</param>
    /// <param name="context">Pregenerated context.</param>
    /// <param name="configureFunctions">Optional function to configure function recognition.</param>
    /// <returns>Original service collection for chaining.</returns>
    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryServerServices(
        this IServiceCollection services,
        Action<CompositeFunctionDescriptorResolverBuilder> configureFunctions)
        => services.AddDataQueryServerServices(false, configureFunctions);
}