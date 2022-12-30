using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolReflectionClientExtensions
{
    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions = default)
        => services
            .AddCommonDataQueryClientServices(noCommonFunctions, configureFunctions)
            .AddSingleton<IDataUtils, ReflectionDataUtils>();

    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions)
        => services.AddDataQueryClientServices(false, configureFunctions);
}