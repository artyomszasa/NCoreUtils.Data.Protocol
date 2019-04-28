[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module NCoreUtils.Data.Protocol.DataExpressionBuilderTests

open System.Linq.Expressions
open Xunit
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open NCoreUtils.Data.Protocol.TypeInference
open NCoreUtils.Data

[<StructuralEquality; NoComparison>]
type SubItem = {
  Name : string }

type Item = {
  Num : int
  Str : string
  Sub : SubItem[] }

type private DummyLogger<'T> () =
  interface ILogger<'T> with
    member __.BeginScope (_state: 'TState) =
      { new System.IDisposable with member __.Dispose () = () }
    member __.IsEnabled(_logLevel: LogLevel) =
      false
    member __.Log(_: LogLevel, _: EventId, _: 'TState, _: exn, _: System.Func<'TState,exn,string>) =
      ()


let private createBuilder () =
  let cirBuilder =
    FunctionDescriptorResolverBuilder()
      .Add<CommonFunctions.StringLength>()
      .Add<CommonFunctions.StringToLower>()
      .Add<CommonFunctions.StringToUpper>()
      .Add<CommonFunctions.StringContains>()
      .Add<CommonFunctions.CollectionContains>()
      .Add<CommonFunctions.CollectionAny>()
      .Add<CommonFunctions.CollectionAll>()

  let services =
    ServiceCollection()
      .AddSingleton<CommonFunctions.StringLength>()
      .AddSingleton<CommonFunctions.StringToLower>()
      .AddSingleton<CommonFunctions.StringToUpper>()
      .AddSingleton<CommonFunctions.StringContains>()
      .AddSingleton<CommonFunctions.CollectionContains>()
      .AddSingleton<CommonFunctions.CollectionAny>()
      .AddSingleton<CommonFunctions.CollectionAll>()
      .BuildServiceProvider()

  let parser = DataQueryParser ()
  let inferrer = TypeInferrer (cirBuilder.Build services)
  DataQueryExpressionBuilder(parser, inferrer, new DummyLogger<DataQueryExpressionBuilder>()) :> IDataQueryExpressionBuilder

[<Fact>]
let ``member access`` () =
  let builder = createBuilder ()
  let raw = "o => o.num"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
  Assert.Equal (2, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``length invocation`` () =
  let builder = createBuilder ()
  let raw = "o => length(o.str) + 1"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
  Assert.Equal (4, fn.Invoke { Num = Unchecked.defaultof<_>; Str = "AbC"; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``lower invocation`` () =
  let builder = createBuilder ()
  let raw = "o => lower(o.str)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, string>>).Compile ()
  Assert.Equal ("abc", fn.Invoke { Num = Unchecked.defaultof<_>; Str = "AbC"; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``upper invocation`` () =
  let builder = createBuilder ()
  let raw = "o => upper(o.str)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, string>>).Compile ()
  Assert.Equal ("ABC", fn.Invoke { Num = Unchecked.defaultof<_>; Str = "AbC"; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``nested lambda`` () =
  let builder = createBuilder ()
  let raw = "o => seed => contains(o.str, seed)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, System.Func<string, bool>>>).Compile ()
  let item = { Num = Unchecked.defaultof<_>; Str = "AbC"; Sub = Unchecked.defaultof<_> }
  Assert.True (fn.Invoke(item).Invoke "Ab")
  Assert.False (fn.Invoke(item).Invoke "ab")

[<Fact>]
let ``collection contains`` () =
  let builder = createBuilder ()
  let raw = "o => seed => contains(o.sub, seed)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, System.Func<SubItem, bool>>>).Compile ()
  let item = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  Assert.True (fn.Invoke(item).Invoke { Name = "xxx" })
  Assert.False (fn.Invoke(item).Invoke { Name = "yyy" })

[<Fact>]
let ``collection includes`` () =
  let builder = createBuilder ()
  let raw = "o => seed => includes(o.sub, seed)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, System.Func<SubItem, bool>>>).Compile ()
  let item = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  Assert.True (fn.Invoke(item).Invoke { Name = "xxx" })
  Assert.False (fn.Invoke(item).Invoke { Name = "yyy" })


[<Fact>]
let ``collection any`` () =
  let builder = createBuilder ()
  let raw = "o => seed => some(o.sub, v => v.name = seed)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, System.Func<string, bool>>>).Compile ()
  let item = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  Assert.True (fn.Invoke(item).Invoke "xxx")
  Assert.False (fn.Invoke(item).Invoke "yyy")

[<Fact>]
let ``collection all`` () =
  let builder = createBuilder ()
  let raw = "o => seed => every(o.sub, v => v.name = seed)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, System.Func<string, bool>>>).Compile ()
  let item0 = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  let item1 = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" }; { Name = "yyy" } |] }
  Assert.True (fn.Invoke(item0).Invoke "xxx")
  Assert.False (fn.Invoke(item1).Invoke "xxx")
