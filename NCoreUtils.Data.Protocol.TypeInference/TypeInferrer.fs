namespace NCoreUtils.Data.Protocol.TypeInference

open System.Collections.Immutable
open System.Diagnostics.Contracts
open System.Threading
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Data.Protocol
open NCoreUtils.Data.Protocol.Ast
open System.Runtime.CompilerServices
open System.Diagnostics.CodeAnalysis

/// Represents immutable type inference context.
[<NoEquality; NoComparison>]
type TypeInferenceContext =
  internal
    { Types         : Map<TypeUid, TypeVariable>
      Substitutions : Map<TypeUid, TypeUid list> }

/// Contains type inference context operations.
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TypeInferenceContext =

  /// Gets empty type inference context.
  [<CompiledName("Empty")>]
  let empty = { Types = Map.empty; Substitutions = Map.empty }

  /// <summary>
  /// Creates new type inference context with the specified type variable.
  /// </summary>
  /// <param name="uid">Type UID to apply variable on.</param>
  /// <param name="variable">Type variable to apply.</param>
  /// <param name="ctx">Type inference context to use.</param>
  /// <returns>New type inference context with the specified type variable.</returns>
  [<Pure>]
  [<CompiledName("ApplyConstraint")>]
  let applyConstraint uid variable (ctx: TypeInferenceContext) =
    let types' =
      ctx.Types
      |> Map.map (fun id c -> if id = uid then TypeVariable.merge c variable else c)
    { Types = types'; Substitutions = ctx.Substitutions }

  /// <summary>
  /// Creates new type inference context with the specified substitution.
  /// </summary>
  /// <param name="a">Type UID to substitute.</param>
  /// <param name="b">Target type UID.</param>
  /// <param name="ctx">Type inference context to use.</param>
  /// <returns>New type inference context with the specified substitution.</returns>
  [<Pure>]
  [<CompiledName("Substitute")>]
  let substitute a b (ctx: TypeInferenceContext) =
    let substitutions =
      match Map.tryFind a ctx.Substitutions with
      | Some l -> Map.add a (b :: l) ctx.Substitutions
      | _      -> Map.add a [b] ctx.Substitutions
    { Types = ctx.Types; Substitutions = substitutions }

  /// <summary>
  /// Collects all constaints (i.e. both owned constaints and substituted constraints) for the specified type UID.
  /// </summary>
  /// <param name="uid">Type UID to collect constraints for.</param>
  /// <param name="ctx">Type inference context to use.</param>
  /// <returns>Type constraints for the specified type UID.</returns>
  [<Pure>]
  [<CompiledName("GetAllConstraints")>]
  let getAllConstraints uid ctx =
    let rec impl loop uid (ctx : TypeInferenceContext) =
      match Set.contains uid loop with
      | true -> TypeVariable.empty
      | _ ->
        let loop' = Set.add uid loop
        match Map.tryFind uid ctx.Types, Map.tryFind uid ctx.Substitutions with
        | None, None
        | None, Some []           -> TypeVariable.empty
        | Some v0, None
        | Some v0, Some []        -> v0
        | Some v0, Some l         -> List.fold (fun v uid -> impl loop' uid ctx |> TypeVariable.merge v) v0 l
        | None,    Some (v0 :: l) -> List.fold (fun v uid -> impl loop' uid ctx |> TypeVariable.merge v) (impl loop' v0 ctx) l
    impl Set.empty uid ctx

  /// <summary>
  /// Instantiate concrete type for the specified type UID.
  /// </summary>
  /// <param name="uid">Type UID to instantiate type for.</param>
  /// <param name="ctx">Type inference context to use.</param>
  /// <returns>Concrete type for the specified type UID.</returns>
  [<Pure>]
  [<CompiledName("InstantiateType")>]
  let instantiateType uid ctx =
    match getAllConstraints uid ctx with
    | KnownType ty -> ty
    | UnknownType c ->
      match c.Base with
      | null ->
        match c.Interfaces.Count with
        | 1 -> c.Interfaces |> Seq.head
        | _ ->
          match c.IsNumeric, c.IsNullable with
          | Nullable.Value true, Nullable.Empty
          | Nullable.Value true, Nullable.Value false -> typeof<int32>
          | _ -> typeof<string>
      | _ -> c.Base

// *********** MODULE FUNCTIONS AS MEMBERS **********************************

type TypeInferenceContext with
  /// Gets empty type inference context.
  static member Empty
    with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = TypeInferenceContext.empty
  /// <summary>
  /// Creates new type inference context with the specified type variable.
  /// </summary>
  /// <param name="uid">Type UID to apply variable on.</param>
  /// <param name="variable">Type variable to apply.</param>
  /// <returns>New type inference context with the specified type variable.</returns>
  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.ApplyConstraint (uid, variable) =
    TypeInferenceContext.applyConstraint uid variable this
  /// <summary>
  /// Creates new type inference context with the specified substitution.
  /// </summary>
  /// <param name="a">Type UID to substitute.</param>
  /// <param name="b">Target type UID.</param>
  /// <returns>New type inference context with the specified substitution.</returns>
  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.Substitute (sourceTypeUid, targetTypeUid) =
    TypeInferenceContext.applyConstraint sourceTypeUid targetTypeUid this
  /// <summary>
  /// Collects all constaints (i.e. both owned constaints and substituted constraints) for the specified type UID.
  /// </summary>
  /// <param name="uid">Type UID to collect constraints for.</param>
  /// <returns>Type constraints for the specified type UID.</returns>
  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.GetAllConstraints typeUid =
    TypeInferenceContext.getAllConstraints typeUid this
  /// <summary>
  /// Instantiate concrete type for the specified type UID.
  /// </summary>
  /// <param name="uid">Type UID to instantiate type for.</param>
  /// <param name="ctx">Type inference context to use.</param>
  /// <returns>Concrete type for the specified type UID.</returns>
  [<Pure>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.InstantiateType typeUid =
    TypeInferenceContext.instantiateType typeUid this

// *********** LOCAL ALIASES ************************************************

type private Ctx = TypeInferenceContext

module TypeInferenceContext = TypeInferenceContext

// *********** IMPLEMENTATION ***********************************************

[<RequireQualifiedAccess>]
module internal TypeInferenceHelpers =

  [<NoEquality; NoComparison>]
  type internal NamingContext = NamingContext of Map<CaseInsensitive, struct (TypeUid * NameUid)>

  [<RequireQualifiedAccess>]
  [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
  module internal NamingContext =

    let empty = NamingContext Map.empty

    [<Pure>]
    let add key typeUid uid (NamingContext names) = Map.add (CaseInsensitive key) (struct (typeUid, uid)) names |> NamingContext

    [<Pure>]
    let tryFind key (NamingContext names) = Map.tryFind (CaseInsensitive key) names

  let private logicOperations =
    [ BinaryOperation.OrElse
      BinaryOperation.AndAlso
      BinaryOperation.GreaterThan
      BinaryOperation.GreaterThanOrEqual
      BinaryOperation.LessThan
      BinaryOperation.LessThanOrEqual
      BinaryOperation.Equal
      BinaryOperation.NotEqual ]
    |> Set.ofList

  let private numericArgOperations =
    [ BinaryOperation.GreaterThan
      BinaryOperation.GreaterThanOrEqual
      BinaryOperation.LessThan
      BinaryOperation.LessThanOrEqual
      BinaryOperation.Add
      BinaryOperation.Substract
      BinaryOperation.Multiply
      BinaryOperation.Divide
      BinaryOperation.Modulo ]
    |> Set.ofList

  let private numericResultOperations =
    [ BinaryOperation.Add
      BinaryOperation.Substract
      BinaryOperation.Multiply
      BinaryOperation.Divide
      BinaryOperation.Modulo ]
    |> Set.ofList

  [<ExcludeFromCodeCoverage>]
  let inline private mapImmutableArray f (arr : ImmutableArray<_>) =
    let length = arr.Length
    let builder = ImmutableArray.CreateBuilder length
    for i in 0 .. arr.Length - 1 do
      builder.Add <| f arr.[i]
    builder.ToImmutable ()

  [<Pure>]
  let private getName node =
    match node with
    | Identifier name -> name
    | _ -> ProtocolSyntaxException "Lambda argument supposed to be identifier" |> raise

  [<Pure>]
  [<CompiledName("Idfy")>]
  let idfy node =
    let newNameUid =
      let supply = ref 0
      fun () -> Interlocked.Increment supply |> NameUid
    let newTypeUid =
      let supply = ref 0
      fun () -> Interlocked.Increment supply |> TypeUid
    // process nodes
    let rec impl ctx node =
      match node with
      | Lambda (Identifier argName, body) ->
        let nodeTypeUid = newTypeUid ()
        let argNameUid = newNameUid ()
        let argTypeUid = newTypeUid ()
        let ctx' = NamingContext.add argName argTypeUid argNameUid ctx
        UnresolvedLambda (nodeTypeUid, UnresolvedIdentifier (argTypeUid, argNameUid), impl ctx' body)
      | Lambda _ -> ProtocolSyntaxException "Lambda argument supposed to be identifier" |> raise
      | Binary (left, op, right) ->
        UnresolvedBinary (newTypeUid (), impl ctx left, op, impl ctx right)
      | Call (name, args) ->
        UnresolvedCall (newTypeUid (), name, args |> mapImmutableArray (impl ctx))
      | Member (instance, name) ->
        UnresolvedMember (newTypeUid (), impl ctx instance, name)
      | Constant value ->
        UnresolvedConstant (newTypeUid (), value)
      | Identifier name ->
        match NamingContext.tryFind name ctx with
        | Some (struct (typeUid, nameUid)) -> UnresolvedIdentifier (typeUid, nameUid)
        | _ -> sprintf "Unable to resolve identifier name \"%s\"." name |> ProtocolTypeInferenceException |> raise
    impl NamingContext.empty node

  [<Pure>]
  [<CompiledName("CollectIds")>]
  let collectIds node =
    let rec impl acc (node : UnresolvedNode<string>) =
      Seq.fold
        (fun map node -> Map.fold (fun map k v -> Map.add k v map) map (impl acc node))
        (Map.add node.TypeUid TypeVariable.empty acc)
        (node.GetChildren ())
    { Types = impl Map.empty node; Substitutions = Map.empty }

  [<ExcludeFromCodeCoverage>]
  let inline private applyIf condition action (ctx : Ctx) =
    match condition with
    | true -> action ctx
    | _    -> ctx

  [<CompiledName("CollectConstraints")>]
  let rec collectConstraints (functionResolver : IFunctionDescriptorResolver) node ctx =
    match node with
    | UnresolvedLambda (uid, UnresolvedIdentifier (argUid, argName), body) ->
      let (ctx', body') = collectConstraints functionResolver body ctx
      let arg' = UnresolvedIdentifier (argUid, argName)
      ctx', UnresolvedLambda (uid, arg', body')
    | UnresolvedLambda _ -> ProtocolSyntaxException "Lambda argument supposed to be identifier" |> raise
    | UnresolvedBinary (uid, left, op, right) ->
      let ctx' =
        ctx
        |> applyIf (Set.contains op logicOperations)         (TypeInferenceContext.applyConstraint uid (KnownType typeof<bool>))
        |> applyIf (Set.contains op numericArgOperations)    (TypeInferenceContext.applyConstraint left.TypeUid TypeVariable.numeric >> TypeInferenceContext.applyConstraint right.TypeUid TypeVariable.numeric)
        |> applyIf (Set.contains op numericResultOperations) (TypeInferenceContext.applyConstraint uid TypeVariable.numeric)
        |> TypeInferenceContext.substitute left.TypeUid right.TypeUid
        |> TypeInferenceContext.substitute right.TypeUid left.TypeUid
      let (ctx'', l) = collectConstraints functionResolver left ctx'
      let (ctx''', r) = collectConstraints functionResolver right ctx''
      ctx''', UnresolvedBinary (uid, l, op, r)
    | UnresolvedCall (uid, name, arguments) ->
      let (ctx', resolvedArgsBuilder) =
        Seq.fold
          (fun (ctx, resolvedArgs : ImmutableArray<_>.Builder) arg ->
            let (ctx', arg') = collectConstraints functionResolver arg ctx
            resolvedArgs.Add arg'
            (ctx', resolvedArgs))
          (ctx, ImmutableArray.CreateBuilder arguments.Length)
          arguments
      let resolvedArgs = resolvedArgsBuilder.ToImmutable ()
      let resConstraints = TypeInferenceContext.getAllConstraints uid ctx'
      let argConstraints = arguments |> Seq.mapToArray (fun node -> TypeInferenceContext.getAllConstraints node.TypeUid ctx')
      match functionResolver.ResolveFunction (name, resConstraints, argConstraints) with
      | null ->
        sprintf "Unable to resolve function call with (Name = %s, Result = %A, Args = %A)" name resConstraints argConstraints
        |> ProtocolTypeInferenceException
        |> raise
      | desc ->
        let ctx'' =
          TypeInferenceContext.applyConstraint uid (KnownType desc.ResultType) ctx'
          |> Seq.foldBack
              (fun (node : UnresolvedNode<_>, ty) -> TypeInferenceContext.applyConstraint node.TypeUid (KnownType ty))
              (Seq.zip arguments desc.ArgumentTypes)
        ctx'', UnresolvedCall (uid, desc, resolvedArgs)
    | UnresolvedMember (uid, instance, name) ->
      let (ctx', instance') = collectConstraints functionResolver instance ctx
      match TypeInferenceContext.getAllConstraints instance'.TypeUid ctx' with
      | UnknownType _ ->
        TypeInferenceContext.applyConstraint instance'.TypeUid (TypeVariable.hasMember name) ctx', UnresolvedMember (uid, instance', name)
      | KnownType instanceType ->
        match Members.getMember name instanceType with
        | NoMember ->
          sprintf "Type %A has no member %s" instanceType name |> ProtocolTypeInferenceException |> raise
        | PropertyMember p ->
          TypeInferenceContext.applyConstraint uid (KnownType p.PropertyType) ctx', UnresolvedMember (uid, instance', name)
        | FieldMember f ->
          TypeInferenceContext.applyConstraint uid (KnownType f.FieldType) ctx', UnresolvedMember (uid, instance', name)
    | UnresolvedConstant (uid, null) -> TypeInferenceContext.applyConstraint uid TypeVariable.nullable ctx, UnresolvedConstant (uid, null)
    | UnresolvedConstant (uid, value) -> ctx, UnresolvedConstant (uid, value)
    | UnresolvedIdentifier (uid, name) -> ctx, UnresolvedIdentifier (uid, name)

  let collectConstraintsRoot rootType functionResolver node ctx =
    let ctx' =
      match node with
      | UnresolvedLambda (_, arg, _) -> TypeInferenceContext.applyConstraint arg.TypeUid (KnownType rootType) ctx
      | _                            -> ctx
    collectConstraints functionResolver node ctx'

  [<CompiledName("Resolve")>]
  let resolve rootType node ctx =
    let ctx' =
      match node with
      | UnresolvedLambda (_, arg, _) -> TypeInferenceContext.applyConstraint arg.TypeUid (KnownType rootType) ctx
      | _                            -> ctx
    UnresolvedNode.resolve (fun typeUid -> TypeInferenceContext.instantiateType typeUid ctx') node

/// Default type inferrer for data query protocol.
type TypeInferrer =
  val private functionResolver : IFunctionDescriptorResolver
  /// Gets function resolver.
  member this.FunctionDescriptorResolver = this.functionResolver
  /// <summary>
  /// Initializes new instance of type inferrer with the specified function resolver.
  /// </summary>
  /// <param name="functionResolver">Function resolver to be used.</param>
  new (functionResolver) = { functionResolver = functionResolver }
  /// <summary>
  /// Performs type inference on the specified AST with respect to the root lambda argument type.
  /// </summary>
  /// <param name="rootType">Type of the root lambda argument.</param>
  /// <param name="expr">AST to perform type inference on.</param>
  /// <returns>Type resolved AST.</returns>
  member this.InferTypes (rootType, expr) =
    let exprX = TypeInferenceHelpers.idfy expr
    let initialContext = TypeInferenceHelpers.collectIds exprX
    let (context, unresolvedExpr) = TypeInferenceHelpers.collectConstraintsRoot rootType this.functionResolver exprX initialContext
    TypeInferenceHelpers.resolve rootType unresolvedExpr context

  interface ITypeInferrer with
    member this.InferTypes (rootType, expr) = this.InferTypes (rootType, expr)

