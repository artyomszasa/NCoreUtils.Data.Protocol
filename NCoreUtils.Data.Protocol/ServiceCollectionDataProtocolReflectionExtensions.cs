using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolReflectionExtensions
{
    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureServerFunctions = default,
        Action<CompositeFunctionMatcherBuilder>? configureClientFunctions = default)
        => services
            .AddCommonDataQueryServerServices(noCommonFunctions, configureServerFunctions)
            .AddCommonDataQueryClientServices(noCommonFunctions, configureClientFunctions)
            .AddSingleton<IDataUtils, ReflectionDataUtils>();

    [RequiresUnreferencedCode(S.ReflectionTrimWarning)]
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureServerFunctions,
        Action<CompositeFunctionMatcherBuilder>? configureClientFunctions)
        => services.AddDataQueryServices(false, configureServerFunctions, configureClientFunctions);
}