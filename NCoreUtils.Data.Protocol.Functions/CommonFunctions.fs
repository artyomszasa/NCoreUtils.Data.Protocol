namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections.Immutable
open System.Diagnostics.CodeAnalysis
open System.Linq
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Data.Protocol.TypeInference

/// Contains common function resolvers.
[<RequireQualifiedAccess>]
module CommonFunctions =

  module private Names =

    [<Literal>]
    let Length = "length"

    [<Literal>]
    let Lower = "lower"

    [<Literal>]
    let Upper = "upper"

    [<Literal>]
    let StartsWith = "startsWith"

    [<Literal>]
    let EndsWith = "endsWith"

    [<Literal>]
    let Contains = "contains"

    [<Literal>]
    let Includes = "includes"

    [<Literal>]
    let Some = "some"

    [<Literal>]
    let Every = "every"

  [<ExcludeFromCodeCoverage>]
  let inline private eqi a b = StringComparer.OrdinalIgnoreCase.Equals (a, b)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<CompiledName("IsStringCompatible")>]
  let private isStringCompatible ``constraint`` =
    match ``constraint`` with
    | KnownType ty when ty = typeof<string> -> true
    | UnknownType cs ->
      match TypeConstraints.``match`` cs typeof<string> with
      | ValueNone -> true
      | _         -> false
    | _ -> false

  type private Exprs =

    static member GetMethod (expr : Expression<Func<_, _>>) =
      match expr.Body with
      | :? MethodCallExpression as mexpr -> mexpr.Method
      | _ -> failwithf "invalid expression provided"

    static member GetMethod (expr : Expression<Func<_, _, _>>) =
      match expr.Body with
      | :? MethodCallExpression as mexpr -> mexpr.Method
      | _ -> failwithf "invalid expression provided"

  /// String.Length operation resolver.
  [<Sealed>]
  type StringLength () =
    static let pLength = typeof<string>.GetProperty ("Length", BindingFlags.Instance ||| BindingFlags.Public)
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      { new IFunctionDescriptor with
          member __.Name = Names.Length
          member __.ResultType = typeof<int>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Property (args.[0], pLength) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match eqi Names.Length name && 1 = args.Count with
        | true when isStringCompatible args.[0] -> desc
        | _                                     -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MemberExpression as mexpr when mexpr.Member.Equals pLength ->
          ValueSome { Name = Names.Length; Arguments = [| mexpr.Expression |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()


  /// String.ToLower operation resolver.
  [<Sealed>]
  type StringToLower () =
    static let mToLower = Exprs.GetMethod (fun (s : string) -> s.ToLower())
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      { new IFunctionDescriptor with
          member __.Name = Names.Lower
          member __.ResultType = typeof<string>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mToLower) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI Names.Lower when args.Count = 1 && isStringCompatible args.[0] -> desc
        | _                                                                  -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.Equals mToLower ->
          ValueSome { Name = Names.Lower; Arguments = [| mexpr.Object |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()


  /// String.ToUpper operation resolver.
  [<Sealed>]
  type StringToUpper () =
    static let mToUpper = Exprs.GetMethod (fun (s : string) -> s.ToUpper())
    static let desc =
      let args = ImmutableArray.Create typeof<string>
      { new IFunctionDescriptor with
          member __.Name = Names.Upper
          member __.ResultType = typeof<string>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mToUpper) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI Names.Upper when args.Count = 1 && isStringCompatible args.[0] -> desc
        | _                                                                  -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.Equals mToUpper ->
          ValueSome { Name = Names.Upper; Arguments = [| mexpr.Object |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()

  /// String.StartsWith operation resolver.
  [<Sealed>]
  type StringStartsWith () =
    static let mStartsWith = Exprs.GetMethod (fun (source : string) (seed: string) -> source.StartsWith(seed))
    static let desc =
      let args = ImmutableArray.Create (typeof<string>, typeof<string>)
      { new IFunctionDescriptor with
          member __.Name = Names.StartsWith
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mStartsWith, args.[1]) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI Names.StartsWith when args.Count = 2 && isStringCompatible args.[0] && isStringCompatible args.[1] -> desc
        | _ -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.Equals mStartsWith && 1 = mexpr.Arguments.Count ->
          ValueSome { Name = Names.StartsWith; Arguments = [| mexpr.Object; mexpr.Arguments.[0] |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()

  /// String.EndsWith operation resolver.
  [<Sealed>]
  type StringEndsWith () =
    static let mEndsWith = Exprs.GetMethod (fun (source : string) (seed: string) -> source.EndsWith(seed))
    static let desc =
      let args = ImmutableArray.Create (typeof<string>, typeof<string>)
      { new IFunctionDescriptor with
          member __.Name = Names.EndsWith
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mEndsWith, args.[1]) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI Names.EndsWith when args.Count = 2 && isStringCompatible args.[0] && isStringCompatible args.[1] -> desc
        | _ -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.Equals mEndsWith && 1 = mexpr.Arguments.Count ->
          ValueSome { Name = Names.EndsWith; Arguments = [| mexpr.Object; mexpr.Arguments.[0] |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()

  /// String.Contains operation resolver.
  [<Sealed>]
  type StringContains () =
    static let mContains = Exprs.GetMethod (fun (s0 : string) (s1 : string) -> s0.Contains(s1))
    static let desc =
      let args = ImmutableArray.Create (typeof<string>, typeof<string>)
      { new IFunctionDescriptor with
          member __.Name = Names.Contains
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (args.[0], mContains, args.[1]) :> _
      }
    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI Names.Contains when args.Count = 2 ->
          match args.[0] with
          | KnownType ty when ty = typeof<string> -> desc
          | UnknownType cs ->
            match TypeConstraints.``match`` cs typeof<string> with
            | ValueNone -> desc
            | _         -> next.Invoke ()
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.Equals mContains ->
          ValueSome { Name = Names.Contains; Arguments = [| mexpr.Object; mexpr.Arguments.[0] |] :> IReadOnlyList<_> }
        | _ -> next.Invoke ()


  type private Helpers =

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElementType (tys : Type[]) =
      let rec impl i =
        match i < tys.Length with
        | false -> ValueNone
        | true ->
          let ty = tys.[i]
          match ty.IsGenericType && ty.GetGenericTypeDefinition () = typedefof<IEnumerable<_>> with
          | true -> ValueSome <| ty.GetGenericArguments().[0]
          | _    -> impl (i + 1)
      impl 0

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElementType (ty : Type) =
      match ty.IsGenericType && ty.GetGenericTypeDefinition () = typedefof<IEnumerable<_>> with
      | true -> ValueSome <| ty.GetGenericArguments().[0]
      | _    -> Helpers.GetElementType (ty.GetInterfaces ())

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElementType (c : TypeConstraints) =
      match c.Base with
      | null     -> ValueNone
      | baseType -> Helpers.GetElementType baseType

  /// Enumerable.Contains operation resolver.
  [<Sealed>]
  type CollectionContains () =
    static let gDescription = typeof<CollectionContains>.GetMethod("Description", BindingFlags.NonPublic ||| BindingFlags.Static)

    static let gContains = Exprs.GetMethod(fun (col : seq<int>) -> col.Contains 2).GetGenericMethodDefinition ()

    static let cache = ConcurrentDictionary<Type, IFunctionDescriptor>()

    static let factory =
      Func<Type, IFunctionDescriptor> (fun ty -> gDescription.MakeGenericMethod(ty).Invoke(null, [| |]) :?> _)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static let getFunctionDescriptorFor ty = cache.GetOrAdd (ty, factory)

    static member private Description<'a> () =
      let args = ImmutableArray.Create (typeof<seq<'a>>, typeof<'a>)
      let mContains = gContains.MakeGenericMethod typeof<'a>
      { new IFunctionDescriptor with
          member __.Name = Names.Includes
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (mContains, args) :> _
      }

    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | (EQI Names.Contains | EQI Names.Includes) when args.Count = 2 ->
          match args.[0] with
          | KnownType ty ->
            match Helpers.GetElementType ty with
            | ValueSome elementType -> getFunctionDescriptorFor elementType
            | _ -> next.Invoke ()
          | UnknownType c ->
            let elementType =
              match Helpers.GetElementType c with
              | ValueSome _ as result -> result
              | _ ->
                match args.[1] with
                | KnownType ty -> ValueSome ty
                | _            -> ValueNone
            match elementType with
            | ValueSome elementType -> getFunctionDescriptorFor elementType
            | _ -> next.Invoke ()
        | _ -> next.Invoke ()
    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.IsGenericMethod && mexpr.Method.GetGenericMethodDefinition () = gContains ->
          ValueSome { Name = Names.Contains; Arguments = mexpr.Arguments }
        | _ -> next.Invoke ()


  [<AbstractClass>]
  type CollectionOperationWithLambda () =
    abstract MatchName : name:string -> bool
    abstract GetFunctionDescriptorFor : ``type``:Type -> IFunctionDescriptor

    interface IFunctionDescriptorResolver with
      member this.ResolveFunction (name, _, args, next) =
        match this.MatchName name with
        | true when args.Count = 2 ->
          match args.[0] with
          | KnownType ty ->
            match Helpers.GetElementType ty with
            | ValueSome elementType -> this.GetFunctionDescriptorFor elementType
            | _ -> next.Invoke ()
          | UnknownType c ->
            let elementType =
              match Helpers.GetElementType c with
              | ValueSome _ as result -> result
              | _ ->
                match args.[1] with
                | KnownType ty when ty.IsConstructedGenericType && ty.GetGenericTypeDefinition() = typedefof<System.Func<_, _>> -> ValueSome <| ty.GetGenericArguments().[0]
                | _            -> ValueNone
            match elementType with
            | ValueSome elementType -> this.GetFunctionDescriptorFor elementType
            | _ -> next.Invoke ()
        | _ -> next.Invoke ()


  /// Enumerable.Any operation resolver.
  [<Sealed>]
  type CollectionAny () =
    inherit CollectionOperationWithLambda ()

    static let gDescription = typeof<CollectionAny>.GetMethod("Description", BindingFlags.NonPublic ||| BindingFlags.Static)

    static let gAny = Exprs.GetMethod(fun (col : seq<int>) -> col.Any (fun i -> i = 2)).GetGenericMethodDefinition ()

    static let cache = ConcurrentDictionary<Type, IFunctionDescriptor>()

    static let factory =
      Func<Type, IFunctionDescriptor> (fun ty -> gDescription.MakeGenericMethod(ty).Invoke(null, [| |]) :?> _)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static let getFunctionDescriptorFor ty = cache.GetOrAdd (ty, factory)

    static member private Description<'a> () =
      let args = ImmutableArray.Create (typeof<seq<'a>>, typeof<Func<'a, bool>>)
      let mAny = gAny.MakeGenericMethod typeof<'a>
      { new IFunctionDescriptor with
          member __.Name = Names.Some
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (mAny, args) :> _
      }

    override __.MatchName name = eqi Names.Some name
    override __.GetFunctionDescriptorFor ty = getFunctionDescriptorFor ty

    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.IsGenericMethod && mexpr.Method.GetGenericMethodDefinition () = gAny ->
          ValueSome { Name = Names.Some; Arguments = mexpr.Arguments }
        | _ -> next.Invoke ()


  /// Enumerable.All operation resolver.
  [<Sealed>]
  type CollectionAll () =
    inherit CollectionOperationWithLambda ()

    static let gDescription = typeof<CollectionAll>.GetMethod("Description", BindingFlags.NonPublic ||| BindingFlags.Static)

    static let gAll = Exprs.GetMethod(fun (col : seq<int>) -> col.All (fun i -> i = 2)).GetGenericMethodDefinition ()

    static let cache = ConcurrentDictionary<Type, IFunctionDescriptor>()

    static let factory =
      Func<Type, IFunctionDescriptor> (fun ty -> gDescription.MakeGenericMethod(ty).Invoke(null, [| |]) :?> _)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static let getFunctionDescriptorFor ty = cache.GetOrAdd (ty, factory)

    static member private Description<'a> () =
      let args = ImmutableArray.Create (typeof<seq<'a>>, typeof<Func<'a, bool>>)
      let mAll = gAll.MakeGenericMethod typeof<'a>
      { new IFunctionDescriptor with
          member __.Name = Names.Every
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (mAll, args) :> _
      }

    override __.MatchName name = eqi Names.Every name
    override __.GetFunctionDescriptorFor ty = getFunctionDescriptorFor ty

    interface IFunctionMatcher with
      member __.MatchFunction (expression, next) =
        match expression with
        | :? MethodCallExpression as mexpr when mexpr.Method.IsGenericMethod && mexpr.Method.GetGenericMethodDefinition () = gAll ->
          ValueSome { Name = Names.Every; Arguments = mexpr.Arguments }
        | _ -> next.Invoke ()
