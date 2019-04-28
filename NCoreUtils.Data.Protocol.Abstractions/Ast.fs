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

[<AutoOpen>]
module private NodeHelpers =

  let eq = System.StringComparer.OrdinalIgnoreCase

/// Represents a single node in protocol AST.
[<CustomEquality; NoComparison>]
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
  with
    static member private DeepEq (a : Node, b : Node, ctx : Map<string, string>) =
      match a, b with
      | Lambda (Identifier arg0, body0), Lambda (Identifier arg1, body1) ->
        Node.DeepEq (body0, body1, Map.add arg0 arg1 ctx)
      | Binary (left0, op0, right0), Binary (left1, op1, right1) when op0 = op1 ->
        Node.DeepEq (left0, left1, ctx) && Node.DeepEq (right0, right1, ctx)
      | Call (name0, args0), Call (name1, args1) when eq.Equals (name0, name1) && args0.Length = args1.Length ->
        Seq.forall2 (fun arg0 arg1 -> Node.DeepEq (arg0, arg1, ctx)) args0 args1
      | Member (inst0, name0), Member (inst1, name1) when eq.Equals (name0, name1) ->
        Node.DeepEq (inst0, inst1, ctx)
      | Constant c0, Constant c1 -> c0 = c1
      | Identifier name0, Identifier name1 ->
        match Map.tryFind name0 ctx with
        | Some name0' -> eq.Equals (name0', name1)
        | _ -> false
      | _ -> false
    interface System.IEquatable<Node> with
      member this.Equals other = Node.DeepEq (this, other, Map.empty)
    override this.Equals obj =
      match obj with
      | null             -> false
      | :? Node as other -> Node.DeepEq (this, other, Map.empty)
      | _                -> false
    override this.GetHashCode () =
      match this with
      | Lambda (_, body) ->
        body.GetHashCode () * 17
      | Binary (left, op, right) ->
        (left.GetHashCode () * 17
          + op.GetHashCode () * 17)
            + right.GetHashCode ()
      | Call (name, args) ->
        Seq.fold
          (fun hash arg -> hash * 17 + arg.GetHashCode ())
          (name.GetHashCode ())
          args
      | Member (inst, name) ->
        inst.GetHashCode () * 17 + name.GetHashCode ()
      | Constant null -> 0
      | Constant v -> v.GetHashCode ()
      | Identifier x -> 0
