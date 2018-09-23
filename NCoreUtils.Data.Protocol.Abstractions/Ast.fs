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
  // Arithmetic
  | Add                  = 8
  | Substract            = 9
  | Multiply             = 10
  | Divide               = 11
  | Modulo               = 12
  // TODO: Bitwise

[<StructuralEquality; NoComparison>]
type Node =
  | Lambda of Arg:Node * Body:Node
  | Binary of Left:Node * Operation:BinaryOperation * Right:Node
  | Call of Name:string * Arguments:ImmutableArray<Node>
  | Member of Instance:Node * Member:string
  | Constant of RawValue:string
  | Identifier of Value:string
