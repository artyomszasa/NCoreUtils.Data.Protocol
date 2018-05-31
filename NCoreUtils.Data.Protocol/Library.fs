namespace NCoreUtils.Data.Protocol

open Microsoft.FSharp.Text.Lexing

type DataQueryParser () =
  let filter tok =
    match tok with
    | NCoreUtils.Data.Protocol.Parser.token.IDENT "null" -> NCoreUtils.Data.Protocol.Parser.token.VALUE null
    | _ -> tok

  interface NCoreUtils.Data.IDataQueryParser with
    member __.ParseQuery input =
      LexBuffer<_>.FromString input
      |> Parser.start (Lexer.tokens >> filter)