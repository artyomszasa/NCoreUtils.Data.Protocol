namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Data.Protocol.TypeInference

/// Contains common function resolvers.
[<RequireQualifiedAccess>]
module CommonFunctions =

  /// String.Length operation resolver.
  [<Sealed>]
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
          | UnknownType cs ->
            match TypeConstraints.``match`` cs typeof<string> with
            | ValueNone -> desc
            | _         -> next.Invoke ()
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  /// String.ToLower operation resolver.
  [<Sealed>]
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
          | UnknownType cs ->
            match TypeConstraints.``match`` cs typeof<string> with
            | ValueNone -> desc
            | _         -> next.Invoke ()
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  /// String.ToUpper operation resolver.
  [<Sealed>]
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
          | UnknownType cs ->
            match TypeConstraints.``match`` cs typeof<string> with
            | ValueNone -> desc
            | _         -> next.Invoke ()
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  /// String.Contains operation resolver.
  [<Sealed>]
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
          | UnknownType cs ->
            match TypeConstraints.``match`` cs typeof<string> with
            | ValueNone -> desc
            | _         -> next.Invoke ()
          | _ -> next.Invoke ()
        | _ -> next.Invoke ()

  /// Enumerable.Contains operation resolver.
  [<Sealed>]
  type CollectionContains () =
    static let gDescription = typeof<CollectionContains>.GetMethod("Description", BindingFlags.NonPublic ||| BindingFlags.Static)
    static let gContains =
      typeof<Enumerable>.GetMethods (BindingFlags.Static ||| BindingFlags.Public)
      |> Seq.filter (fun m -> m.Name = "Contains" && m.GetParameters().Length = 2)
      |> Seq.head
    static member private Description<'a> () =
      let args = ImmutableArray.Create (typeof<seq<'a>>, typeof<'a>)
      let mContains = gContains.MakeGenericMethod typeof<'a>
      { new IFunctionDescriptor with
          member __.Name = "contains"
          member __.ResultType = typeof<bool>
          member __.ArgumentTypes = args :> _
          member __.CreateExpression args = Expression.Call (mContains, args) :> _
      }
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member private GetElementType (tys : Type[]) =
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
    static member private GetElementType (ty : Type) =
      match ty.IsGenericType && ty.GetGenericTypeDefinition () = typedefof<IEnumerable<_>> with
      | true -> ValueSome <| ty.GetGenericArguments().[0]
      | _    -> CollectionContains.GetElementType (ty.GetInterfaces ())

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member private GetElementType (c : TypeConstraints) =
      match c.Base with
      | null     -> ValueNone
      | baseType -> CollectionContains.GetElementType baseType

    interface IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, args, next) =
        match name with
        | EQI "contains" when args.Count = 2 ->
          match args.[0] with
          | KnownType ty ->
            match CollectionContains.GetElementType ty with
            | ValueSome elementType ->
              gDescription.MakeGenericMethod(elementType).Invoke(null, [| |]) :?> _
            | _ -> next.Invoke ()
          | UnknownType c ->
            let elementType =
              match CollectionContains.GetElementType c with
              | ValueSome _ as result -> result
              | _ ->
                match args.[1] with
                | KnownType ty -> ValueSome ty
                | _            -> ValueNone
            match elementType with
            | ValueSome elementType ->
              gDescription.MakeGenericMethod(elementType).Invoke(null, [| |]) :?> _
            | _ -> next.Invoke ()
        | _ -> next.Invoke ()
