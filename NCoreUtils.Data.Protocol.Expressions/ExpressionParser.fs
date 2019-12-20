namespace NCoreUtils.Data.Protocol

open System.Linq.Expressions
open NCoreUtils
open NCoreUtils.Data.Protocol.Ast
open System.Collections.Generic
open System.Collections.Immutable
open System.Reflection
open System.Globalization

module ValueOption = Microsoft.FSharp.Core.ValueOption

[<AutoOpen>]
module private Helpers =

  let c (node : Node) = Expression.Constant node :> Expression

  let (|CExpr|) (expression : Expression) =
    match expression with
    | :? ConstantExpression as cexpr when cexpr.Type = typeof<Node> -> cexpr.Value :?> Node
    | _ -> invalidOpf "%A is not a valid ast node." expression

  let mapToImmutable f (args : IReadOnlyList<Expression>) =
    let builder = ImmutableArray.CreateBuilder args.Count
    for e in args do
      builder.Add (f e)
    builder.ToImmutable ()

  let rec tryBoxedConst (expression : Expression) =
    match expression with
    | :? ConstantExpression as cexpr -> Some struct (cexpr.Type, cexpr.Value)
    | :? MemberExpression as mexpr ->
      match mexpr.Member with
      | :? PropertyInfo as p ->
        match p.GetMethod.IsStatic with
        | true -> Some struct (p.PropertyType, p.GetValue (null, null))
        | _ ->
          tryBoxedConst mexpr.Expression
          |> Option.map (fun (struct (_, o)) -> struct (p.PropertyType, p.GetValue (o, null)) )
      | :? FieldInfo as f ->
        match f.IsStatic with
        | true -> Some struct (f.FieldType, f.GetValue null)
        | _ ->
          tryBoxedConst mexpr.Expression
          |> Option.map (fun (struct (_, o)) -> struct (f.FieldType, f.GetValue o) )
      | _ -> None
    | _ -> None

  let (|ConstableExpression|_|) = tryBoxedConst

  let rec isHasValue (expression : MemberExpression) =
    match expression.Member.Name with
    | "HasValue" -> expression.Expression.Type.IsGenericType && expression.Expression.Type.GetGenericTypeDefinition () = typedefof<System.Nullable<_>>
    | _          -> false

  let (|NullableHasValue|_|) expression =
    if isHasValue expression then Some () else None

module ExpressionToAstVisitor =

  let private binOpMap =
    Map.ofList
      [ ExpressionType.Equal,              BinaryOperation.Equal
        ExpressionType.NotEqual,           BinaryOperation.NotEqual
        ExpressionType.LessThan,           BinaryOperation.LessThan
        ExpressionType.LessThanOrEqual,    BinaryOperation.LessThanOrEqual
        ExpressionType.GreaterThan,        BinaryOperation.GreaterThan
        ExpressionType.GreaterThanOrEqual, BinaryOperation.GreaterThanOrEqual
        ExpressionType.OrElse,             BinaryOperation.OrElse
        ExpressionType.AndAlso,            BinaryOperation.AndAlso
        ExpressionType.Add,                BinaryOperation.Add
        ExpressionType.Subtract,           BinaryOperation.Substract
        ExpressionType.Multiply,           BinaryOperation.Multiply
        ExpressionType.Divide,             BinaryOperation.Divide
        ExpressionType.Modulo,             BinaryOperation.Modulo ]

  type Context =
    private
      { Supply  : int
        Mapping : ImmutableDictionary<ParameterExpression, string> }
    with
      member this.Add (parameter : ParameterExpression) =
        let (supply', name') =
          match parameter.Name with
          | null | "" -> (this.Supply + 1, sprintf "p%d" this.Supply)
          | name      -> (this.Supply,     name)
        (name', { Supply = supply'; Mapping = this.Mapping.Add (parameter, name') })
      member this.Item
        with get parameter =
          let mutable name = Unchecked.defaultof<_>
          match this.Mapping.TryGetValue (parameter, &name) with
          | true -> name
          | _    -> invalidOpf "Parameter %A not found in the context." parameter

  module Context =

    let empty = { Supply = 0; Mapping = ImmutableDictionary.Empty }

  let rec nodeToAst (ctx : Context) (matcher : IFunctionMatcher) (node : Expression) =
    // function call may match any node.
    match matcher.MatchFunction node with
    | ValueSome { Name = name; Arguments = args } ->
      Call (name, mapToImmutable (nodeToAst ctx matcher) args)
    | _ ->
    match node with
    | ConstableExpression (_, value) ->
      match value with
      | null           -> Constant null
      | :? float   as f -> Constant (f.ToString CultureInfo.InvariantCulture)
      | :? single  as s -> Constant (s.ToString CultureInfo.InvariantCulture)
      | :? decimal as d -> Constant (d.ToString CultureInfo.InvariantCulture)
      | _               -> Constant (value.ToString ())
    | :? MemberExpression as node ->
      match isHasValue node with
      | true -> Binary (nodeToAst ctx matcher node.Expression, BinaryOperation.Equal, Constant null)
      | _    -> Member (nodeToAst ctx matcher node.Expression, node.Member.Name)
    | :? BinaryExpression as node ->
      let op = Map.tryFind node.NodeType binOpMap |> Option.defaultWith (fun () -> invalidOpf "Unsupported binary operation %A." node.NodeType)
      Binary (nodeToAst ctx matcher node.Left, op, nodeToAst ctx matcher node.Right)
    | :? ParameterExpression as node -> Identifier node.Name
    | :? LambdaExpression as lambda ->
      let rec nest (ctx : Context) index =
        match index = lambda.Parameters.Count - 1 with
        | true ->
          let (name, ctx') = ctx.Add lambda.Parameters.[index]
          Lambda (Identifier name, nodeToAst ctx' matcher lambda.Body)
        | _ ->
          let (name, ctx') = ctx.Add lambda.Parameters.[index]
          Lambda (Identifier name, nest ctx' (index + 1))
      nest ctx 0
    | :? UnaryExpression as unary ->
      match unary.NodeType with
      | ExpressionType.Not ->
        Binary (nodeToAst ctx matcher unary.Operand, BinaryOperation.Equal, Constant "false")
      | ExpressionType.Quote
      | ExpressionType.Convert -> nodeToAst ctx matcher unary.Operand
      | _ -> invalidOpf "Unsupported unary expression %A" unary
    | _ -> invalidOpf "Unsupported expression %A" node

type ExpressionParser (matcher : IFunctionMatcher) =
  member __.ParseExpression expression =
    ExpressionToAstVisitor.nodeToAst ExpressionToAstVisitor.Context.empty matcher expression