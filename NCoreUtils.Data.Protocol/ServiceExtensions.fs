namespace NCoreUtils.Data

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open NCoreUtils.Data.Protocol
open NCoreUtils.Data.Protocol.TypeInference
open System
open System.Runtime.CompilerServices

/// Contains service collection extensions to easily add data query parsing services.
[<Extension>]
[<AbstractClass; Sealed>]
type ServiceCollectionDataQueryExtensions =

  /// <summary>
  /// Adds data query parsing services optionally configuring supported function recognition.
  /// </summary>
  /// <param name="services">Service collection to operate on.</param>
  /// <param name="suppressDefaultCalls">Whether to suppress adding default function recognition.</param>
  /// <param name="configureFunctions">Optional function to configure function recognition.</param>
  /// <returns>Original service collection for chaining.</returns>
  [<Extension>]
  static member AddDataQueryServices (services : IServiceCollection, [<OptionalArgument>] suppressDefaultCalls : bool, [<OptionalArgument>] configureFunctions : Action<FunctionDescriptorResolverBuilder>) =
    let cirBuilder = FunctionDescriptorResolverBuilder ()
    if not suppressDefaultCalls then
      services
        .AddSingleton<CommonFunctions.StringLength>()
        .AddSingleton<CommonFunctions.StringToLower>()
        .AddSingleton<CommonFunctions.StringToUpper>()
        .AddSingleton<CommonFunctions.StringContains>()
        .AddSingleton<CommonFunctions.CollectionContains>()
        .AddSingleton<CommonFunctions.CollectionAny>()
        .AddSingleton<CommonFunctions.CollectionAll>()
        |> ignore
      cirBuilder
        .Add<CommonFunctions.StringLength>()
        .Add<CommonFunctions.StringToLower>()
        .Add<CommonFunctions.StringToUpper>()
        .Add<CommonFunctions.StringContains>()
        .Add<CommonFunctions.CollectionContains>()
        .Add<CommonFunctions.CollectionAny>()
        .Add<CommonFunctions.CollectionAll>()
        |> ignore
    if not (isNull configureFunctions) then
      configureFunctions.Invoke cirBuilder

    services.TryAddScoped<IDataQueryExpressionBuilder, DataQueryExpressionBuilder>()
    services.TryAddSingleton<IDataQueryParser, DataQueryParser>()
    services.TryAddScoped<ITypeInferrer, TypeInferrer>()

    services.AddScoped<IFunctionDescriptorResolver>(fun serviceProvider -> cirBuilder.Build (serviceProvider))

  /// <summary>
  /// Adds data query parsing services with default function recognizers optionally configuring supported function recognition.
  /// </summary>
  /// <param name="services">Service collection to operate on.</param>
  /// <param name="configureFunctions">Optional function to configure function recognition.</param>
  /// <returns>Original service collection for chaining.</returns>
  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member AddDataQueryServices (services : IServiceCollection, configureCalls : Action<FunctionDescriptorResolverBuilder>) =
    ServiceCollectionDataQueryExtensions.AddDataQueryServices (services, false, configureCalls)

