namespace NCoreUtils.Data

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open NCoreUtils.Data.Protocol
open NCoreUtils.Data.Protocol.TypeInference
open System
open System.Runtime.CompilerServices

[<Extension>]
type ServiceCollectionDataQueryExtensions =

  [<Extension>]
  static member AddDataQueryServices (services : IServiceCollection, [<OptionalArgument>] suppressDefaultCalls : bool, [<OptionalArgument>] configureCalls : Action<CallInfoResolverBuilder>) =
    let cirBuilder = CallInfoResolverBuilder ()
    if not suppressDefaultCalls then
      cirBuilder.AddRange DefaultCalls.all |> ignore
    if not (isNull configureCalls) then
      configureCalls.Invoke cirBuilder

    services.TryAddSingleton<IDataQueryExpressionBuilder, DataQueryExpressionBuilder>()
    services.TryAddSingleton<IDataQueryParser, DataQueryParser>()
    services.TryAddSingleton<ITypeInferer, TypeInferer>()

    services
      .AddSingleton(cirBuilder)
      .AddSingleton<ICallInfoResolver, CallInfoResolver>()

  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member AddDataQueryServices (services : IServiceCollection, configureCalls : Action<CallInfoResolverBuilder>) =
    ServiceCollectionDataQueryExtensions.AddDataQueryServices (services, false, configureCalls)

