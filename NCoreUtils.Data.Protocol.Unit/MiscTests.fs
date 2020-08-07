module NCoreUtils.Data.Protocol.MiscTests

open NCoreUtils.Data.Protocol.TypeInference
open System
open System.Reflection
open Xunit
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol
open System.Collections.Immutable

type Expression = System.Linq.Expressions.Expression

type XObj =

  val mutable public fi32 : int

  member this.I32
    with get () = this.fi32
    and  set v  = this.fi32 <- v

  new (i) = { fi32 = i }

  member this.Inc () = this.fi32 <- this.fi32 + 1

[<Fact>]
let ``constant extractable expressions`` () =
  let nullExpr : Expression = Unchecked.defaultof<_>
  let sf = Expression.Field (nullExpr, typeof<DateTimeOffset>.GetField("MinValue", BindingFlags.Public ||| BindingFlags.Static))
  let sp = Expression.Property (nullExpr, typeof<DateTimeOffset>.GetProperty("Now", BindingFlags.Public ||| BindingFlags.Static))
  let obj = XObj 3
  let f = Expression.Field (Expression.Constant obj, "fi32")
  let p = Expression.Property (Expression.Constant obj, "I32")
  Assert.True (match sf with | NCoreUtils.Data.Protocol.Helpers.ConstableExpression _ -> true | _ -> false)
  Assert.True (match sp with | NCoreUtils.Data.Protocol.Helpers.ConstableExpression _ -> true | _ -> false)
  Assert.True (match f with | NCoreUtils.Data.Protocol.Helpers.ConstableExpression _ -> true | _ -> false)
  Assert.True (match p with | NCoreUtils.Data.Protocol.Helpers.ConstableExpression _ -> true | _ -> false)

[<Fact>]
let ``map to immutable`` () =
  let arr = [| 1; 2; 3 |] |> Array.map (fun i -> Expression.Constant i :> Expression)
  Assert.True (System.Linq.Enumerable.SequenceEqual (arr, NCoreUtils.Data.Protocol.Helpers.mapToImmutable id arr))

[<Fact>]
let ``default property resolver`` () =
  let resolver = DefaultPropertyResolver.Instance :> IPropertyResolver
  let ty = typeof<QueryTests.Entity>
  let p = ty.GetProperty("I32", BindingFlags.Public ||| BindingFlags.Instance)
  let vpn = resolver.TryResolve (ty, "I32_non_existent")
  Assert.True vpn.IsNone
  let vp0 = resolver.TryResolve (ty, "I32")
  Assert.True vp0.IsSome
  Assert.Equal (p, Assert.IsType<DefaultProperty>(vp0.Value).Property)
  let vp1 = resolver.TryResolve (ty, "I32")
  Assert.True vp1.IsSome
  Assert.Equal (p, Assert.IsType<DefaultProperty>(vp1.Value).Property)

[<Fact>]
let ``Node equality`` () =
  let node = Constant "xxx"
  Assert.False ((node :> obj).Equals(null))
  Assert.NotEqual (node :> obj, box 2)
  Assert.NotEqual (node :> obj, Identifier "xx" |> box)
  Assert.NotEqual (Identifier "yy" |> box, Identifier "xx" |> box)
  ()

[<Fact>]
let ``members`` () =
  let f0 = Members.getMember "fi32" typeof<XObj>
  let f1 = Members.getMember "fi64" typeof<XObj>
  let p0 = Members.getMember "I32" typeof<XObj>
  let p1 = Members.getMember "Inc" typeof<XObj>
  Assert.True ((match f0 with FieldMember _ -> true | _ -> false), string f0)
  Assert.True ((match f1 with NoMember -> true | _ -> false), string f1)
  Assert.True ((match p0 with PropertyMember _ -> true | _ -> false), string p0)
  Assert.True ((match p1 with NoMember -> true | _ -> false), string p1)

[<Fact>]
let ``simple expression combining`` () =
  let x = Lambda (Identifier "x", Binary (Identifier "x", BinaryOperation.LessThan, Constant "10"))
  let y = Lambda (Identifier "y", Binary (Identifier "y", BinaryOperation.GreaterThan, Constant "2"))
  let z = Node.combineAnd x y
  let check =
    Lambda (
      Identifier "a",
      Binary (
        Binary (Identifier "a", BinaryOperation.LessThan, Constant "10"),
        BinaryOperation.AndAlso,
        Binary (Identifier "a", BinaryOperation.GreaterThan, Constant "2")
      )
    )
  Assert.Equal (check, z)

[<Fact>]
let ``complex expression combining`` () =
  let impl innerArgName =
    let genBody outer inner =
      Call (
        "fn",
        ImmutableArray.CreateRange (
          [|
            Lambda (
              Identifier inner,
              Binary (
                Identifier inner,
                BinaryOperation.Equal,
                Identifier outer
              )
            )
            Member (Identifier outer, "member")
          |]
        )
      )
    let x = Lambda (Identifier "x", genBody "x" innerArgName)
    let y = Lambda (Identifier "y", Binary (Identifier "y", BinaryOperation.GreaterThan, Constant "2"))
    let z = Node.combineAnd y x
    let check =
      Lambda (
        Identifier "a",
        Binary (
          Binary (Identifier "a", BinaryOperation.GreaterThan, Constant "2"),
          BinaryOperation.AndAlso,
          genBody "a" "v"
        )
      )
    Assert.Equal ("a => (a > 2) && fn(v => v = a, a.member)", Node.stringify check)
    Assert.Equal (check, z)
  impl "u"
  impl "y"


[<Fact>]
let ``const expression to ast node`` () =
  let ctx = ExpressionToAstVisitor.Context.empty
  let matcher = { new IFunctionMatcher with member _.MatchFunction (_, next) = next.Invoke () }
  let toAst = ExpressionToAstVisitor.nodeToAst ctx matcher
  Assert.Equal (Constant null, System.Linq.Expressions.Expression.Constant (null, typeof<string>) |> toAst)
  Assert.Equal (Constant "0.5", System.Linq.Expressions.Expression.Constant 0.5f |> toAst)
  Assert.Equal (Constant "0.5", System.Linq.Expressions.Expression.Constant 0.5 |> toAst)
  Assert.Equal (Constant "0.5", System.Linq.Expressions.Expression.Constant 0.5m |> toAst)