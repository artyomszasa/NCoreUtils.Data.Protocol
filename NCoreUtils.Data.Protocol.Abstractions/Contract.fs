namespace NCoreUtils.Data

open NCoreUtils.Data.Protocol.Ast
open System.Collections.Immutable
open System
open System.Linq.Expressions
open System.Runtime.Serialization

[<Serializable>]
type ProtocolException =
  inherit Exception
  new () = { inherit Exception () }
  new (message : string) = { inherit Exception (message) }
  new (message : string, innerException) = { inherit Exception (message, innerException) }
  new (info : SerializationInfo, context) = { inherit Exception (info, context) }

[<Serializable>]
type ProtocolSyntaxException =
  inherit ProtocolException
  new (message) = { inherit ProtocolException (message) }
  new (message : string, innerException) = { inherit ProtocolException (message, innerException) }
  new (info : SerializationInfo, context) = { inherit ProtocolException (info, context) }

[<Interface>]
type IDataQueryParser =
  /// <summary>
  /// Parses input into internal AST.
  /// </summary>
  /// <param name="input">String that contains raw input.</param>
  /// <returns>Root node of parsed AST.</returns>
  /// <exception cref="NCoreUtils.Data.ProtocolException">
  /// Thrown if expression is malformed.
  /// </exception>
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
