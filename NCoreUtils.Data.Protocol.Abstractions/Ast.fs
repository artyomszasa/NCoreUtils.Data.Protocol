namespace NCoreUtils.Data.Protocol.Ast

open System.Collections.Immutable

/// Supported binary operations
type BinaryOperation =
  // Comparison
  /// Represents equality check operation.
  | Equal                = 0
  /// Represents inequality check operation.
  | NotEqual             = 1
  /// Represents less than comparison operation.
  | LessThan             = 2
  /// Represents less than or equal comparison operation.
  | LessThanOrEqual      = 3
  /// Represents grater than comparison operation.
  | GreaterThan          = 4
  /// Represents grater than or equals comparison operation.
  | GreaterThanOrEqual   = 5
  // Conditional
  /// Represents a short-circuiting conditional OR operation.
  | OrElse               = 6
  /// Represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to
  /// true.
  | AndAlso              = 7
  // Arithmetic
  /// Represents an addition operation.
  | Add                  = 8
  /// Represents a substraction operation.
  | Substract            = 9
  /// Represents a multiplication operation.
  | Multiply             = 10
  /// Represents a division operation.
  | Divide               = 11
  /// Represents an arithmetic remainder operation.
  | Modulo               = 12
  // TODO: Bitwise

/// Represents a single node in protocol AST.
[<StructuralEquality; NoComparison>]
type Node =
  /// Represents lambda expression node.
  | Lambda     of Arg:Node * Body:Node
  /// Represents binary operation node.
  | Binary     of Left:Node * Operation:BinaryOperation * Right:Node
  /// Represents function invocation node.
  | Call       of Name:string * Arguments:ImmutableArray<Node>
  /// Represents member access node.
  | Member     of Instance:Node * Member:string
  /// Represents constant node.
  | Constant   of RawValue:string
  /// Represents identifier node.
  | Identifier of Value:string
