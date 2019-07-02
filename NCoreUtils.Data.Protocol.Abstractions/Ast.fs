namespace NCoreUtils.Data.Protocol.Ast

open System.Collections.Immutable
open System.Runtime.CompilerServices
open System.Threading

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
      | Identifier _ -> 0

[<RequireQualifiedAccess>]
[<Extension>]
module Node =

  let private binOpStrings =
    Map.ofList
      [ BinaryOperation.Equal,              "="
        BinaryOperation.NotEqual,           "!="
        BinaryOperation.LessThan,           "<"
        BinaryOperation.LessThanOrEqual,    "<="
        BinaryOperation.GreaterThan,        ">"
        BinaryOperation.GreaterThanOrEqual, ">="
        BinaryOperation.OrElse,             "||"
        BinaryOperation.AndAlso,            "&&"
        BinaryOperation.Add,                "+"
        BinaryOperation.Substract,          "-"
        BinaryOperation.Multiply,           "*"
        BinaryOperation.Divide,             "/"
        BinaryOperation.Modulo,             "%" ]

  let private isNum (s : string) =
    s |> String.forall (fun ch -> ch >= '0' && ch <= '9')

  let private nextUid =
    let i = ref 0
    fun () -> sprintf "__param%d" <| Interlocked.Increment i

  [<CompiledName("SubstituteParameter")>]
  let rec substituteParameter source target node =
    match node with
    | Lambda (arg, body) ->
      let struct (arg', body') =
        match arg with
        | Identifier n ->
          match n = target with
          | true ->
            let n' = nextUid ()
            struct (Identifier n', substituteParameter n n' body)
          | _    -> struct (arg, body)
        | _ -> invalidArg "node" "Invalid lambda node."
      Lambda (arg', substituteParameter source target body')
    | Binary (left, op, right) ->
      Binary (substituteParameter source target left, op, substituteParameter source target right)
    | Call (name, args) ->
      let builder = ImmutableArray.CreateBuilder args.Length
      for i = 0 to args.Length - 1 do
        builder.Add (substituteParameter source target args.[i])
      Call (name, builder.ToImmutable ())
    | Member (inst, name) ->
      Member (substituteParameter source target inst, name)
    | Identifier n ->
      Identifier (if n = source then target else n)
    | v -> v

  [<Extension>]
  [<CompiledName("Stringify")>]
  let stringify node =
    let rec impl complex node =
      match node with
      | Lambda (arg, body) ->
        sprintf "%s => %s" (impl false arg) (impl false body)
      | Binary (left, op, right) ->
        match complex with
        | true -> sprintf "(%s %s %s)" (impl true left) (Map.find op binOpStrings) (impl true right)
        | _    -> sprintf "%s %s %s"   (impl true left) (Map.find op binOpStrings) (impl true right)
      | Call (name, args) ->
        sprintf "%s(%s)" name (args |> Seq.map (impl false) |> String.concat ", ")
      | Member (inst, name) ->
        sprintf "%s.%s" (impl true inst) name
      | Constant null -> "null"
      | Constant v    ->
        match isNum v with
        | true -> v
        | _    -> sprintf "\"%s\"" (v.Replace("\\", "\\\\").Replace("\"", "\\\""))
      | Identifier x  -> x
    impl false node

  [<CompiledName("Equals")>]
  let eq node1 node2 =
    let rec impl map node1 node2 =
      match node1, node2 with
      | Lambda (Identifier i1, body1), Lambda (Identifier i2, body2) ->
        impl (Map.add i1 i2 map) body1 body2
      | Binary (left1, op1, right1), Binary (left2, op2, right2) ->
        op1 = op2
          && impl map left1  left2
          && impl map right1 right2
      | Call (name1, args1), Call (name2, args2) ->
        System.StringComparer.OrdinalIgnoreCase.Equals (name1, name2)
          && args1.Length = args2.Length
          && Seq.forall2 (impl map) args1 args2
      | Member (inst1, name1), Member (inst2, name2) ->
        System.StringComparer.OrdinalIgnoreCase.Equals (name1, name2)
          && impl map inst1 inst2
      | Constant c1, Constant c2 -> c1 = c2
      | Identifier i1, Identifier i2 ->
        match Map.tryFind i1 map with
        | Some ix -> ix = i2
        | _       -> i1 = i2
      | _ -> false
    impl Map.empty node1 node2

  [<CompiledName("CombineBy")>]
  let combineBy f node1 node2 =
    match node1, node2 with
    | Lambda ((Identifier n1) as arg1, body1), Lambda (Identifier n2, body2) ->
      Lambda (arg1, f body1 (substituteParameter n2 n1 body2))
    | _ -> invalidOp <| sprintf "Invalid nodes (%A, %A)." node1 node2

  [<CompiledName("CombineAnd")>]
  let combineAnd node1 node2 =
    combineBy (fun body1 body2 -> Binary (body1, BinaryOperation.AndAlso, body2)) node1 node2
