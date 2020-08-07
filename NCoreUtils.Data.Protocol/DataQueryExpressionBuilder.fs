namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Concurrent
open System.Collections.Immutable
open System.Diagnostics.CodeAnalysis
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.TypeInference

[<Sealed>]
type private ConstantBox<'a> (value : 'a) =
  member val Value = value
  [<ExcludeFromCodeCoverage>]
  override this.ToString () = sprintf "cbox(%A)" this.Value

[<AbstractClass>]
type private BoxedConstantBuilder () =
  static let cache = ConcurrentDictionary<Type, BoxedConstantBuilder> ()
  static let mkBuilder =
    Func<_, _> (fun (ty : Type) -> Activator.CreateInstance (typedefof<BoxedConstantBuilder<_>>.MakeGenericType ty, true) :?> BoxedConstantBuilder)
  abstract BuildExpression : value:obj -> Expression
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member BuildExpression (value : obj, ty) =
    cache.GetOrAdd(ty, mkBuilder).BuildExpression value

and [<Sealed>] private BoxedConstantBuilder<'a> () =
  inherit BoxedConstantBuilder ()
  let property = typeof<ConstantBox<'a>>.GetProperty("Value", BindingFlags.Instance ||| BindingFlags.NonPublic)
  override __.BuildExpression value =
    let v = value :?> 'a
    let valueBox = ConstantBox v
    Expression.Property (
      Expression.Constant (valueBox, typeof<ConstantBox<'a>>),
      property
    ) :> _

[<AutoOpen>]
module private DataQueryExpressionBuilderHelpers =

  let inline private isNullable (ty : Type) = ty.IsConstructedGenericType && ty.GetGenericTypeDefinition () = typedefof<Nullable<_>>

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private changeType (value: string) (target: Type) =
    match target with
    | t when t = typeof<Guid> ->
      match value with
      | null | "" -> Guid.Empty       :> obj
      | _         -> Guid.Parse value :> obj
    | _ ->
      // default
      Convert.ChangeType (value, target, Globalization.CultureInfo.InvariantCulture)

  let rec private createExpression (propertyResolver : IPropertyResolver) (ps : Map<_, ParameterExpression>) node =
    match node with
    | ResolvedConstant (ty, null) ->
      match ty.IsValueType && not (isNullable ty) with
      | true -> failwith "null value cannot be used with value types"
      | _    -> Expression.Constant (null, ty) :> Expression
    | ResolvedConstant (ty, value) ->
      match ty.IsEnum with
      | true ->
        match tryInt64Value value with
        | ValueSome i64 ->
          let value = Convert.ChangeType (i64, Enum.GetUnderlyingType ty)
          BoxedConstantBuilder.BuildExpression (Enum.ToObject (ty, value), ty)
        | _ ->
          let enumValue = Enum.Parse (ty, value, true)
          BoxedConstantBuilder.BuildExpression (enumValue, ty)
      | _ ->
      match isNullable ty with
      | true ->
        let realType = ty.GetGenericArguments().[0]
        let value = changeType value realType
        let box = BoxedConstantBuilder.BuildExpression (value, realType)
        Expression.Convert(box, ty) :> _
      | _ ->
        BoxedConstantBuilder.BuildExpression (changeType value ty, ty)
    | ResolvedIdentifier (_, uid) ->
      match Map.tryFind uid ps with
      | Some expr -> expr :> Expression
      | None      -> failwithf "No parameter for %A" uid
    | ResolvedCall (_, desc, args) ->
      let args' = Seq.mapToArray (createExpression propertyResolver ps) args
      desc.CreateExpression args'
    | ResolvedMember (_, instance, name) ->
      let inst = createExpression propertyResolver ps instance
      match Members.getMember name inst.Type with
      | PropertyMember p -> Expression.Property (inst, p) :> _
      | FieldMember f    -> Expression.Field    (inst, f) :> _
      | NoMember         ->
      match propertyResolver.TryResolve (inst.Type, name) with
      | ValueNone        -> failwithf "Type %A has no member %s" inst.Type name
      | ValueSome p      -> p.CreateExpression inst
    | ResolvedBinary (_, left, op, right) ->
      let l = createExpression propertyResolver ps left
      let r = createExpression propertyResolver ps right
      match op with
      | BinaryOperation.AndAlso            -> Expression.AndAlso            (l, r) :> _
      | BinaryOperation.OrElse             -> Expression.OrElse             (l, r) :> _
      | BinaryOperation.Equal              -> Expression.Equal              (l, r) :> _
      | BinaryOperation.NotEqual           -> Expression.NotEqual           (l, r) :> _
      | BinaryOperation.GreaterThan        -> Expression.GreaterThan        (l, r) :> _
      | BinaryOperation.GreaterThanOrEqual -> Expression.GreaterThanOrEqual (l, r) :> _
      | BinaryOperation.LessThan           -> Expression.LessThan           (l, r) :> _
      | BinaryOperation.LessThanOrEqual    -> Expression.LessThanOrEqual    (l, r) :> _
      | BinaryOperation.Add                -> Expression.Add                (l, r) :> _
      | BinaryOperation.Substract          -> Expression.Subtract           (l, r) :> _
      | BinaryOperation.Multiply           -> Expression.Multiply           (l, r) :> _
      | BinaryOperation.Divide             -> Expression.Divide             (l, r) :> _
      | BinaryOperation.Modulo             -> Expression.Modulo             (l, r) :> _
      | _ -> failwithf "WTF op (%A)" op
    | ResolvedLambda (_, arg, body) ->
      let (ty, uid) =
        match arg with
        | ResolvedIdentifier (ty, uid) -> (ty, uid)
        | _ -> failwith "WTF param"
      let arg = Expression.Parameter (ty, "e" + uid.RawDisplayString)
      let body' = createExpression propertyResolver (Map.add uid arg ps) body
      Expression.Lambda (body', arg) :> _

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let mapImmutableArray f (arr : ImmutableArray<_>) =
    let builder = ImmutableArray.CreateBuilder arr.Length
    for i = 0 to (arr.Length - 1) do
      builder.Add (f arr.[i])
    builder.ToImmutable ()

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toExpression propertyResolver node = createExpression propertyResolver Map.empty node

/// <summary>
/// Default data query expression builder. Uses provided data query parser and type inferrer to parse and process query
/// creating LINQ expression as the result.
/// </summary>
type DataQueryExpressionBuilder =
  val private parser   : IDataQueryParser
  val private inferrer : ITypeInferrer
  val private logger   : ILogger
  /// Gets the data query parser used to parse queries.
  member this.Parser with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.parser
  /// Gets type inferrer used to infer types in parsed query.
  member this.Inferrer with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get () = this.inferrer
  /// <summary>
  /// Initializes new instance of default data query expression builder with the specified data query parser and type
  /// inferrer.
  /// </summary>
  /// <param name="parser">Data query parser to use.</param>
  /// <param name="inferrer">Type inferrer to use.</param>
  new (parser, inferrer, logger : ILogger<DataQueryExpressionBuilder>) =
    if isNull (box parser)   then nullArg "parser"
    if isNull (box inferrer) then nullArg "inferrer"
    if isNull  logger        then nullArg "logger"
    { parser   = parser
      inferrer = inferrer
      logger   = logger }

  member private this.AdaptLegacy (node : Node) =
    // expression is legacy expression if its root node is not a lambda
    match node with
    | Node.Lambda _ -> node
    | _ ->
      this.logger.LogDebug "Root node is not a lambda, assuming legacy node."
      let rec adapt (rootArg : Node) (node : Node) =
        match node with
        | Node.Binary (l, op, r) -> Node.Binary (adapt rootArg l, op, adapt rootArg r)
        | Node.Constant _        -> node
        | Node.Call (name, args) -> Node.Call (name, mapImmutableArray (adapt rootArg) args)
        | Node.Identifier (name) -> Node.Member (rootArg, name)
        | _                      -> notImplementdf "Node %A is not valid in legacy mode" node
      let arg = Node.Identifier "__generated"
      let body = adapt arg node
      Node.Lambda (arg, body)

  /// <summary>
  /// Parses and processes specified query creating LINQ expression with respect to the root argument type.
  /// </summary>
  /// <param name="rootType">Type of the root argument in the expression.</param>
  /// <param name="input">Raw query to parse and process.</param>
  /// <returns>LINQ Expression representation of the input query.</returns>
  abstract BuildExpression: rootType:Type * input:string -> Expression

  /// <summary>
  /// Creates LINQ expression from the resolved internal expression.
  /// </summary>
  /// <param name="resolvedExpression">Resolved internal expression.</param>
  /// <returns>LINQ Expression representation of the resolved expression.</returns>
  abstract CreateExpression: resolvedExpression:ResolvedNode -> Expression

  /// <summary>
  /// Parses raw input creating internal expression.
  /// </summary>
  /// <param name="input">String that contains the expression to parse.</param>
  /// <returns>Internal expression.</returns>
  abstract ParseExpression: input:string -> Node

  /// <summary>
  /// Inters and validates types in the specified internal expression.
  /// </summary>
  /// <param name="rootType">Type of the root argument in the expression.</param>
  /// <param name="expression">Internal expression without type information.</param>
  /// <returns>Internal expression with resolved type information.</returns>
  abstract ResolveExpression: rootType:Type * expression:Node -> ResolvedNode

  default this.BuildExpression (rootType, input) =
    try
      let rawExpression = this.ParseExpression input
      let resolvedExpression = this.ResolveExpression (rootType, rawExpression)
      this.CreateExpression resolvedExpression
    with exn ->
      ProtocolException (sprintf "Failed to build expression for \"%s\" with root type %A" input rootType, exn)
      |> raise

  default this.CreateExpression resolvedExpression =
    toExpression this.inferrer.PropertyResolver resolvedExpression

  default this.ParseExpression input =
    this.Parser.ParseQuery input |> this.AdaptLegacy

  default this.ResolveExpression (rootType, expression) =
    this.Inferrer.InferTypes (rootType, expression)

  interface IDataQueryExpressionBuilder with
    member this.BuildExpression (rootType, input) =
      match this.BuildExpression (rootType, input) with
      | :? LambdaExpression as lambda -> lambda
      | expr -> ArgumentException (sprintf "Specified input defines non-lambda expression %A" expr, "input") |> raise
