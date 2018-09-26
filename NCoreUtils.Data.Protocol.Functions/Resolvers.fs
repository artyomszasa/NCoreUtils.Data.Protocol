namespace NCoreUtils.Data.Protocol

open NCoreUtils
open System.Collections.Immutable
open System

[<Sealed>]
type private FunctionDescriptorResolverChain (serviceProvider : IServiceProvider, resolvers : ImmutableArray<Type>) =
  interface IFunctionDescriptorResolver with
    member __.ResolveFunction (name, res, args, next) =
      let rec find i =
        match i = resolvers.Length with
        | true -> next.Invoke ()
        | _    ->
          let resolverType = resolvers.[i]
          use service = serviceProvider.GetOrActivateService resolverType
          let resolver = service.BoxedService :?> IFunctionDescriptorResolver
          resolver.ResolveFunction (name, res, args, fun () -> find (i + 1))
      find 0

[<Sealed>]
type private FunctionDescriptorResolverSingleton (serviceProvider : IServiceProvider, resolverType : Type) =
  interface IFunctionDescriptorResolver with
    member __.ResolveFunction (name, res, args, next) =
      use service = serviceProvider.GetOrActivateService resolverType
      let resolver = service.BoxedService :?> IFunctionDescriptorResolver
      resolver.ResolveFunction (name, res, args, next)

/// Provides function resolvation configuration possibility for data query expression processing.
type FunctionDescriptorResolverBuilder =
  val private resolvers : ResizeArray<Type>
  /// Gets internal collection of the function resolver types.
  member this.Resolvers = this.resolvers
  /// Initializes new instance of builder.
  new () = { resolvers = ResizeArray () }
  /// <summary>
  /// Appends function resolver to function resolver collection.
  /// </summary>
  /// <typeparam name="TResolver">Function resolver type to add.</typeparam>
  /// <returns>Original builder for chaining.</returns>
  member this.Add<'TResolver when 'TResolver :> IFunctionDescriptorResolver> () =
    this.resolvers.Add typeof<'TResolver>
    this
  /// <summary>
  /// Inserts function resolver to function resolver collection at the specified index.
  /// </summary>
  /// <typeparam name="TResolver">Function resolver type to add.</typeparam>
  /// <param name="index">Index to insert function resolver at.</param>
  /// <returns>Original builder for chaining.</returns>
  member this.Insert<'TResolver when 'TResolver :> IFunctionDescriptorResolver> index =
    this.resolvers.Insert (index, typeof<'TResolver>)
    this
  /// <summary>
  /// Builds function resolver for the DI context defined by the specified service provider.
  /// </summary>
  /// <param name="serviceProvider">Service provider to use.</param>
  /// <returns>Function resolver.</returns>
  member this.Build (serviceProvider : IServiceProvider) =
    let ress = this.resolvers.ToImmutableArray ()
    match ress.Length with
    | 0 -> { new IFunctionDescriptorResolver with member __.ResolveFunction (_, _, _, next) = next.Invoke () }
    | 1 -> FunctionDescriptorResolverSingleton (serviceProvider, ress.[0]) :> _
    | _ -> FunctionDescriptorResolverChain (serviceProvider, ress) :> _
