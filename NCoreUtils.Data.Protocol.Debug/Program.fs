open NCoreUtils.Data
// open NCoreUtils.Data.Protocol
// open NCoreUtils.Data.Protocol.Ast
open System
// open System.Collections.Immutable
// open System.Reflection
// open System.Linq.Expressions
// open System.Threading
// open System.ComponentModel
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

// [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
// type Ty =
//   | TAny
//   | TExact of Type
//   | TVar   of int64

// type NodeXNode<'a> =
//   | BinaryX     of Left:NodeX<'a> * Operation:BinaryOperation * Right:NodeX<'a>
//   | CallX       of Name:string * Arguments:ImmutableArray<NodeX<'a>>
//   | ConstantX   of RawValue:string
//   | IdentifierX of Value:string
//
// and NodeX<'a> = {
//   Node : NodeXNode<'a>
//   Data : 'a }
//   with
//     member this.GetChildren () =
//       match this.Node with
//       | BinaryX (left, _, right) -> [| left; right |] :> seq<_>
//       | CallX (_, args)          -> args :> seq<_>
//       | _                        -> Seq.empty

// module NodeX =
//
//   let rec mapData mapper (node : NodeX<_>) =
//     let node' =
//       match node.Node with
//       | BinaryX (left, op, right) -> BinaryX (mapData mapper left, op, mapData mapper right)
//       | CallX (name, args) -> CallX (name, args |> Seq.map (mapData mapper) |> ImmutableArray.CreateRange)
//       | ConstantX   v -> ConstantX   v
//       | IdentifierX v -> IdentifierX v
//     { Node = node'; Data = mapper node.Data}

// type IHasTypeVar<'a> =
//   abstract Substitute : substitution:(Ty -> Ty) -> 'a
// and [<Struct>] Subs =
//   | Subs of struct (Ty * Ty)
//   with
//     member this.Source =
//       match this with
//       | Subs struct (x, _) -> x
//     member this.Target =
//       match this with
//       | Subs struct (_, x) -> x
//     interface IHasTypeVar<Subs> with
//       member this.Substitute substitution =
//         match this with
//         | Subs struct (source, target) ->
//           Subs struct (source, substitution target)

// type ISubstitutable<'a> =
//   abstract Substitute : substitution:Subs -> 'a

// [<AutoOpen>]
// module Substitution =
//   let subs a b = Subs struct (a, b)
//   let apply substitution (varHolder : 'a when 'a :> IHasTypeVar<'a>) = varHolder.Substitute substitution
//   let (<==) (target : 'a when 'a :> ISubstitutable<'a>) subs = target.Substitute subs

// type Subss =
//  | Subss of Subs list
//  with
//   member this.TryResolveExact ty =
//     match this with
//     | Subss l ->
//       let rec resolve ty =
//         match ty with
//         | TExact typ -> Some typ
//         | TVar _ as v ->
//           match l |> List.tryFind (fun (Subs struct (x, _)) -> x = v) with
//           | Some (Subs struct (_, x)) -> resolve x
//           | _                         -> None
//         | _ -> None
//       resolve ty
//
//   interface IHasTypeVar<Subss> with
//     member this.Substitute substitution =
//       match this with
//       | Subss list -> List.map (apply substitution) list |> Subss
//   interface ISubstitutable<Subss> with
//     member this.Substitute substitution =
//       match this with
//       | Subss list ->
//         let f t0 =
//           match t0 = substitution.Source with
//           | true -> substitution.Target
//           | _    -> t0
//         let rec insert acc shouldAppend l =
//           match l with
//           | [] ->
//             match shouldAppend with
//             | true  -> substitution :: acc
//             | false -> acc
//           | (subs : Subs) :: subss ->
//             match subs.Source = substitution.Source with
//             | true ->
//               match subs.Target, substitution.Target with
//               | a, b when a = b  -> insert (subs :: acc) false subss
//               | TAny, _          -> insert (substitution :: acc) false subss
//               | _, TAny          -> insert (subs :: acc) false subss
//               | TVar _ as a, b   -> insert (subs :: Subs struct (a, b) :: acc) shouldAppend subss
//               | a, b             -> failwithf "Substitution conflict (%A ~ %A)" a b
//             | _ -> insert (apply f subs :: acc) shouldAppend subss
//         insert [] true list |> Subss


