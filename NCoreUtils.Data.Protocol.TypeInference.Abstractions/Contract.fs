namespace NCoreUtils.Data.Protocol.TypeInference

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Diagnostics.CodeAnalysis
open System.Diagnostics.Contracts
open System.Linq.Expressions
open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Data.Protocol
open NCoreUtils.Data.Protocol.Ast

// ************************** COMMON DATA ***********************************

/// Represents immutable name identifier that is unique within some context.
[<Struct>]
[<StructuralEquality; StructuralComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
[<ExcludeFromCodeCoverage>]
type NameUid =
  NameUid of Uid:int
  with
    member private this.DisplayString
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () =
        let (NameUid uid) = this in sprintf "#%d" uid
    member this.RawDisplayString
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () =
        let (NameUid uid) = this in sprintf "%d" uid
    [<Pure>]
    [<ExcludeFromCodeCoverage>]
    override this.ToString () = this.DisplayString

/// Represents immutable type identifier that is unique within some context.
[<Struct>]
[<StructuralEquality; StructuralComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
[<ExcludeFromCodeCoverage>]
type TypeUid =
  TypeUid of int
  with
    member private this.DisplayString
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () =
        let (TypeUid uid) = this in sprintf "'%d" uid
    [<Pure>]
    [<ExcludeFromCodeCoverage>]
    override this.ToString () = this.DisplayString

/// Defines functionality for naming objects.
[<Interface>]
[<AllowNullLiteral>]
type IHasName =
  /// Gets name of the object.
  abstract Name : string

type IProperty =
  abstract PropertyType : Type
  abstract CreateExpression : instance:Expression -> Expression

[<Interface>]
[<AllowNullLiteral>]
type IPropertyResolver =
  abstract TryResolve : ``type``:Type * name:string -> IProperty voption

[<AutoOpen>]
module private UnresolvedNodeHelpers =

  [<ExcludeFromCodeCoverage>]
  let inline functionName (source : 'T) =
    match box source with
    | null -> "<null>"
    | :? string   as string -> string
    | :? IHasName as desc   -> desc.Name
    | value                 -> value.ToString ()

/// Represents a single unresolved node in protocol AST.
[<StructuralEquality; NoComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
type UnresolvedNode<'T> =
  /// Represents lambda expression node.
  | UnresolvedLambda     of TypeUid:TypeUid * Arg:UnresolvedNode<'T> * Body:UnresolvedNode<'T>
  /// Represents binary operation node.
  | UnresolvedBinary     of TypeUid:TypeUid * Left:UnresolvedNode<'T> * Operation:BinaryOperation * Right:UnresolvedNode<'T>
  /// Represents function invocation node.
  | UnresolvedCall       of TypeUid:TypeUid * Name:'T * Arguments:ImmutableArray<UnresolvedNode<'T>>
  /// Represents member access node.
  | UnresolvedMember     of TypeUid:TypeUid * Instance:UnresolvedNode<'T> * Member:string
  /// Represents constant node.
  | UnresolvedConstant   of TypeUid:TypeUid * RawValue:string
  /// Represents identifier node.
  | UnresolvedIdentifier of TypeUid:TypeUid * Value:NameUid
  with
    member private this.DisplayString
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () =
        let inner =
          match this with
          | UnresolvedLambda (_, arg, body)       -> sprintf "(%A) => (%A)" arg body
          | UnresolvedBinary (_, left, op, right) -> sprintf "(%A) %A (%A)" left op right
          | UnresolvedCall (_, name, args)        -> sprintf "%s(%s)" (functionName name) (args |> Seq.map (sprintf "%A") |> String.concat ", ")
          | UnresolvedMember (_, instance, name)  -> sprintf "(%A).%s" instance name
          | UnresolvedConstant (_, value)         -> value
          | UnresolvedIdentifier (_, name)        -> sprintf "%A" name
        sprintf "%A : %A" inner this.TypeUid
    /// Gets type UID assigned to the node.
    member this.TypeUid =
      match this with
      | UnresolvedLambda     (uid, _, _)    -> uid
      | UnresolvedBinary     (uid, _, _, _) -> uid
      | UnresolvedCall       (uid, _, _)    -> uid
      | UnresolvedMember     (uid, _, _)    -> uid
      | UnresolvedConstant   (uid, _)       -> uid
      | UnresolvedIdentifier (uid, _)       -> uid
    /// Gets child nodes of the node.
    member this.GetChildren () =
      match this with
      | UnresolvedLambda (_, arg, body)       -> [| arg; body |] :> seq<_>
      | UnresolvedBinary (_, left, _, right)  -> [| left; right |] :> seq<_>
      | UnresolvedCall (_, _, args)           -> args :> seq<_>
      | UnresolvedMember (_, instance, _)     -> Seq.singleton instance
      | UnresolvedConstant _
      | UnresolvedIdentifier _                -> Seq.empty
    [<Pure>]
    [<ExcludeFromCodeCoverage>]
    override this.ToString () = this.DisplayString

/// Represents immutable type constraint collection.
type
  [<StructuralEquality; NoComparison>]
  TypeConstraints = {
    /// Gets member constraints.
    Members    : ImmutableHashSet<CaseInsensitive>
    /// Gets interface constraints.
    Interfaces : ImmutableHashSet<Type>
    /// Gets base type constraint.
    Base       : Type
    /// Gets numericity constraint.
    IsNumeric  : Nullable<bool>
    /// Gets nullability constraint.
    IsNullable : Nullable<bool>
    /// Gets membership information.
    MemberOf   : ImmutableList<struct (TypeUid * string)>  }

/// Represents immutable type variable.
and
  [<Struct>]
  [<StructuralEquality; NoComparison>]
  TypeVariable =
    /// Represents exact type.
    | KnownType   of Type:Type
    /// Represents unresolved type.
    | UnknownType of Constraints:TypeConstraints

/// Contains type constraints operations.
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
      typeof<float>
      typeof<DateTimeOffset>
      typeof<Nullable<int8>>
      typeof<Nullable<int16>>
      typeof<Nullable<int32>>
      typeof<Nullable<int64>>
      typeof<Nullable<uint8>>
      typeof<Nullable<uint16>>
      typeof<Nullable<uint32>>
      typeof<Nullable<uint64>>
      typeof<Nullable<decimal>>
      typeof<Nullable<single>>
      typeof<Nullable<float>>
      typeof<Nullable<DateTimeOffset>> ]
    |> ImmutableHashSet.CreateRange

  let private nullableTypedefs =
    [ typedefof<Nullable<_>>
      typedefof<Option<_>>
      typedefof<ValueOption<_>> ]
    |> ImmutableHashSet.CreateRange

  [<Pure>]
  let private isNullable (ty : Type) =
    ty.IsGenericType && (ty.GetGenericTypeDefinition () |> nullableTypedefs.Contains)

  /// Checks whether a specified type is enum or nullable enum.
  [<Pure>]
  let private isEnum (ty : Type) =
    ty.IsEnum || (isNullable ty && ty.GetGenericArguments().[0].IsEnum)

  /// Gets empty type constraints.
  [<CompiledName("Empty")>]
  let empty =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty
      MemberOf   = ImmutableList.Empty }

  /// Gets numeric type constraint.
  [<CompiledName("Numeric")>]
  let numeric =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.mk true
      MemberOf   = ImmutableList.Empty }

  /// Gets non-numeric type constraint.
  [<CompiledName("NotNumeric")>]
  let notNumeric =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.mk false
      MemberOf   = ImmutableList.Empty }

  /// Gets nullable type constraint.
  [<CompiledName("Nullable")>]
  let nullable =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.mk true
      IsNumeric  = Nullable.empty
      MemberOf   = ImmutableList.Empty }

  /// <summary>
  /// Gets type constraint for the specified member.
  /// </summary>
  /// <param name="name">Member name.</param>
  /// <returns>Type constraint for the specified member.</returns>
  [<Pure>]
  [<CompiledName("HasMember")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let hasMember name =
    { Members    = ImmutableHashSet.Create (CaseInsensitive name)
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty
      MemberOf   = ImmutableList.Empty }

  /// <summary>
  /// Gets type constraint for the specified interface.
  /// </summary>
  /// <param name="iface">Interface type.</param>
  /// <returns>Type constraint for the specified interface.</returns>
  [<Pure>]
  [<CompiledName("HasInterface")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let hasInterface (iface : Type) =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Create iface
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty
      MemberOf   = ImmutableList.Empty }

  let isMemberOf memberName owner =
    { Members    = ImmutableHashSet.Empty
      Interfaces = ImmutableHashSet.Empty
      Base       = null
      IsNullable = Nullable.empty
      IsNumeric  = Nullable.empty
      MemberOf   = ImmutableList.Create (struct (owner, memberName)) }

  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private checkMembers (members : seq<_>) (candidateType : Type) =
    let rec impl (enumerator : IEnumerator<CaseInsensitive>) =
      match enumerator.MoveNext () with
      | false -> ValueNone
      | _ ->
        let memberName = enumerator.Current
        match Members.getMember memberName.Value candidateType with
        | NoMember -> ValueSome { TargetType = TypeRef candidateType; Reason = MissingMember memberName.Value }
        | _        -> impl enumerator
    use enumerator = members.GetEnumerator ()
    impl enumerator

  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private checkInterfaces (members : seq<_>) (candidateType : Type) =
    let rec impl (enumerator : IEnumerator<Type>) =
      match enumerator.MoveNext () with
      | false -> ValueNone
      | _ ->
        let iface = enumerator.Current
        match iface.IsAssignableFrom candidateType with
        | false -> ValueSome { TargetType = TypeRef candidateType; Reason = MissingInterfaceImplmentation (TypeRef iface) }
        | _     -> impl enumerator
    use enumerator = members.GetEnumerator ()
    impl enumerator

  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private checkBaseType (baseType : Type) (candidateType : Type) =
    match baseType with
    | null -> ValueNone
    | _ ->
    match baseType = candidateType || baseType.IsAssignableFrom candidateType with
    | true -> ValueNone
    | _    -> ValueSome { TargetType = TypeRef candidateType; Reason = IncompatibleType (TypeRef baseType) }

  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private checkNumericity (numericity : Nullable<_>) (candidateType : Type) =
    match numericity.HasValue with
    | false -> ValueNone
    | true ->
      match numericity.Value, (numericTypes.Contains candidateType || isEnum candidateType) with
      | true, false -> ValueSome { TargetType = TypeRef candidateType; Reason = NumericConstraint }
      | false, true -> ValueSome { TargetType = TypeRef candidateType; Reason = NonNumericConstraint }
      | _           -> ValueNone

  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private checkNullability (nullability : Nullable<_>) (candidateType : Type) =
    match nullability.HasValue with
    | false -> ValueNone
    | true ->
      match nullability.Value, not candidateType.IsValueType || isNullable candidateType with
      | true, false -> ValueSome { TargetType = TypeRef candidateType; Reason = NullableConstraint }
      | false, true -> ValueSome { TargetType = TypeRef candidateType; Reason = NonNullableConstraint }
      | _           -> ValueNone

  /// <summary>
  /// Matches type against type constraints.
  /// </summary>
  /// <param name="constraints">Constraints to match against.</param>
  /// <param name="candidateType">Type to match.</param>
  /// <returns>
  /// Either empty value if the specified type matches the specified constraints or constraints mismatch otherwise.
  /// </returns>
  [<Pure>]
  [<CompiledName("Match")>]
  let ``match`` (constraints : TypeConstraints) (candidateType : Type) =
    match checkMembers constraints.Members candidateType with
    | ValueSome _ as result -> result
    | _ ->
    match checkInterfaces constraints.Interfaces candidateType with
    | ValueSome _ as result -> result
    | _ ->
    match checkBaseType constraints.Base candidateType with
    | ValueSome _ as result -> result
    | _ ->
    match checkNumericity constraints.IsNumeric candidateType with
    | ValueSome _ as result -> result
    | _ ->
    match checkNullability constraints.IsNullable candidateType with
    | ValueSome _ as result -> result
    | _ -> ValueNone

  /// <summary>
  /// Validates type against type constraints. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="constraints">Constraints to validate against.</param>
  /// <param name="candidateType">Type to validate.</param>
  /// <returns>
  /// The specified type.
  /// </returns>
  [<Pure>]
  [<CompiledName("Validate")>]
  let validate (constraints : TypeConstraints) (candidateType : Type) =
    match ``match`` constraints candidateType with
    | ValueNone -> candidateType
    | ValueSome error -> ProtocolTypeConstraintMismatchException error |> raise

  /// <summary>
  /// Merges constraints. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="a">First constraint collection.</param>
  /// <param name="b">Second constraint collection.</param>
  /// <returns>
  /// Result constraint collection.
  /// </returns>
  [<Pure>]
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
    let memberOf =
      match a.MemberOf.Count, b.MemberOf.Count with
      | 0, 0 -> ImmutableList.Empty
      | _, 0 -> a.MemberOf
      | 0, _ -> b.MemberOf
      | _    -> a.MemberOf.AddRange b.MemberOf
    { Members    = Seq.append a.Members b.Members |> ImmutableHashSet.CreateRange
      Interfaces = Seq.append a.Interfaces b.Interfaces |> ImmutableHashSet.CreateRange
      Base       = newBase
      IsNullable = isNullable
      IsNumeric  = isNumeric
      MemberOf   = memberOf }

/// Contains type variable oprations.
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TypeVariable =

  /// Gets empty type variable.
  [<CompiledName("Empty")>]
  let empty = UnknownType TypeConstraints.empty

  /// Gets numeric type variable.
  [<CompiledName("Numeric")>]
  let numeric = UnknownType TypeConstraints.numeric

  /// Gets non-numeric type variable.
  [<CompiledName("NotNumeric")>]
  let notNumeric = UnknownType TypeConstraints.notNumeric

  /// Gets nullable type variable.
  [<CompiledName("Nullable")>]
  let nullable = UnknownType TypeConstraints.nullable

  /// <summary>
  /// Gets type variable for the specified member.
  /// </summary>
  /// <param name="name">Member name.</param>
  /// <returns>Type variable for the specified member.</returns>
  [<Pure>]
  [<CompiledName("HasMember")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let hasMember name = UnknownType (TypeConstraints.hasMember name)

  /// <summary>
  /// Gets type variable for the specified interface.
  /// </summary>
  /// <param name="iface">Interface type.</param>
  /// <returns>Type variable for the specified interface.</returns>
  [<Pure>]
  [<CompiledName("HasInterface")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let hasInterface iface = UnknownType (TypeConstraints.hasInterface iface)

  let isMemberOf owner memberName = UnknownType (TypeConstraints.isMemberOf owner memberName)

  /// <summary>
  /// Merges type variables. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="a">First variable collection.</param>
  /// <param name="b">Second variable collection.</param>
  /// <returns>
  /// Result variable collection.
  /// </returns>
  [<Pure>]
  [<CompiledName("Merge")>]
  let merge a b =
    match a, b with
    | KnownType a, KnownType b ->
      match a = b with
      | true -> KnownType a
      | _    ->
        match a.IsAssignableFrom b with
        | true -> KnownType b
        | _ ->
        match b.IsAssignableFrom a with
        | true -> KnownType a
        | _    -> sprintf "Incompatible types: %A %A" a b |> ProtocolTypeInferenceException |> raise
    | UnknownType a, UnknownType b -> TypeConstraints.merge a b |> UnknownType
    | UnknownType c, KnownType ty
    | KnownType ty, UnknownType c -> TypeConstraints.validate c ty |> KnownType

// *********** MODULE FUNCTIONS AS MEMBERS **********************************

type TypeConstraints with
  /// Gets empty type constraints.
  static member Empty
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeConstraints.empty
  /// Gets numeric type constraint.
  static member Numberic
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeConstraints.numeric
  /// Gets non-numeric type constraint.
  static member NotNumeric
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeConstraints.notNumeric
  /// Gets nullable type constraint.
  static member Nullable
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeConstraints.nullable
  /// <summary>
  /// Gets type constraint for the specified member.
  /// </summary>
  /// <param name="name">Member name.</param>
  /// <returns>Type constraint for the specified member.</returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasMember name =
    TypeConstraints.hasMember name
  /// <summary>
  /// Gets type constraint for the specified interface.
  /// </summary>
  /// <param name="iface">Interface type.</param>
  /// <returns>Type constraint for the specified interface.</returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasInterface iface =
    TypeConstraints.hasInterface iface
  /// <summary>
  /// Validates type against type constraints. Returns constraint mismatch is any.
  /// </summary>
  /// <param name="candidateType">Type to validate.</param>
  /// <returns>
  /// Either empty value or constraint mismatch.
  /// </returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Match candidateType =
    TypeConstraints.``match`` this candidateType
  /// <summary>
  /// Merges constraints. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="other">Second constraint collection.</param>
  /// <returns>
  /// Result constraint collection.
  /// </returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Validate candidateType =
    TypeConstraints.validate this candidateType
  /// <summary>
  /// Merges constraints. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="other">Second constraint collection.</param>
  /// <returns>
  /// Result constraint collection.
  /// </returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Merge other =
    TypeConstraints.merge this other

type TypeVariable with
  /// Gets empty type variable.
  static member Empty
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeVariable.empty
  /// Gets numeric type variable.
  static member Numberic
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeVariable.numeric
  /// Gets non-numeric type variable.
  static member NotNumeric
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeVariable.notNumeric
  /// Gets nullable type variable.
  static member Nullable
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () = TypeVariable.nullable
  /// <summary>
  /// Gets type variable for the specified member.
  /// </summary>
  /// <param name="name">Member name.</param>
  /// <returns>Type variable for the specified member.</returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasMember name =
    TypeVariable.hasMember name
  /// <summary>
  /// Gets type variable for the specified interface.
  /// </summary>
  /// <param name="iface">Interface type.</param>
  /// <returns>Type variable for the specified interface.</returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member HasInterface iface =
    TypeConstraints.hasInterface iface
  /// <summary>
  /// Merges type variables. Throws exception on constraint mismatch.
  /// </summary>
  /// <param name="other">Second variable collection.</param>
  /// <returns>
  /// Result variable collection.
  /// </returns>
  [<Pure>]
  [<ExcludeFromCodeCoverage>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Merge other =
    TypeVariable.merge this other


// ************************** INTERFACE *************************************

/// Defines function descriptor functionality.
[<Interface>]
[<AllowNullLiteral>]
type IFunctionDescriptor =
  inherit IHasName
  /// Gets result type.
  abstract ResultType    : Type
  /// Gets arguments types.
  abstract ArgumentTypes : IReadOnlyList<Type>
  /// <summary>
  /// Creates expression that represents function invocation defined by the actual descriptor instance.
  /// </summary>
  /// <param name="arguments">Function arguments.</param>
  /// <returns>Expression that represents function invocation.</returns>
  abstract CreateExpression : arguments:IReadOnlyList<Expression> -> Expression

/// Represents a single resolved node in protocol AST.
[<StructuralEquality; NoComparison>]
[<StructuredFormatDisplay("{DisplayString}")>]
[<ExcludeFromCodeCoverage>]
type ResolvedNode =
  /// Represents lambda expression node.
  | ResolvedLambda     of Type:Type * Arg:ResolvedNode * Body:ResolvedNode
  /// Represents binary operation node.
  | ResolvedBinary     of Type:Type * Left:ResolvedNode * Operation:BinaryOperation * Right:ResolvedNode
  /// Represents function invocation node.
  | ResolvedCall       of Type:Type * Name:IFunctionDescriptor * Arguments:ImmutableArray<ResolvedNode>
  /// Represents member access node.
  | ResolvedMember     of Type:Type * Instance:ResolvedNode * Member:string
  /// Represents constant node.
  | ResolvedConstant   of Type:Type * RawValue:string
  /// Represents identifier node.
  | ResolvedIdentifier of Type:Type * Value:NameUid
  with
    member private this.DisplayString
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>][<ExcludeFromCodeCoverage>] get () =
        let inner =
          match this with
          | ResolvedLambda (_, arg, body)       -> sprintf "(%A) => (%A)" arg body
          | ResolvedBinary (_, left, op, right) -> sprintf "(%A) %A (%A)" left op right
          | ResolvedCall (_, desc, args)        -> sprintf "%s(%s)" desc.Name (args |> Seq.map (sprintf "%A") |> String.concat ", ")
          | ResolvedMember (_, instance, name)  -> sprintf "(%A).%s" instance name
          | ResolvedConstant (_, value)         -> value
          | ResolvedIdentifier (_, name)        -> sprintf "%A" name
        sprintf "%A : %A" inner this.Type
    member this.Type
      with [<Pure>][<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () =
        match this with
        | ResolvedLambda     (ty, _, _)    -> ty
        | ResolvedBinary     (ty, _, _, _) -> ty
        | ResolvedCall       (ty, _, _)    -> ty
        | ResolvedMember     (ty, _, _)    -> ty
        | ResolvedConstant   (ty, _)       -> ty
        | ResolvedIdentifier (ty, _)       -> ty
    [<Pure>]
    [<ExcludeFromCodeCoverage>]
    override this.ToString () = this.DisplayString

/// Defines type inferrer functionality.
[<Interface>]
type ITypeInferrer =
  abstract PropertyResolver : IPropertyResolver

  /// <summary>
  /// Infers type for all nodes in the specified expression with respect to the specified root type.
  /// </summary>
  /// <param name="rootType">Argument type of root lambda expression.</param>
  /// <param name="expression">Expression to infer types within.</param>
  /// <returns>Expression with inferred types.</returns>
  abstract InferTypes : rootType:Type * expression:Node -> ResolvedNode