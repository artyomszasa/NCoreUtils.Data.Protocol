namespace NCoreUtils.Data.Protocol

open NCoreUtils.Data
open System.Collections.Immutable
open System.Linq.Expressions
open System.Reflection

module DefaultCalls =

  [<CompiledName("Length")>]
  let length =
    let ps = ImmutableArray.CreateRange [ typeof<string> ]
    { new ICallInfo with
        member __.Name = "length"
        member __.ResultType = typeof<int>
        member __.Parameters = ps
        member __.CreateExpression arguments = Expression.Property (arguments.[0], "Length") :> _
    }

  [<CompiledName("Lower")>]
  let lower =
    let ps = ImmutableArray.CreateRange [ typeof<string> ]
    let m = typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public) |> Array.find (fun m -> m.Name = "ToLower" && m.GetParameters().Length = 0)
    { new ICallInfo with
        member __.Name = "lower"
        member __.ResultType = typeof<string>
        member __.Parameters = ps
        member __.CreateExpression arguments = Expression.Call (arguments.[0], m) :> _
    }

  [<CompiledName("Upper")>]
  let upper =
    let ps = ImmutableArray.CreateRange [ typeof<string> ]
    let m = typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public) |> Array.find (fun m -> m.Name = "ToUpper" && m.GetParameters().Length = 0)
    { new ICallInfo with
        member __.Name = "upper"
        member __.ResultType = typeof<string>
        member __.Parameters = ps
        member __.CreateExpression arguments = Expression.Call (arguments.[0], m) :> _
    }

  [<CompiledName("Contains")>]
  let contains =
    let ps = ImmutableArray.CreateRange [ typeof<string>; typeof<string> ]
    let m = typeof<string>.GetMethods (BindingFlags.Instance ||| BindingFlags.Public) |> Array.find (fun m -> m.Name = "Contains" && m.GetParameters().Length = 1)
    { new ICallInfo with
        member __.Name = "contains"
        member __.ResultType = typeof<bool>
        member __.Parameters = ps
        member __.CreateExpression arguments = Expression.Call (arguments.[0], m, arguments.[1]) :> _
    }

  [<CompiledName("All")>]
  let all = [| length; lower; upper; contains |]
