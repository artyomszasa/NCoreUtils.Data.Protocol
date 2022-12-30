using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolPortableExtensions
{
    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        IPortableDataContext context,
        bool noCommonFunctions = false,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureServerFunctions = default,
        Action<CompositeFunctionMatcherBuilder>? configureClientFunctions = default)
        => services
            .AddCommonDataQueryServerServices(noCommonFunctions, configureServerFunctions)
            .AddCommonDataQueryClientServices(noCommonFunctions, configureClientFunctions)
            .AddSingleton<IDataUtils>(_ => new PortableDataUtils(context));

    public static IServiceCollection AddDataQueryServices(
        this IServiceCollection services,
        IPortableDataContext context,
        Action<CompositeFunctionDescriptorResolverBuilder>? configureServerFunctions,
        Action<CompositeFunctionMatcherBuilder>? configureClientFunctions)
        => services.AddDataQueryServices(context, false, configureServerFunctions, configureClientFunctions);
}