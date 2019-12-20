namespace NCoreUtils.Data.Protocol

open System.Collections.Immutable
open System.Runtime.CompilerServices
open Antlr4.Runtime
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data

[<AutoOpen>]
module private DataQueryParserHelpers =

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private (|IsExprInParens|_|) (context : ProtocolParser.ExprContext) =
    match context.expr () with
    | null               -> None
    | [| exprInParens |] -> Some exprInParens
    | _                  -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private (|IsBinOp|_|) (context : ProtocolParser.ExprContext) =
    match context.expr () with
    | null               -> None
    | [| left; right |]  ->
      match context.binOp with
      | null             -> None
      | binOp            -> Some (left, binOp.Text, right)
    | _                  -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private parseBinOp source =
    match source with
    | "&&" -> BinaryOperation.AndAlso
    | "||" -> BinaryOperation.OrElse
    | "="  -> BinaryOperation.Equal
    | "!=" -> BinaryOperation.NotEqual
    | "<"  -> BinaryOperation.LessThan
    | ">"  -> BinaryOperation.GreaterThan
    | "<=" -> BinaryOperation.LessThanOrEqual
    | ">=" -> BinaryOperation.GreaterThanOrEqual
    | "+"  -> BinaryOperation.Add
    | "-"  -> BinaryOperation.Substract
    | "/"  -> BinaryOperation.Divide
    | "*"  -> BinaryOperation.Multiply
    | "%"  -> BinaryOperation.Modulo
    | text ->
      sprintf "Invalid binary operator %s" text
      |> ProtocolSyntaxException
      |> raise

  let antl4NodeVisitor =
    { new ProtocolBaseVisitor<Node>() with
        override this.VisitStart context =
          match context.lambda () with
          | null   -> this.VisitExpr   <| context.expr ()
          | lambda -> this.VisitLambda <| lambda
        override this.VisitLambda context =
          let arg = context.IDENT().Symbol.Text
          let body = this.VisitExpr <| context.expr ()
          Lambda (Identifier arg, body)
        override this.VisitExpr context =
          match context with
          | IsExprInParens inner -> this.VisitExpr inner
          | IsBinOp (left, binOp, right) ->
            let l = this.VisitExpr left
            let r = this.VisitExpr right
            Binary (l, parseBinOp binOp, r)
          | _ -> base.VisitExpr context
        override this.VisitCall context =
          let name = context.IDENT().GetText ()
          let args = context.args().expr () |> Array.map this.VisitExpr
          Call (name, args.ToImmutableArray ())
        override __.VisitIdent context =
          let idents = context.IDENT ()
          match idents |> List.ofArray with
          | [] ->
            sprintf "Zero length ident at %d (%s)" context.SourceInterval.a (context.GetText ())
            |> ProtocolSyntaxException
            |> raise
          | [ name ] ->
            match name.GetText () with
            | "null" -> Constant null
            | name -> Identifier name
          | root :: names ->
            List.fold (fun instance (name : Tree.ITerminalNode) -> Member (instance, name.GetText ())) (root.GetText () |> Identifier) names
        override __.VisitNumValue    context = context.NUMVALUE().Symbol.Text |> Constant
        // FIMXE: escaőed quotes...
        override __.VisitStringValue context = context.STRINGVALUE().Symbol.Text.Trim '"' |> Constant
    }

/// <summary>
/// Default data query parser based on Antlr4 grammar.
/// </summary>
type DataQueryParser =

  /// Initializes new instance of default data parser.
  new () = { }

  abstract ParseQuery : raw:string -> Node

  /// <summary>
  /// Parses input into internal AST using Antlr4 grammar.
  /// </summary>
  /// <param name="input">String that contains raw input.</param>
  /// <returns>Root node of parsed AST.</returns>
  /// <exception cref="NCoreUtils.Data.ProtocolException">
  /// Thrown if expression is malformed.
  /// </exception>
  default __.ParseQuery (raw : string) =
    try
      let ctx =
        let parser =
          AntlrInputStream raw
          |> ProtocolLexer
          |> CommonTokenStream
          |> ProtocolParser
        parser.start ()
      ctx.Accept antl4NodeVisitor
    with exn ->
      ProtocolSyntaxException (sprintf "Failed to parse expression: \"%s\"." raw, exn) |> raise

  interface NCoreUtils.Data.IDataQueryParser with
    member this.ParseQuery input = this.ParseQuery input
