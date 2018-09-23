namespace NCoreUtils.Data.Protocol.TypeInference

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq.Expressions
open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Data.Protocol
open NCoreUtils.Data.Protocol.Ast

// ************************** COMMON DATA ***********************************

[<Struct>]
[<StructuralEquality; StructuralComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
type NameUid =
  NameUid of Uid:int
  with
    member private this.DisplayString =
      let (NameUid uid) = this in sprintf "#%d" uid
    override this.ToString () = this.DisplayString

[<Struct>]
[<StructuralEquality; StructuralComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
type TypeUid =
  TypeUid of int
  with
    member private this.DisplayString =
      let (TypeUid uid) = this in sprintf "'%d" uid
    override this.ToString () = this.DisplayString

[<Interface>]
[<AllowNullLiteral>]
type IHasName =
  abstract Name : string

[<AutoOpen>]
module private UnresolvedNodeHelpers =

  let functionName (source : 'T) =
    match box source with
    | null -> "<null>"
    | :? string   as string -> string
    | :? IHasName as desc   -> desc.Name
    | value                 -> value.ToString ()

[<StructuralEquality; NoComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
type UnresolvedNode<'T> =
  | UnresolvedLambda     of TypeUid:TypeUid * Arg:UnresolvedNode<'T> * Body:UnresolvedNode<'T>
  | UnresolvedBinary     of TypeUid:TypeUid * Left:UnresolvedNode<'T> * Operation:BinaryOperation * Right:UnresolvedNode<'T>
  | UnresolvedCall       of TypeUid:TypeUid * Name:'T * Arguments:ImmutableArray<UnresolvedNode<'T>>
  | UnresolvedMember     of TypeUid:TypeUid * Instance:UnresolvedNode<'T> * Member:string
  | UnresolvedConstant   of TypeUid:TypeUid * RawValue:string
  | UnresolvedIdentifier of TypeUid:TypeUid * Value:NameUid
  with
    member private this.DisplayString =
      let inner =
        match this with
        | UnresolvedLambda (_, arg, body)       -> sprintf "(%A) => (%A)" arg body
        | UnresolvedBinary (_, left, op, right) -> sprintf "(%A) %A (%A)" left op right
        | UnresolvedCall (_, name, args)        -> sprintf "%s(%s)" (functionName name) (args |> Seq.map (sprintf "%A") |> String.concat ", ")
        | UnresolvedMember (_, instance, name)  -> sprintf "(%A).%s" instance name
        | UnresolvedConstant (_, value)         -> value
        | UnresolvedIdentifier (_, name)        -> sprintf "%A" name
      sprintf "%A : %A" inner this.TypeUid
    member this.TypeUid =
      match this with
      | UnresolvedLambda     (uid, _, _)    -> uid
      | UnresolvedBinary     (uid, _, _, _) -> uid
      | UnresolvedCall       (uid, _, _)    -> uid
      | UnresolvedMember     (uid, _, _)    -> uid
      | UnresolvedConstant   (uid, _)       -> uid
      | UnresolvedIdentifier (uid, _)       -> uid
    member this.GetChildren () =
      match this with
      | UnresolvedLambda (_, arg, body)       -> [| arg; body |] :> seq<_>
      | UnresolvedBinary (_, left, _, right)  -> [| left; right |] :> seq<_>
      | UnresolvedCall (_, _, args)           -> args :> seq<_>
      | UnresolvedMember (_, instance, _)     -> Seq.singleton instance
      | UnresolvedConstant _
      | UnresolvedIdentifier _                -> Seq.empty
    override this.ToString () = this.DisplayString

[<StructuralEquality; NoComparison>]
type TypeConstraints = {
  Members    : ImmutableHashSet<CaseInsensitive>
  Interfaces : ImmutableHashSet<Type>
  Base       : Type
  IsNumeric  : Nullable<bool>
  IsNullable : Nullable<bool>  }

[<Struct>]
[<StructuralEquality; NoComparison>]
type TypeVariable =
  | KnownType   of Type:Type
  | UnknownType of Constants:TypeConstraints

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TypeConstraints =

  let private numericTypes =
    [ typeof<int8>
      typeof<int16>
      typeof<int32>
      typeof<int64>
      typeof<uint8>
      typeof<uint16>
      typeof<uint32>
      typeof<uint64>
      typeof<decimal>
      typeof<single>
      typeof<float> ]
    |> ImmutableHashSet.CreateRange

  let private nullableTypedefs =
    [ typedefof<Nullable<_>>
      typedefof<Option<_>>
      typedefof<ValueOption<_>> ]
    |> ImmutableHashSet.CreateRange

  let private isNullable (ty : Type) =
    ty.IsGenericType && (ty.GetGenericTypeDefinition () |> nullableTypedefs.Contains)

  [<CompiledName("Empty")>]
  let empty =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty }

  [<CompiledName("Numeric")>]
  let numeric =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.mk true }

  [<CompiledName("NotNumeric")>]
  let notNumeric =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.mk false }

  [<CompiledName("Nullable")>]
  let nullable =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.mk true
      IsNumeric  = Nullable.empty }

  [<CompiledName("HasMember")>]
  let hasMember name =
    { Members    = ImmutableHashSet.Create (CaseInsensitive name)
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty }

  [<CompiledName("Validate")>]
  let validate (constraints : TypeConstraints) (candidateType : Type) =
    for memberName in constraints.Members do
      if Members.getMember memberName.Value candidateType = NoMember then
        sprintf "Type %A has no member %s." candidateType memberName.Value |> ProtocolTypeInferenceException |> raise
    for iface in constraints.Interfaces do
      if not (iface.IsAssignableFrom candidateType) then
        sprintf "Type %A does not implement %A." candidateType iface |> ProtocolTypeInferenceException |> raise
    if not (isNull constraints.Base) then
      if constraints.Base <> candidateType && not (constraints.Base.IsAssignableFrom candidateType) then
        sprintf "Type %A is not compatible with constrinted base type %A." candidateType constraints.Base |> ProtocolTypeInferenceException |> raise
    match constraints.IsNumeric with
    | Nullable.Empty -> ()
    | Nullable.Value true ->
      if not (numericTypes.Contains candidateType) then
        sprintf "Type %A has been constrinted to be numeric." candidateType |> ProtocolTypeInferenceException |> raise
    | Nullable.Value false ->
      if numericTypes.Contains candidateType then
        sprintf "Type %A has been constrinted to be not numeric." candidateType |> ProtocolTypeInferenceException |> raise
    match constraints.IsNullable with
    | Nullable.Empty -> ()
    | Nullable.Value true ->
      if candidateType.IsValueType && not (isNullable candidateType) then
        sprintf "Type %A has been constrinted to be nullable." candidateType |> ProtocolTypeInferenceException |> raise
    | Nullable.Value false ->
      if not (candidateType.IsValueType && not (isNullable candidateType)) then
        sprintf "Type %A has been constrinted to be non-nullable." candidateType |> ProtocolTypeInferenceException |> raise
    candidateType

  [<CompiledName("Merge")>]
  let merge (a : TypeConstraints) (b : TypeConstraints) =
    let newBase =
      match a.Base, b.Base with
      | null, b
      | b,    null -> b
      | a, b when a = b -> a
      | a, b ->
        match a.IsSubclassOf b with
        | true -> a
        | _ ->
        match b.IsSubclassOf a with
        | true ->  b
        | _ -> sprintf "Incompatible base classes: %A %A" a b |> ProtocolTypeInferenceException |> raise
    let isNumeric =
      match a.IsNumeric, b.IsNumeric with
      | Nullable.Empty, x
      | x, Nullable.Empty -> x
      | Nullable.Value a, Nullable.Value b ->
      match a = b with
      | true -> Nullable.mk a
      | _    -> ProtocolTypeInferenceException "Incompatible numericity" |> raise
    let isNullable =
      match a.IsNullable, b.IsNullable with
      | Nullable.Empty, x
      | x, Nullable.Empty -> x
      | Nullable.Value a, Nullable.Value b ->
      match a = b with
      | true -> Nullable.mk a
      | _    -> ProtocolTypeInferenceException "Incompatible nullability" |> raise
    { Members    = Seq.append a.Members b.Members |> ImmutableHashSet.CreateRange
      Interfaces = Seq.append a.Interfaces b.Interfaces |> ImmutableHashSet.CreateRange
      Base       = newBase
      IsNullable = isNullable
      IsNumeric  = isNumeric }

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TypeVariable =

  [<CompiledName("Empty")>]
  let empty = UnknownType TypeConstraints.empty

  [<CompiledName("Numeric")>]
  let numeric = UnknownType TypeConstraints.numeric

  [<CompiledName("NotNumeric")>]
  let notNumeric = UnknownType TypeConstraints.notNumeric

  [<CompiledName("Nullable")>]
  let nullable = UnknownType TypeConstraints.nullable

  [<CompiledName("HasMember")>]
  let hasMember name = UnknownType (TypeConstraints.hasMember name)

  [<CompiledName("Merge")>]
  let merge a b =
    match a, b with
    | KnownType a, KnownType b ->
      match a = b with
      | true -> KnownType a
      | _    -> sprintf "Incompatible types: %A %A" a b |> ProtocolTypeInferenceException |> raise
    | UnknownType a, UnknownType b -> TypeConstraints.merge a b |> UnknownType
    | UnknownType c, KnownType ty
    | KnownType ty, UnknownType c -> TypeConstraints.validate c ty |> KnownType

