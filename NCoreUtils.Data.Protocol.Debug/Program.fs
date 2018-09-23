open NCoreUtils.Data
open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Antlr4.Runtime
open NCoreUtils.Data.Protocol

type X () =
  member val Id   =  2  with get, set
  member val Name = "2" with get, set

type AstNode =
  | Lambda of string * AstNode
  | Var    of string []
  | Value  of string
  | Binar  of AstNode * string * AstNode

[<EntryPoint>]
let main _ =

  let serviceCheck () =
    let services =
      ServiceCollection()
        .AddLogging(fun builder -> builder.SetMinimumLevel(LogLevel.Trace).ClearProviders().AddConsole() |> ignore)
        .AddDataQueryServices()
        .BuildServiceProvider()
    try
      use scope = services.CreateScope ()
      let serviceProvider = scope.ServiceProvider
      let expressionBuilder = serviceProvider.GetRequiredService<IDataQueryExpressionBuilder>()
      let inputs =
        [ "x > 2"
          "length(name) > 2"
          "(x > 30) || (x < 10) || (x = 20)"
          "((x > 30) || (x < 10)) || (x = 20)"
          "(x > 30) || ((x < 10) || (x = 20))"
          "name = null || length(name) > 2"
          "name = null || contains(name, \"xy\")" ]
      for input in inputs do
        let lambda = expressionBuilder.BuildExpression<X> input
        printfn "%A => %A" input lambda
    finally
      match box services with
      | :? IDisposable as d -> d.Dispose ()
      | _                   -> ()

  let debugAntlr4 () =
    let stream = AntlrInputStream ("x => x.data > 2")
    let lexer = ProtocolLexer stream
    let tokenStream = CommonTokenStream lexer
    // tokenStream.Fill()
    // tokenStream.GetTokens(0, tokenStream.Size - 1) |> Seq.iter (printfn "%A")
    let parser = ProtocolParser tokenStream
    let ctx = parser.start()

    let (|IsExprInParens|_|) (context : ProtocolParser.ExprContext) =
      match context.expr () with
      | null               -> None
      | [| exprInParens |] -> Some exprInParens
      | _                  -> None

    let (|IsBinOp|_|) (context : ProtocolParser.ExprContext) =
      match context.expr () with
      | null               -> None
      | [| left; right |]  ->
        match context.binOp with
        | null             -> None
        | binOp            -> Some (left, binOp.Text, right)
      | _                  -> None

    ctx.Accept
      { new ProtocolBaseVisitor<AstNode>() with
          // override this.VisitStart context = context.lambda () |> this.Visit
          // override this.VisitLambda context =
          //   let arg = context.IDENT().Symbol.Text
          //   let body = context.expr () |> this.Visit
          //   Lambda (arg, body)
          // // override this.VisitExpr context =
          // //   this.VisitChildren (context)
          // override this.VisitValueexpr context =
          //   // ProtocolParser.ValueexprContext. ValueexprContext
          //   let istate = context.invokingState
          //   let ctx = ProtocolParser.ValueexprContext.GetChildContext(context, istate)
          //   this.VisitChildren context
          // override __.VisitValue context = Unchecked.defaultof<_>
          // override this.VisitConstant context =
          //   let child = context.children.[0]
          //   child.
          //   Unchecked.defaultof<_>
          override this.VisitStart context = this.VisitLambda <| context.lambda ()
          override this.VisitLambda context =
            let arg = context.IDENT().Symbol.Text
            let body = this.VisitExpr <| context.expr ()
            Lambda (arg, body)
          override this.VisitExpr context =
            match context with
            | IsExprInParens inner -> this.VisitExpr inner
            | IsBinOp (left, binOp, right) ->
              let l = this.VisitExpr left
              let r = this.VisitExpr right
              Binar (l, binOp, r)
            | _ -> base.VisitExpr context
          override __.VisitIdent context =
            let idents = context.IDENT ()
            idents |> Array.map (fun id -> id.Symbol.Text) |> Var
          override __.VisitNumValue    context = context.NUMVALUE().Symbol.Text |> Value
          override __.VisitStringValue context = context.STRINGVALUE().Symbol.Text |> Value
      }
      |> printfn "%A"


    ()

  let debugAntlr4Full () =
    let services =
      ServiceCollection()
        .AddDataQueryServices()
        .BuildServiceProvider()
    // let qp = NCoreUtils.Data.Protocol.DataQueryParser ()
    // let qt = NCoreUtils.Data.Protocol.TypeInference.TypeInferrer ()
    // let qb = DataQueryExpressionBuilder (qp, qt)

    use scope = services.CreateScope ()
    let qb = scope.ServiceProvider.GetRequiredService<IDataQueryExpressionBuilder> ()

    qb.BuildExpression (typeof<X>, "x => length(lower(x.name)) < 5 && (x.id - 2 > 2 || x.name = \"alma\" || contains(x.name, \"körte\"))") |> printfn "%A"

    ()

  debugAntlr4Full ()

  // let qparser = DataQueryParser () :> IDataQueryParser
  // let q0 = qparser.ParseQuery "x > 2"
  // let q1 = qparser.ParseQuery "length(name) > 2"
  // let q2 = qparser.ParseQuery "name = null || length(name) > 2"

  // let ctx =
  //   let sup = ref 0L
  //   { RootType    = typeof<X>
  //     NewVar      = fun () -> Interlocked.Increment sup |> TVar
  //     ResolveCall = fun name ->
  //                     match StringComparer.InvariantCultureIgnoreCase.Equals (name, fnLength.Name) with
  //                     | true -> fnLength
  //                     | _    -> null
  //    }

  // let x0 = collect ctx (ctx.NewVar ()) (Subss []) q0 ||> resolve
  // let x1 = collect ctx (ctx.NewVar ()) (Subss []) q1 ||> resolve
  // let x2 = collect ctx (ctx.NewVar ()) (Subss []) q2 ||> resolve
  // printfn "%A" x0
  // printfn "%A" x1
  // printfn "%A" x2

  // let e0 = toLambdaExpression ctx.ResolveCall ctx.RootType x0
  // printfn "%A" e0
  // let e1 = toLambdaExpression ctx.ResolveCall ctx.RootType x1
  // printfn "%A" e1
  // let e2 = toLambdaExpression ctx.ResolveCall ctx.RootType x2
  // printfn "%A" e2

  0
