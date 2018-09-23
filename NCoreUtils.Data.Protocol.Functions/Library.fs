namespace NCoreUtils.Data.Protocol

open NCoreUtils
open NCoreUtils.Data.Protocol.TypeInference
open System.Collections.Immutable
open System.Reflection
open System.Linq.Expressions
open System
open System.Linq

[<RequireQualifiedAccess>]
module CommonFunctions =

  type StringLength () =
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      let pLength = typeof<string>.GetProperty ("Length", BindingFlags.Instance ||| BindingFlags.Public)
      { new IFunctionDescriptor with
          member __.Name = "length"
          member __.ResultType = typeof<int>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Property (args.[0], pLength) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI "length" when args.Count = 1 ->
          match args.[0] with
          | KnownType ty when ty = typeof<string> -> desc
          | UnknownType _ -> desc
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  type StringToLower () =
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      let mToLower =
        typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public)
        |> Seq.filter (fun m -> m.Name = "ToLower" && m.GetParameters().Length = 0)
        |> Seq.head
      { new IFunctionDescriptor with
          member __.Name = "lower"
          member __.ResultType = typeof<string>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mToLower) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI "lower" when args.Count = 1 ->
          match args.[0] with
          | KnownType ty when ty = typeof<string> -> desc
          | UnknownType _ -> desc
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  type StringToUpper () =
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      let mToLower =
        typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public)
        |> Seq.filter (fun m -> m.Name = "ToUpper" && m.GetParameters().Length = 0)
        |> Seq.head
      { new IFunctionDescriptor with
          member __.Name = "upper"
          member __.ResultType = typeof<string>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mToLower) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI "upper" when args.Count = 1 ->
          match args.[0] with
          | KnownType ty when ty = typeof<string> -> desc
          | UnknownType _ -> desc
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  type StringContains () =
    static let desc =
      let args = ImmutableArray.Create (typeof<string>, typeof<string>)
      let mContains =
        typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public)
        |> Seq.filter (fun m -> m.Name = "Contains" && m.GetParameters().Length = 1)
        |> Seq.head
      { new IFunctionDescriptor with
          member __.Name = "contains"
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mContains, args.[1]) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI "contains" when args.Count = 2 ->
          match args.[0] with
          | KnownType ty when ty = typeof<string> -> desc
          | UnknownType _ -> desc
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

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

type FunctionDescriptorResolverBuilder () =
  let resolvers = ResizeArray ()
  member __.Resolvers = resolvers
  member this.Add<'TResolver when 'TResolver :> IFunctionDescriptorResolver> () =
    resolvers.Add typeof<'TResolver>
    this
  member this.Insert<'TResolver when 'TResolver :> IFunctionDescriptorResolver> index =
    resolvers.Insert (index, typeof<'TResolver>)
    this
  member __.Build (serviceProvider : IServiceProvider) =
    let ress = resolvers.ToImmutableArray ()
    match ress.Length with
    | 0 -> { new IFunctionDescriptorResolver with member __.ResolveFunction (_, _, _, next) = next.Invoke () }
    | 1 -> FunctionDescriptorResolverSingleton (serviceProvider, ress.[0]) :> _
    | _ -> FunctionDescriptorResolverChain (serviceProvider, ress) :> _
