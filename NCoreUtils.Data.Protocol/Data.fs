namespace NCoreUtils.Data.Protocol.Internal

open NCoreUtils.Data.Protocol.Ast
open System.Collections.Immutable

type [<NoComparison>] NodeXNode< [<EqualityConditionalOn>] 'a> =
  | BinaryX     of Left:NodeX<'a> * Operation:BinaryOperation * Right:NodeX<'a>
  | CallX       of Name:string * Arguments:ImmutableArray<NodeX<'a>>
  | ConstantX   of RawValue:string
  | IdentifierX of Value:string

and [<NoComparison>] NodeX< [<EqualityConditionalOn>] 'a> = {
  Node : NodeXNode<'a>
  Data : 'a }
  with
    member this.GetChildren () =
      match this.Node with
      | BinaryX (left, _, right) -> [| left; right |] :> seq<_>
      | CallX (_, args)          -> args :> seq<_>
      | _                        -> Seq.empty

module NodeX =

  [<CompiledName("MapData")>]
  let rec mapData mapper (node : NodeX<_>) =
    let node' =
      match node.Node with
      | BinaryX (left, op, right) -> BinaryX (mapData mapper left, op, mapData mapper right)
      | CallX (name, args) -> CallX (name, args |> Seq.map (mapData mapper) |> ImmutableArray.CreateRange)
      | ConstantX   v -> ConstantX   v
      | IdentifierX v -> IdentifierX v
    { Node = node'; Data = mapper node.Data}