// type Context = {
//   RootType    : Type
//   NewVar      : unit -> Ty
//   ResolveCall : string -> ICallInfo }

// let rec collect (ctx : Context) ty (subss : Subss) node =
//   match node with
//   | Constant value ->
//     let current = ctx.NewVar ()
//     subss <== subs current ty, { Node = ConstantX value; Data = current }
//   | Identifier value ->
//     let property = ctx.RootType.GetProperty (value, BindingFlags.Instance ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Public ||| BindingFlags.IgnoreCase)
//     if isNull property then failwithf "Type %A has no property named %A" ctx.RootType value
//     let current = TExact property.PropertyType
//     subss <== subs ty current, { Node = IdentifierX value; Data = current }
//   | Call (name, args) ->
//     match ctx.ResolveCall name with
//     | null -> failwithf "Unsupported function %A" name
//     | info ->
//       if info.Parameters.Length <> args.Length then
//         failwithf "Parameter count mismatch for %A" name
//       let (subss', args') =
//         args
//         |> Seq.zip info.Parameters
//         |> Seq.fold
//           (fun (ss, res) (typ, arg) ->
//             let v = ctx.NewVar ()
//             let ss' = ss <== subs v (TExact typ)
//             let (ss'', arg') = collect ctx v ss' arg
//             ss'', arg' :: res
//           )
//           (subss, [])
//       let current = TExact info.ResultType
//       subss' <== subs ty current, { Node = CallX (name, args' |> List.rev |> ImmutableArray.CreateRange); Data = current }
//   | Binary (left, op, right) ->
//     match op with
//     | BinaryOperation.AndAlso
//     | BinaryOperation.OrElse ->
//       let current = TExact typeof<bool>
//       let vl = ctx.NewVar ()
//       let vr = ctx.NewVar ()
//       let ss = (subss <== subs vl current) <== subs vr current
//       let (ss',  l') = collect ctx vl ss  left
//       let (ss'', r') = collect ctx vr ss' right
//       ss'' <== subs ty current, { Node = BinaryX (l', op, r'); Data = current }
//     | BinaryOperation.Equal
//     | BinaryOperation.NotEqual
//     | BinaryOperation.GreaterThan
//     | BinaryOperation.GreaterThanOrEqual
//     | BinaryOperation.LessThan
//     | BinaryOperation.LessThanOrEqual ->
//       let current = TExact typeof<bool>
//       let vl = ctx.NewVar ()
//       let vr = ctx.NewVar ()
//       let ss = (subss <== subs vl vr)
//       let (ss',  l') = collect ctx vl ss  left
//       let (ss'', r') = collect ctx vr ss' right
//       ss'' <== subs ty current, { Node = BinaryX (l', op, r'); Data = current }
//     | _ -> failwithf "Unsupported binary op = %A" op

// let resolve (subss : Subss) node =
//   let mapper ty =
//     match subss.TryResolveExact ty with
//     | Some typ -> typ
//     | _        -> failwithf "Unable to resolve %A" ty
//   NodeX.mapData mapper node

