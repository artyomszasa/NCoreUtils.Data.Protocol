namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Concurrent
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.TypeInference

[<Sealed>]
type ConstantBox<'a> (value : 'a) =
  member val Value = value
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

and private BoxedConstantBuilder<'a> () =
  inherit BoxedConstantBuilder ()
  static let property = typeof<ConstantBox<'a>>.GetProperty("Value", BindingFlags.Instance ||| BindingFlags.Public)
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

  let rec private createExpression (ps : Map<_, ParameterExpression>) node =
    match node with
    | ResolvedConstant (ty, null) ->
      let value =
        match ty.IsValueType with
        | true ->
          match isNullable ty with
          | true -> Activator.CreateInstance ty
          | _    -> failwith "null value cannot be used with value types"
        | _ -> null
      BoxedConstantBuilder.BuildExpression (value, ty)
    | ResolvedConstant (ty, value) ->
      BoxedConstantBuilder.BuildExpression (Convert.ChangeType (value, ty), ty)
    | ResolvedIdentifier (ty, uid) ->
      match Map.tryFind uid ps with
      | Some expr -> expr :> Expression
      | None      -> failwithf "No parameter for %A" uid
    | ResolvedCall (_, desc, args) ->
      let args' = Seq.mapToArray (createExpression ps) args
      desc.CreateExpression args'
    | ResolvedMember (_, instance, name) ->
      let inst = createExpression ps instance
      match Members.getMember name inst.Type with
      | NoMember         -> failwithf "Type %A has no member %s" inst.Type name
      | PropertyMember p -> Expression.Property (inst, p) :> _
      | FieldMember f    -> Expression.Field    (inst, f) :> _
    | ResolvedBinary (_, left, op, right) ->
      let l = createExpression ps left
      let r = createExpression ps right
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
      let arg = Expression.Parameter ty
      let body' = createExpression (Map.add uid arg ps) body
      Expression.Lambda (body', arg) :> _

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toExpression node = createExpression Map.empty node

type DataQueryExpressionBuilder =
  val private parser   : IDataQueryParser
  val private inferrer : ITypeInferrer

  member this.Parser = this.parser

  member this.Inferrer = this.inferrer

  new (parser, inferrer) =
    if isNull (box parser)   then ArgumentNullException "parser"   |> raise
    if isNull (box inferrer) then ArgumentNullException "inferrer" |> raise
    { parser   = parser
      inferrer = inferrer }

  member this.BuildExpression (rootType, input) =
    let expression = this.Parser.ParseQuery input
    this.Inferrer.InferTypes (rootType, expression)
    |> toExpression

  interface IDataQueryExpressionBuilder with
    member this.BuildExpression (rootType, input) =
      match this.BuildExpression (rootType, input) with
      | :? LambdaExpression as lambda -> lambda
      | expr -> ArgumentException (sprintf "Specified input defines non-lambda expression %A" expr, "input") |> raise
