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
  static member AddDataQueryServices (services : IServiceCollection, [<OptionalArgument>] suppressDefaultCalls : bool, [<OptionalArgument>] configureCalls : Action<FunctionDescriptorResolverBuilder>) =
    let cirBuilder = FunctionDescriptorResolverBuilder ()
    if not suppressDefaultCalls then
      services
        .AddSingleton<CommonFunctions.StringLength>()
        .AddSingleton<CommonFunctions.StringToLower>()
        .AddSingleton<CommonFunctions.StringToUpper>()
        .AddSingleton<CommonFunctions.StringContains>()
        |> ignore
      cirBuilder
        .Add<CommonFunctions.StringLength>()
        .Add<CommonFunctions.StringToLower>()
        .Add<CommonFunctions.StringToUpper>()
        .Add<CommonFunctions.StringContains>()
        |> ignore
    if not (isNull configureCalls) then
      configureCalls.Invoke cirBuilder

    services.TryAddSingleton<IDataQueryExpressionBuilder, DataQueryExpressionBuilder>()
    services.TryAddSingleton<IDataQueryParser, DataQueryParser>()
    services.TryAddSingleton<ITypeInferrer, TypeInferrer>()

    services.AddScoped<IFunctionDescriptorResolver>(fun serviceProvider -> cirBuilder.Build (serviceProvider))

  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member AddDataQueryServices (services : IServiceCollection, configureCalls : Action<FunctionDescriptorResolverBuilder>) =
    ServiceCollectionDataQueryExtensions.AddDataQueryServices (services, false, configureCalls)