// let rec toExpression resolveCall root (node : NodeX<Type>) =
//   match node.Node with
//   | ConstantX null ->
//     match node.Data.IsValueType with
//     | true -> failwith "null value cannot be used with value types"
//     | _ ->
//       Expression.Constant (null, node.Data) :> Expression
//   | ConstantX value ->
//     let converter = TypeDescriptor.GetConverter node.Data
//     Expression.Constant (converter.ConvertFrom value, node.Data) :> Expression
//   | IdentifierX name ->
//     Expression.Property (root, name) :> Expression
//   | BinaryX (left, op, right) ->
//     let l = toExpression resolveCall root left
//     let r = toExpression resolveCall root right
//     match op with
//     | BinaryOperation.AndAlso            -> Expression.AndAlso            (l, r)
//     | BinaryOperation.OrElse             -> Expression.OrElse             (l, r)
//     | BinaryOperation.Equal              -> Expression.Equal              (l, r)
//     | BinaryOperation.NotEqual           -> Expression.NotEqual           (l, r)
//     | BinaryOperation.GreaterThan        -> Expression.GreaterThan        (l, r)
//     | BinaryOperation.GreaterThanOrEqual -> Expression.GreaterThanOrEqual (l, r)
//     | BinaryOperation.LessThan           -> Expression.LessThan           (l, r)
//     | BinaryOperation.LessThanOrEqual    -> Expression.LessThanOrEqual    (l, r)
//     | _                                  -> failwithf "Unsupported operation %A" op
//     :> Expression
//   | CallX (name, args) ->
//     let (call : ICallInfo) = resolveCall name
//     let args' = args |> Seq.map (toExpression resolveCall root) |> Seq.toArray
//     call.CreateExpression (args')

// let toLambdaExpression resolveCall rootType node =
//   let arg = Expression.Parameter rootType
//   Expression.Lambda (toExpression resolveCall arg node, arg)

// let fnLength =
//   let ps = ImmutableArray.CreateRange [ typeof<string> ]
//   { new ICallInfo with
//       member __.Name = "length"
//       member __.ResultType = typeof<int>
//       member __.Parameters = ps
//       member __.CreateExpression arguments =
//         let instance = arguments.[0]
//         Expression.Property (instance, "Length") :> _
//   }

type X () =
  member val X    =  2  with get, set
  member val Name = "2" with get, set

[<EntryPoint>]
let main _ =

  let services =
    ServiceCollection()
      .AddLogging(fun builder -> builder.SetMinimumLevel(LogLevel.Trace).ClearProviders().AddConsole() |> ignore)
      .AddDataQueryServices()
      .BuildServiceProvider()
  try
    use scope = services.CreateScope ()
    let serviceProvider = scope.ServiceProvider
    let expressionBuilder = serviceProvider.GetRequiredService<IDataQueryExpressionBuilder>()
    let inputs =
      [ "x > 2"
        "length(name) > 2"
        "(x > 30) || (x < 10) || (x = 20)"
        "((x > 30) || (x < 10)) || (x = 20)"
        "(x > 30) || ((x < 10) || (x = 20))"
        "name = null || length(name) > 2"
        "name = null || contains(name, \"xy\")" ]
    for input in inputs do
      let lambda = expressionBuilder.BuildExpression<X> input
      printfn "%A => %A" input lambda
  finally
    match box services with
    | :? IDisposable as d -> d.Dispose ()
    | _                   -> ()
  // let qparser = DataQueryParser () :> IDataQueryParser
  // let q0 = qparser.ParseQuery "x > 2"
  // let q1 = qparser.ParseQuery "length(name) > 2"
  // let q2 = qparser.ParseQuery "name = null || length(name) > 2"

  // let ctx =
  //   let sup = ref 0L
  //   { RootType    = typeof<X>
  //     NewVar      = fun () -> Interlocked.Increment sup |> TVar
  //     ResolveCall = fun name ->
  //                     match StringComparer.InvariantCultureIgnoreCase.Equals (name, fnLength.Name) with
  //                     | true -> fnLength
  //                     | _    -> null
  //    }

  // let x0 = collect ctx (ctx.NewVar ()) (Subss []) q0 ||> resolve
  // let x1 = collect ctx (ctx.NewVar ()) (Subss []) q1 ||> resolve
  // let x2 = collect ctx (ctx.NewVar ()) (Subss []) q2 ||> resolve
  // printfn "%A" x0
  // printfn "%A" x1
  // printfn "%A" x2

  // let e0 = toLambdaExpression ctx.ResolveCall ctx.RootType x0
  // printfn "%A" e0
  // let e1 = toLambdaExpression ctx.ResolveCall ctx.RootType x1
  // printfn "%A" e1
  // let e2 = toLambdaExpression ctx.ResolveCall ctx.RootType x2
  // printfn "%A" e2

  0
