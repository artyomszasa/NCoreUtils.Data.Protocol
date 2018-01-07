// Implementation file for parser generated by fsyacc
module NCoreUtils.Data.Protocol.Parser
#nowarn "64";; // turn off warnings that type variables used in production annotations are instantiated to concrete type
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing.ParseHelpers
# 1 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"

open NCoreUtils.Data.Protocol.Ast
open System.Collections.Immutable

# 11 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
// This type is the type of tokens accepted by the parser
type token = 
  | EOF
  | LPAREN
  | RPAREN
  | COMMA
  | OR
  | AND
  | EQ
  | NEQ
  | GT
  | GE
  | LT
  | LE
  | VALUE of (string)
  | IDENT of (string)
// This type is used to give symbolic names to token indexes, useful for error messages
type tokenId = 
    | TOKEN_EOF
    | TOKEN_LPAREN
    | TOKEN_RPAREN
    | TOKEN_COMMA
    | TOKEN_OR
    | TOKEN_AND
    | TOKEN_EQ
    | TOKEN_NEQ
    | TOKEN_GT
    | TOKEN_GE
    | TOKEN_LT
    | TOKEN_LE
    | TOKEN_VALUE
    | TOKEN_IDENT
    | TOKEN_end_of_input
    | TOKEN_error
// This type is used to give symbolic names to token indexes, useful for error messages
type nonTerminalId = 
    | NONTERM__startstart
    | NONTERM_start
    | NONTERM_expr
    | NONTERM_valueexpr
    | NONTERM_args
    | NONTERM_andor
    | NONTERM_binop

// This function maps tokens to integer indexes
let tagOfToken (t:token) = 
  match t with
  | EOF  -> 0 
  | LPAREN  -> 1 
  | RPAREN  -> 2 
  | COMMA  -> 3 
  | OR  -> 4 
  | AND  -> 5 
  | EQ  -> 6 
  | NEQ  -> 7 
  | GT  -> 8 
  | GE  -> 9 
  | LT  -> 10 
  | LE  -> 11 
  | VALUE _ -> 12 
  | IDENT _ -> 13 

// This function maps integer indexes to symbolic token ids
let tokenTagToTokenId (tokenIdx:int) = 
  match tokenIdx with
  | 0 -> TOKEN_EOF 
  | 1 -> TOKEN_LPAREN 
  | 2 -> TOKEN_RPAREN 
  | 3 -> TOKEN_COMMA 
  | 4 -> TOKEN_OR 
  | 5 -> TOKEN_AND 
  | 6 -> TOKEN_EQ 
  | 7 -> TOKEN_NEQ 
  | 8 -> TOKEN_GT 
  | 9 -> TOKEN_GE 
  | 10 -> TOKEN_LT 
  | 11 -> TOKEN_LE 
  | 12 -> TOKEN_VALUE 
  | 13 -> TOKEN_IDENT 
  | 16 -> TOKEN_end_of_input
  | 14 -> TOKEN_error
  | _ -> failwith "tokenTagToTokenId: bad token"

/// This function maps production indexes returned in syntax errors to strings representing the non terminal that would be produced by that production
let prodIdxToNonTerminal (prodIdx:int) = 
  match prodIdx with
    | 0 -> NONTERM__startstart 
    | 1 -> NONTERM_start 
    | 2 -> NONTERM_start 
    | 3 -> NONTERM_expr 
    | 4 -> NONTERM_expr 
    | 5 -> NONTERM_expr 
    | 6 -> NONTERM_valueexpr 
    | 7 -> NONTERM_valueexpr 
    | 8 -> NONTERM_valueexpr 
    | 9 -> NONTERM_args 
    | 10 -> NONTERM_args 
    | 11 -> NONTERM_andor 
    | 12 -> NONTERM_andor 
    | 13 -> NONTERM_binop 
    | 14 -> NONTERM_binop 
    | 15 -> NONTERM_binop 
    | 16 -> NONTERM_binop 
    | 17 -> NONTERM_binop 
    | 18 -> NONTERM_binop 
    | _ -> failwith "prodIdxToNonTerminal: bad production index"

let _fsyacc_endOfInputTag = 16 
let _fsyacc_tagOfErrorTerminal = 14

// This function gets the name of a token as a string
let token_to_string (t:token) = 
  match t with 
  | EOF  -> "EOF" 
  | LPAREN  -> "LPAREN" 
  | RPAREN  -> "RPAREN" 
  | COMMA  -> "COMMA" 
  | OR  -> "OR" 
  | AND  -> "AND" 
  | EQ  -> "EQ" 
  | NEQ  -> "NEQ" 
  | GT  -> "GT" 
  | GE  -> "GE" 
  | LT  -> "LT" 
  | LE  -> "LE" 
  | VALUE _ -> "VALUE" 
  | IDENT _ -> "IDENT" 

