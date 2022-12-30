using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data.Protocol;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils;

public static class ServiceCollectionDataProtocolPortableClientExtensions
{
    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        IPortableDataContext context,
        bool noCommonFunctions = false,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions = default)
        => services
            .AddCommonDataQueryClientServices(noCommonFunctions, configureFunctions)
            .AddSingleton<IDataUtils>(_ => new PortableDataUtils(context));

    public static IServiceCollection AddDataQueryClientServices(
        this IServiceCollection services,
        IPortableDataContext context,
        Action<CompositeFunctionMatcherBuilder>? configureFunctions)
        => services.AddDataQueryClientServices(context, false, configureFunctions);
}