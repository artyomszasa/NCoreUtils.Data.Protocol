namespace NCoreUtils.Data.Protocol.TypeInference

open System
open System.Collections.Immutable
open System.Reflection
open System.Threading
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.Internal
open System.Runtime.CompilerServices

[<CustomEquality; NoComparison>]
[<Struct>]
type Ty =
  | TAny
  | TExact of Type:Type
  | TVar   of Uid:int64
  with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member internal Eq (a : Ty, b : Ty) =
      match a with
      | TAny ->
        match b with
        | TAny -> true
        | _    -> true
      | TExact aty ->
        match b with
        | TExact bty -> aty.Equals bty
        | _          -> false
      | TVar auid ->
        match b with
        | TVar buid -> auid = buid
        | _         -> false
    interface IEquatable<Ty> with
      member this.Equals that = Ty.Eq (this, that)
    override this.Equals obj =
      match obj with
      | null          -> false
      | :? Ty as that -> Ty.Eq (this, that)
      | _             -> false
    override this.GetHashCode () =
      match this with
      | TAny      -> 0
      | TExact ty -> 1 + (ty.GetHashCode  () <<< 1)
      | TVar  uid -> 0 + (uid.GetHashCode () <<< 1)



type IHasTypeVar<'a> =
  abstract Substitute : substitution:(Ty -> Ty) -> 'a

and [<Struct; CustomEquality; NoComparison>] Subs =
  | Subs of struct (Ty * Ty)
  with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member private Eq (a : Subs, b : Subs) =
      match a with
      | Subs (struct (ax, ay)) ->
      match b with
      | Subs (struct (bx, by)) ->
        Ty.Eq (ax, bx) && Ty.Eq (ay, by)
    interface IEquatable<Subs> with
      member this.Equals that = Subs.Eq (this, that)
    override this.Equals obj =
      match obj with
      | null            -> false
      | :? Subs as that -> Subs.Eq (this, that)
      | _               -> false
    override this.GetHashCode () =
      match this with
      | Subs struct (x, y) -> x.GetHashCode () ^^^ y.GetHashCode ()
    member this.Source =
      match this with
      | Subs struct (x, _) -> x
    member this.Target =
      match this with
      | Subs struct (_, x) -> x
    interface IHasTypeVar<Subs> with
      member this.Substitute substitution =
        match this with
        | Subs struct (source, target) ->
          Subs struct (source, substitution target)

type ISubstitutable<'a> =
  abstract Substitute : substitution:Subs -> 'a