// This function gets the data carried by a token as an object
let _fsyacc_dataOfToken (t:token) = 
  match t with 
  | EOF  -> (null : System.Object) 
  | LPAREN  -> (null : System.Object) 
  | RPAREN  -> (null : System.Object) 
  | COMMA  -> (null : System.Object) 
  | OR  -> (null : System.Object) 
  | AND  -> (null : System.Object) 
  | EQ  -> (null : System.Object) 
  | NEQ  -> (null : System.Object) 
  | GT  -> (null : System.Object) 
  | GE  -> (null : System.Object) 
  | LT  -> (null : System.Object) 
  | LE  -> (null : System.Object) 
  | VALUE _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
  | IDENT _fsyacc_x -> Microsoft.FSharp.Core.Operators.box _fsyacc_x 
let _fsyacc_gotos = [| 0us; 65535us; 2us; 65535us; 0us; 1us; 3us; 4us; 6us; 65535us; 0us; 2us; 3us; 2us; 6us; 7us; 11us; 9us; 14us; 10us; 18us; 10us; 6us; 65535us; 0us; 12us; 3us; 12us; 6us; 12us; 11us; 12us; 14us; 12us; 18us; 12us; 2us; 65535us; 14us; 15us; 18us; 19us; 1us; 65535us; 2us; 3us; 4us; 65535us; 2us; 11us; 7us; 11us; 9us; 11us; 10us; 11us; |]
let _fsyacc_sparseGotoTableRowOffsets = [|0us; 1us; 4us; 11us; 18us; 21us; 23us; |]
let _fsyacc_stateToProdIdxsTableElements = [| 1us; 0us; 1us; 0us; 3us; 1us; 2us; 4us; 1us; 1us; 1us; 1us; 1us; 2us; 1us; 3us; 2us; 3us; 4us; 1us; 3us; 2us; 4us; 4us; 3us; 4us; 9us; 10us; 1us; 4us; 1us; 5us; 2us; 6us; 7us; 1us; 6us; 1us; 6us; 1us; 6us; 1us; 8us; 1us; 9us; 1us; 9us; 1us; 11us; 1us; 12us; 1us; 13us; 1us; 14us; 1us; 15us; 1us; 16us; 1us; 17us; 1us; 18us; |]
let _fsyacc_stateToProdIdxsTableRowOffsets = [|0us; 2us; 4us; 8us; 10us; 12us; 14us; 16us; 19us; 21us; 24us; 28us; 30us; 32us; 35us; 37us; 39us; 41us; 43us; 45us; 47us; 49us; 51us; 53us; 55us; 57us; 59us; 61us; |]
let _fsyacc_action_rows = 28
let _fsyacc_actionTableElements = [|3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 0us; 49152us; 9us; 32768us; 0us; 5us; 4us; 21us; 5us; 20us; 6us; 22us; 7us; 23us; 8us; 24us; 9us; 26us; 10us; 25us; 11us; 27us; 3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 0us; 16385us; 0us; 16386us; 3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 7us; 32768us; 2us; 8us; 6us; 22us; 7us; 23us; 8us; 24us; 9us; 26us; 10us; 25us; 11us; 27us; 0us; 16387us; 6us; 16388us; 6us; 22us; 7us; 23us; 8us; 24us; 9us; 26us; 10us; 25us; 11us; 27us; 7us; 16394us; 3us; 18us; 6us; 22us; 7us; 23us; 8us; 24us; 9us; 26us; 10us; 25us; 11us; 27us; 3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 0us; 16389us; 1us; 16391us; 1us; 14us; 3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 1us; 32768us; 2us; 16us; 0us; 16390us; 0us; 16392us; 3us; 32768us; 1us; 6us; 12us; 17us; 13us; 13us; 0us; 16393us; 0us; 16395us; 0us; 16396us; 0us; 16397us; 0us; 16398us; 0us; 16399us; 0us; 16400us; 0us; 16401us; 0us; 16402us; |]
let _fsyacc_actionTableRowOffsets = [|0us; 4us; 5us; 15us; 19us; 20us; 21us; 25us; 33us; 34us; 41us; 49us; 53us; 54us; 56us; 60us; 62us; 63us; 64us; 68us; 69us; 70us; 71us; 72us; 73us; 74us; 75us; 76us; |]
let _fsyacc_reductionSymbolCounts = [|1us; 3us; 2us; 3us; 3us; 1us; 4us; 1us; 1us; 3us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; 1us; |]
let _fsyacc_productionToNonTerminalTable = [|0us; 1us; 1us; 2us; 2us; 2us; 3us; 3us; 3us; 4us; 4us; 5us; 5us; 6us; 6us; 6us; 6us; 6us; 6us; |]
let _fsyacc_immediateActions = [|65535us; 49152us; 65535us; 65535us; 16385us; 16386us; 65535us; 65535us; 16387us; 65535us; 65535us; 65535us; 16389us; 65535us; 65535us; 65535us; 16390us; 16392us; 65535us; 16393us; 16395us; 16396us; 16397us; 16398us; 16399us; 16400us; 16401us; 16402us; |]
let _fsyacc_reductions ()  =    [| 
# 168 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data :  Node )) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
                      raise (Microsoft.FSharp.Text.Parsing.Accept(Microsoft.FSharp.Core.Operators.box _1))
                   )
                 : '_startstart));
