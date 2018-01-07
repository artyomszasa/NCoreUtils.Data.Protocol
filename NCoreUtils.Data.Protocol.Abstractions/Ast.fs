namespace NCoreUtils.Data.Protocol.Ast

open System.Collections.Immutable

type BinaryOperation =
  // Comparison
  | Equal                = 0
  | NotEqual             = 1
  | LessThan             = 2
  | LessThanOrEqual      = 3
  | GreaterThan          = 4
  | GreaterThanOrEqual   = 5
  // Conditional
  | OrElse               = 6
  | AndAlso              = 7
  // TODO: Arithmetic
  // TODO: Bitwise

[<StructuralEquality; NoComparison>]
type Node =
  | Binary of Left:Node * Operation:BinaryOperation * Right:Node
  | Call of Name:string * Arguments:ImmutableArray<Node>
  | Constant of RawValue:string
  | Identifier of Value:string
