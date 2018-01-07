namespace NCoreUtils.Data

open NCoreUtils.Data.Protocol.Ast
open System.Collections.Immutable
open System
open System.Linq.Expressions

[<Interface>]
type IDataQueryParser =
  abstract ParseQuery : input:string -> Node

[<AllowNullLiteral>]
[<Interface>]
type ICallInfo =
  abstract Name       : string
  abstract ResultType : Type
  abstract Parameters : ImmutableArray<Type>
  abstract CreateExpression : arguments:Expression[] -> Expression

[<Interface>]
type ICallInfoResolver =
  abstract ResolveCall : name:string * argNum:int -> ICallInfo

[<Interface>]
type IDataQueryExpressionBuilder =
  abstract BuildExpression : rootType:Type * input:string -> LambdaExpression