// *********** MODULE FUNCTIONS AS MEMBERS **********************************

type TypeConstraints with
  static member Empty
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeConstraints.empty
  static member Numberic
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeConstraints.numeric
  static member NotNumeric
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeConstraints.notNumeric
  static member Nullable
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeConstraints.nullable
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasMember name =
    TypeConstraints.hasMember name
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Validate candidateType =
    TypeConstraints.validate this candidateType
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Merge other =
    TypeConstraints.merge this other

type TypeVariable with
  static member Empty
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeVariable.empty
  static member Numberic
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeVariable.numeric
  static member NotNumeric
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeVariable.notNumeric
  static member Nullable
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeVariable.nullable
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasMember name =
    TypeVariable.hasMember name
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Merge other =
    TypeVariable.merge this other


// ************************** INTERFACE *************************************

[<Interface>]
[<AllowNullLiteral>]
type IFunctionDescriptor =
  inherit IHasName
  abstract ResultType    : Type
  abstract ArgumentTypes : IReadOnlyList<Type>
  abstract CreateExpression : arguments:IReadOnlyList<Expression> -> Expression

[<StructuralEquality; NoComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
type ResolvedNode =
  | ResolvedLambda     of Type:Type * Arg:ResolvedNode * Body:ResolvedNode
  | ResolvedBinary     of Type:Type * Left:ResolvedNode * Operation:BinaryOperation * Right:ResolvedNode
  | ResolvedCall       of Type:Type * Name:IFunctionDescriptor * Arguments:ImmutableArray<ResolvedNode>
  | ResolvedMember     of Type:Type * Instance:ResolvedNode * Member:string
  | ResolvedConstant   of Type:Type * RawValue:string
  | ResolvedIdentifier of Type:Type * Value:NameUid
  with
    member private this.DisplayString =
      let inner =
        match this with
        | ResolvedLambda (_, arg, body)       -> sprintf "(%A) => (%A)" arg body
        | ResolvedBinary (_, left, op, right) -> sprintf "(%A) %A (%A)" left op right
        | ResolvedCall (_, desc, args)        -> sprintf "%s(%s)" desc.Name (args |> Seq.map (sprintf "%A") |> String.concat ", ")
        | ResolvedMember (_, instance, name)  -> sprintf "(%A).%s" instance name
        | ResolvedConstant (_, value)         -> value
        | ResolvedIdentifier (_, name)        -> sprintf "%A" name
      sprintf "%A : %A" inner this.Type
    member this.Type =
      match this with
      | ResolvedLambda     (ty, _, _)    -> ty
      | ResolvedBinary     (ty, _, _, _) -> ty
      | ResolvedCall       (ty, _, _)    -> ty
      | ResolvedMember     (ty, _, _)    -> ty
      | ResolvedConstant   (ty, _)       -> ty
      | ResolvedIdentifier (ty, _)       -> ty
    override this.ToString () = this.DisplayString

[<Interface>]
type ITypeInferrer =
  /// <summary>
  /// Infers type for all nodes in the specified expression with respect to the specified root type.
  /// </summary>
  /// <param name="rootType">Argument type of root lambda expression.</param>
  /// <param name="expression">Expression to infer types within.</param>
  /// <returns>Expression with inferred types.</returns>
  abstract InferTypes : rootType:Type * expression:Node -> ResolvedNode