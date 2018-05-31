namespace NCoreUtils.Data.Protocol

open System
open System.ComponentModel
open System.Linq.Expressions
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.Internal
open NCoreUtils.Data.Protocol.TypeInference

type DataQueryExpressionBuilder (parser : IDataQueryParser, typeInferer : ITypeInferer, callInfoResolver : ICallInfoResolver) =

  static let isNullable (ty : Type) = ty.IsConstructedGenericType && ty.GetGenericTypeDefinition () = typedefof<Nullable<_>>


  static let rec toExpression resolveCall root (node : NodeX<Type>) =
    match node.Node with
    | ConstantX null ->
      match node.Data.IsValueType with
      | true ->
        match isNullable node.Data with
        | true -> Expression.Constant (Activator.CreateInstance node.Data, node.Data) :> Expression
        | _    -> failwith "null value cannot be used with value types"
      | _ ->
        Expression.Constant (null, node.Data) :> Expression
    | ConstantX value ->
      let converter = TypeDescriptor.GetConverter node.Data
      Expression.Constant (converter.ConvertFrom value, node.Data) :> Expression
    | IdentifierX name ->
      Expression.Property (root, name) :> Expression
    | BinaryX (left, op, right) ->
      let l = toExpression resolveCall root left
      let r = toExpression resolveCall root right
      match op with
      | BinaryOperation.AndAlso            -> Expression.AndAlso            (l, r)
      | BinaryOperation.OrElse             -> Expression.OrElse             (l, r)
      | BinaryOperation.Equal              -> Expression.Equal              (l, r)
      | BinaryOperation.NotEqual           -> Expression.NotEqual           (l, r)
      | BinaryOperation.GreaterThan        -> Expression.GreaterThan        (l, r)
      | BinaryOperation.GreaterThanOrEqual -> Expression.GreaterThanOrEqual (l, r)
      | BinaryOperation.LessThan           -> Expression.LessThan           (l, r)
      | BinaryOperation.LessThanOrEqual    -> Expression.LessThanOrEqual    (l, r)
      | _                                  -> failwithf "Unsupported operation %A" op
      :> Expression
    | CallX (name, args) ->
      let (call : ICallInfo) = resolveCall (struct (name, args.Length))
      let args' = args |> Seq.map (toExpression resolveCall root) |> Seq.toArray
      call.CreateExpression (args')

  static let toLambdaExpression resolveCall rootType node =
    let arg = Expression.Parameter rootType
    Expression.Lambda (toExpression resolveCall arg node, arg)


  interface IDataQueryExpressionBuilder with
    member __.BuildExpression (rootType, input) =
      let ast  = parser.ParseQuery input
      let ast' = typeInferer.InferTypes (ast, rootType)
      toLambdaExpression (fun (struct (name, argNum)) -> callInfoResolver.ResolveCall (name, argNum)) rootType ast'
