namespace NCoreUtils.Data

open NCoreUtils.Data.Protocol.Ast
open System.Collections.Immutable
open System
open System.Linq.Expressions
open System.Runtime.Serialization

/// Represents errors that occur during protocol related operations.
[<Serializable>]
type ProtocolException =
  inherit Exception
  new () = { inherit Exception () }
  new (message : string) = { inherit Exception (message) }
  new (message : string, innerException) = { inherit Exception (message, innerException) }
  new (info : SerializationInfo, context) = { inherit Exception (info, context) }

/// Represents errors that occur when raw data query has invalid syntax.
[<Serializable>]
type ProtocolSyntaxException =
  inherit ProtocolException
  new (message) = { inherit ProtocolException (message) }
  new (message : string, innerException) = { inherit ProtocolException (message, innerException) }
  new (info : SerializationInfo, context) = { inherit ProtocolException (info, context) }

/// Defines functionality to parse raw input string into internal AST.
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

/// Defines functionality to parse and process raw input string into LINQ expressions.
[<Interface>]
type IDataQueryExpressionBuilder =
  /// <summary>
  /// Parses and processes specified query creating LINQ expression with respect to the root argument type.
  /// </summary>
  /// <param name="rootType">Type of the root argument in the expression.</param>
  /// <param name="input">Raw query to parse and process.</param>
  /// <returns>LINQ Expression representation of the input query.</returns>
  abstract BuildExpression : rootType:Type * input:string -> LambdaExpression