# 177 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'andor)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data :  Node )) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 21 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                            Binary (_1, _2, _3) 
                   )
# 21 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 :  Node ));
# 190 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 22 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                    _1 
                   )
# 22 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 :  Node ));
# 201 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 25 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                              _2 
                   )
# 25 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'expr));
# 212 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            let _2 = (let data = parseState.GetInput(2) in (Microsoft.FSharp.Core.Operators.unbox data : 'binop)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 26 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                           Binary (_1, _2, _3) 
                   )
# 26 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'expr));
# 225 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'valueexpr)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 27 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                     _1 
                   )
# 27 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'expr));
# 236 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'args)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 30 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                                    Call (_1, ImmutableArray.CreateRange _3) 
                   )
# 30 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'valueexpr));
# 248 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 31 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                 Identifier _1 
                   )
# 31 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'valueexpr));
# 259 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : string)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 32 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                 Constant _1 
                   )
# 32 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'valueexpr));
# 270 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            let _3 = (let data = parseState.GetInput(3) in (Microsoft.FSharp.Core.Operators.unbox data : 'args)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 35 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                           _1 :: _3 
                   )
# 35 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'args));
# 282 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            let _1 = (let data = parseState.GetInput(1) in (Microsoft.FSharp.Core.Operators.unbox data : 'expr)) in
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 36 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                [ _1 ] 
                   )
# 36 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'args));
# 293 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 39 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.AndAlso 
                   )
# 39 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'andor));
# 303 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 40 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.OrElse 
                   )
# 40 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'andor));
# 313 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 43 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.Equal 
                   )
# 43 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
# 323 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 44 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.NotEqual 
                   )
# 44 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
# 333 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 45 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.GreaterThan 
                   )
# 45 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
# 343 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 46 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.LessThan 
                   )
# 46 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
# 353 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 47 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.GreaterThanOrEqual 
                   )
# 47 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
# 363 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
        (fun (parseState : Microsoft.FSharp.Text.Parsing.IParseState) ->
            Microsoft.FSharp.Core.Operators.box
                (
                   (
# 48 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                                BinaryOperation.LessThanOrEqual 
                   )
# 48 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fsy"
                 : 'binop));
|]
# 374 "/home/artyom/Projects/NCoreUtils.Data.Protocol/NCoreUtils.Data.Protocol/Parser.fs"
let tables () : Microsoft.FSharp.Text.Parsing.Tables<_> = 
  { reductions= _fsyacc_reductions ();
    endOfInputTag = _fsyacc_endOfInputTag;
    tagOfToken = tagOfToken;
    dataOfToken = _fsyacc_dataOfToken; 
    actionTableElements = _fsyacc_actionTableElements;
    actionTableRowOffsets = _fsyacc_actionTableRowOffsets;
    stateToProdIdxsTableElements = _fsyacc_stateToProdIdxsTableElements;
    stateToProdIdxsTableRowOffsets = _fsyacc_stateToProdIdxsTableRowOffsets;
    reductionSymbolCounts = _fsyacc_reductionSymbolCounts;
    immediateActions = _fsyacc_immediateActions;
    gotos = _fsyacc_gotos;
    sparseGotoTableRowOffsets = _fsyacc_sparseGotoTableRowOffsets;
    tagOfErrorTerminal = _fsyacc_tagOfErrorTerminal;
    parseError = (fun (ctxt:Microsoft.FSharp.Text.Parsing.ParseErrorContext<_>) -> 
                              match parse_error_rich with 
                              | Some f -> f ctxt
                              | None -> parse_error ctxt.Message);
    numTerminals = 17;
    productionToNonTerminalTable = _fsyacc_productionToNonTerminalTable  }
let engine lexer lexbuf startState = (tables ()).Interpret(lexer, lexbuf, startState)
let start lexer lexbuf :  Node  =
    Microsoft.FSharp.Core.Operators.unbox ((tables ()).Interpret(lexer, lexbuf, 0))