[<AutoOpen>]
module Substitution =

  [<CompiledName("New")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let subs a b = Subs struct (a, b)

  [<CompiledName("Apply")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let apply substitution (varHolder : 'a when 'a :> IHasTypeVar<'a>) = varHolder.Substitute substitution

  [<CompiledName("Apply")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (<==) (target : 'a when 'a :> ISubstitutable<'a>) subs = target.Substitute subs

type [<NoEquality; NoComparison>] Subss =
 | Subss of Subs list
 with
  member this.TryResolveExact ty =
    match this with
    | Subss l ->
      let rec resolve ty =
        match ty with
        | TExact typ -> Some typ
        | TVar _ as v ->
          match l |> List.tryFind (fun (Subs struct (x, _)) -> x = v) with
          | Some (Subs struct (_, x)) -> resolve x
          | _                         -> None
        | _ -> None
      resolve ty

  interface IHasTypeVar<Subss> with
    member this.Substitute substitution =
      match this with
      | Subss list -> List.map (apply substitution) list |> Subss
  interface ISubstitutable<Subss> with
    member this.Substitute substitution =
      match this with
      | Subss list ->
        let f t0 =
          match t0 = substitution.Source with
          | true -> substitution.Target
          | _    -> t0
        let rec insert acc shouldAppend l =
          match l with
          | [] ->
            match shouldAppend with
            | true  -> substitution :: acc
            | false -> acc
          | (subs : Subs) :: subss ->
            match subs.Source = substitution.Source with
            | true ->
              match subs.Target, substitution.Target with
              | a, b when a = b  -> insert (subs :: acc) false subss
              | TAny, _          -> insert (substitution :: acc) false subss
              | _, TAny          -> insert (subs :: acc) false subss
              | TVar _ as a, b   -> insert (subs :: Subs struct (a, b) :: acc) shouldAppend subss
              | a, b             -> failwithf "Substitution conflict (%A ~ %A)" a b
            | _ -> insert (apply f subs :: acc) shouldAppend subss
        insert [] true list |> Subss

type ITypeInferenceContext =
  inherit ICallInfoResolver
  abstract RootType    : Type
  abstract NewVar      : unit -> Ty

type ITypeInferer =
  abstract InferTypes : ast:Node * rootType:Type -> NodeX<Type>

type internal Context = {
  RootType    : Type
  NewVar      : unit -> Ty
  ResolveCall : struct (string * int) -> ICallInfo }
  with
    interface ITypeInferenceContext with
      member this.RootType = this.RootType
      member this.NewVar () = this.NewVar ()
      member this.ResolveCall (name, argNum) = this.ResolveCall (struct (name, argNum))

type TypeInferer (callInfoResolver : ICallInfoResolver) =
  static let rec collect (ctx : ITypeInferenceContext) ty (subss : Subss) node =
    match node with
    | Constant value ->
      let current = ctx.NewVar ()
      struct (subss <== subs current ty, { Node = ConstantX value; Data = current })
    | Identifier value ->
      let property = ctx.RootType.GetProperty (value, BindingFlags.Instance ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Public ||| BindingFlags.IgnoreCase)
      if isNull property then failwithf "Type %A has no property named %A" ctx.RootType value
      let current = TExact property.PropertyType
      struct (subss <== subs ty current, { Node = IdentifierX value; Data = current })
    | Call (name, args) ->
      match ctx.ResolveCall (name, args.Length) with
      | null -> failwithf "Unsupported function %A" name
      | info ->
        if info.Parameters.Length <> args.Length then
          failwithf "Parameter count mismatch for %A" name
        let (struct (subss', args')) =
          args
          |> Seq.zip info.Parameters
          |> Seq.fold
            (fun (struct (ss, res)) (typ, arg) ->
              let v = ctx.NewVar ()
              let ss' = ss <== subs v (TExact typ)
              let (struct (ss'', arg')) = collect ctx v ss' arg
              struct (ss'', arg' :: res)
            )
            (struct (subss, []))
        let current = TExact info.ResultType
        struct (subss' <== subs ty current, { Node = CallX (name, args' |> List.rev |> ImmutableArray.CreateRange); Data = current })
    | Binary (left, op, right) ->
      match op with
      | BinaryOperation.AndAlso
      | BinaryOperation.OrElse ->
        let current = TExact typeof<bool>
        let vl = ctx.NewVar ()
        let vr = ctx.NewVar ()
        let ss = (subss <== subs vl current) <== subs vr current
        let (struct (ss',  l')) = collect ctx vl ss  left
        let (struct (ss'', r')) = collect ctx vr ss' right
        struct (ss'' <== subs ty current, { Node = BinaryX (l', op, r'); Data = current })
      | BinaryOperation.Equal
      | BinaryOperation.NotEqual
      | BinaryOperation.GreaterThan
      | BinaryOperation.GreaterThanOrEqual
      | BinaryOperation.LessThan
      | BinaryOperation.LessThanOrEqual ->
        let current = TExact typeof<bool>
        let vl = ctx.NewVar ()
        let vr = ctx.NewVar ()
        let ss = (subss <== subs vl vr)
        let (struct (ss',  l')) = collect ctx vl ss  left
        let (struct (ss'', r')) = collect ctx vr ss' right
        struct (ss'' <== subs ty current, { Node = BinaryX (l', op, r'); Data = current })
      | _ -> failwithf "Unsupported binary op = %A" op


  abstract CreateContext : rootType:Type -> ITypeInferenceContext
  abstract CollectSubstitutions : context:ITypeInferenceContext * ast:Node -> struct (Subss * NodeX<Ty>)
  abstract ResolveTypes : context:ITypeInferenceContext * substitutions:Subss * ast:NodeX<Ty> -> NodeX<Type>

  default __.CreateContext rootType =
    let sup = ref 0L
    { RootType    = rootType
      NewVar      = fun () -> Interlocked.Increment sup |> TVar
      ResolveCall = fun (struct (name, argNum)) -> callInfoResolver.ResolveCall (name, argNum) }
    :> ITypeInferenceContext

  default __.CollectSubstitutions (context, ast) =
    collect context (context.NewVar ()) (Subss []) ast

  default __.ResolveTypes (_, substitutions, ast) =
    let mapper ty =
      match substitutions.TryResolveExact ty with
      | Some typ -> typ
      | _        -> failwithf "Unable to resolve %A" ty
    NodeX.mapData mapper ast


  interface ITypeInferer with
    member this.InferTypes (ast, rootType) =
      let ctx = this.CreateContext rootType
      let (struct (subs, ast')) = this.CollectSubstitutions (ctx, ast)
      this.ResolveTypes (ctx, subs, ast')
