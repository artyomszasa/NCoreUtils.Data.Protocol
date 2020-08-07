module NCoreUtils.Data.Protocol.DataExpressionBuilderTests

open System
open System.IO
open System.Linq.Expressions
open System.Runtime.Serialization.Formatters.Binary
open Xunit
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open NCoreUtils.Data.Protocol.TypeInference
open NCoreUtils.Data
open System.Collections.Immutable
open System.Collections.Generic

[<StructuralEquality; NoComparison>]
type SubItem = {
  Name : string }

type Item = {
  Num : int
  Str : string
  Sub : SubItem[] }

type AOrB =
  | A = 0
  | B = 1

type ItemWithEnum = {
  Value: AOrB }

type ItemWithNullable = {
  NValue: Nullable<int> }

type private DummyLogger<'T> () =
  interface ILogger<'T> with
    member __.BeginScope (_state: 'TState) =
      { new System.IDisposable with member __.Dispose () = () }
    member __.IsEnabled(_logLevel: LogLevel) =
      false
    member __.Log(_: LogLevel, _: EventId, _: 'TState, _: exn, _: System.Func<'TState,exn,string>) =
      ()

let private createBuilderWith addFunctions addServices =
  let cirBuilder : FunctionDescriptorResolverBuilder =
    FunctionDescriptorResolverBuilder()
      .Add<CommonFunctions.StringLength>()
      .Add<CommonFunctions.StringToLower>()
      .Add<CommonFunctions.StringToUpper>()
      .Add<CommonFunctions.StringContains>()
      .Add<CommonFunctions.CollectionContains>()
      .Add<CommonFunctions.CollectionAny>()
      .Add<CommonFunctions.CollectionAll>()
    |> addFunctions

  let services =
    let services =
      ServiceCollection()
        .AddSingleton<CommonFunctions.StringLength>()
        .AddSingleton<CommonFunctions.StringToLower>()
        .AddSingleton<CommonFunctions.StringToUpper>()
        .AddSingleton<CommonFunctions.StringContains>()
        .AddSingleton<CommonFunctions.CollectionContains>()
        .AddSingleton<CommonFunctions.CollectionAny>()
        .AddSingleton<CommonFunctions.CollectionAll>()
    let services': IServiceCollection = addServices services
    services'.BuildServiceProvider()

  let resolver = cirBuilder.Build services
  Assert.Null (resolver.ResolveFunction ("non-existent-function", TypeVariable.empty, [| |]))
  let parser = DataQueryParser ()
  let inferrer = TypeInferrer resolver
  DataQueryExpressionBuilder(parser, inferrer, new DummyLogger<DataQueryExpressionBuilder>()) :> IDataQueryExpressionBuilder

let private createBuilder () =
  createBuilderWith id id

[<Fact>]
let ``member access`` () =
  let builder = createBuilder ()
  do
    let raw = "o => o.num + 1"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
    Assert.Equal (3, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num - 1"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
    Assert.Equal (1, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num * 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
    Assert.Equal (4, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num / 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
    Assert.Equal (1, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num % 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, int>>).Compile ()
    Assert.Equal (0, fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num > 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 3; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num < 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 1; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num >= 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 3; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.True (fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 1; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num <= 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 1; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.True (fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 3; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => o.num != 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 1; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 2; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })
    Assert.True (fn.Invoke { Num = 3; Str = Unchecked.defaultof<_>; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``logical operators`` () =
  let builder = createBuilder ()
  do
    let raw = "o => length(o.str) > 2 && o.num = 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 2; Str = "AbC"; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 2; Str = "Ab"; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 3; Str = "AbC"; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 3; Str = "Ab"; Sub = Unchecked.defaultof<_> })
  do
    let raw = "o => length(o.str) > 2 || o.num = 2"
    let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
    Assert.True (fn.Invoke { Num = 2; Str = "AbC"; Sub = Unchecked.defaultof<_> })
    Assert.True (fn.Invoke { Num = 2; Str = "Ab"; Sub = Unchecked.defaultof<_> })
    Assert.True (fn.Invoke { Num = 3; Str = "AbC"; Sub = Unchecked.defaultof<_> })
    Assert.False (fn.Invoke { Num = 3; Str = "Ab"; Sub = Unchecked.defaultof<_> })

[<Fact>]
let ``enum`` () =
  let builder = createBuilder ()
  do
    let raw = "o => o.value = 0"
    let fn = (builder.BuildExpression(typeof<ItemWithEnum>, raw) :?> Expression<System.Func<ItemWithEnum, bool>>).Compile ()
    Assert.True (fn.Invoke { Value = AOrB.A })
    Assert.False (fn.Invoke { Value = AOrB.B })
  do
    let raw = "o => o.value = \"A\""
    let fn = (builder.BuildExpression(typeof<ItemWithEnum>, raw) :?> Expression<System.Func<ItemWithEnum, bool>>).Compile ()
    Assert.True (fn.Invoke { Value = AOrB.A })
    Assert.False (fn.Invoke { Value = AOrB.B })
  do
    let raw = "o => o.value = \"a\""
    let fn = (builder.BuildExpression(typeof<ItemWithEnum>, raw) :?> Expression<System.Func<ItemWithEnum, bool>>).Compile ()
    Assert.True (fn.Invoke { Value = AOrB.A })
    Assert.False (fn.Invoke { Value = AOrB.B })

[<Fact>]
let ``nullable`` () =
  let builder = createBuilder ()
  do
    let raw = "o => o.nvalue = 1"
    let fn = (builder.BuildExpression(typeof<ItemWithNullable>, raw) :?> Expression<System.Func<ItemWithNullable, bool>>).Compile ()
    Assert.True (fn.Invoke { NValue = Nullable 1 })
    Assert.False (fn.Invoke { NValue = Nullable 2 })
    Assert.False (fn.Invoke { NValue = Nullable () })
  do
    let raw = "o => o.nvalue = null"
    let fn = (builder.BuildExpression(typeof<ItemWithNullable>, raw) :?> Expression<System.Func<ItemWithNullable, bool>>).Compile ()
    Assert.False (fn.Invoke { NValue = Nullable 1 })
    Assert.False (fn.Invoke { NValue = Nullable 2 })
    Assert.True (fn.Invoke { NValue = Nullable () })

[<Fact>]
let ``non-nullable`` () =
  let builder = createBuilder ()
  let raw = "o => o.num = null"
  let exn =
    Assert.Throws<ProtocolException>
      (fun () ->
        let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<System.Func<Item, bool>>).Compile ()
        fn.Invoke { Num = 0; Str = "AbC"; Sub = Unchecked.defaultof<_> } |> ignore
      )
  Assert.IsType<ProtocolTypeConstraintMismatchException>(exn.InnerException)
  |> ignore


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
let ``upper invocation in fallback mode`` () =
  let builder = createBuilder ()
  let raw = "upper(str)"
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
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<Func<Item, Func<string, bool>>>).Compile ()
  let item0 = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  let item1 = { Num = Unchecked.defaultof<_>; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" }; { Name = "yyy" } |] }
  Assert.True (fn.Invoke(item0).Invoke "xxx")
  Assert.False (fn.Invoke(item1).Invoke "xxx")

[<Fact>]
let ``explicit function descriptor`` () =
  let myAddResolver =
    let retc = Func<_, _> (fun (es : IReadOnlyList<Expression>) -> Expression.Constant 2 :> Expression)
    Assert.Throws<ArgumentNullException> (fun () -> FunctionDescriptor.Create (Unchecked.defaultof<_>, typeof<int>, Unchecked.defaultof<_>, retc) |> ignore) |> ignore
    Assert.Throws<ArgumentNullException> (fun () -> FunctionDescriptor.Create ("test", Unchecked.defaultof<_>, Unchecked.defaultof<_>, retc) |> ignore) |> ignore
    Assert.Throws<ArgumentNullException> (fun () -> FunctionDescriptor.Create ("test", typeof<int>, Unchecked.defaultof<_>, Unchecked.defaultof<_>) |> ignore) |> ignore
    Assert.False (FunctionDescriptor.Create("test", typeof<int>, Unchecked.defaultof<_>, retc).ArgumentTypes.IsDefault)
    let myAdd =
      FunctionDescriptor.Create (
        "xxxadd",
        typeof<int>,
        ImmutableArray.Create (typeof<int>, typeof<int>),
        (fun es -> Expression.Add (es.[0], es.[1]) :> Expression)
      )
    Assert.Equal (typeof<int>, myAdd.ResultType)
    Assert.Equal (2, myAdd.ArgumentTypes.Length)
    Assert.Equal (typeof<int>, myAdd.ArgumentTypes.[0])
    Assert.Equal (typeof<int>, myAdd.ArgumentTypes.[0])
    Assert.Equal (myAdd.Name, (myAdd :> IFunctionDescriptor).Name)
    { new IFunctionDescriptorResolver with
      member __.ResolveFunction (name, _, _, next) =
        match StringComparer.OrdinalIgnoreCase.Equals (myAdd.Name, name) with
        | true -> myAdd :> IFunctionDescriptor
        | _    -> next.Invoke ()
    }
  let builder =
    createBuilderWith
      (fun b -> b.Resolvers.Add (myAddResolver.GetType()); b)
      (fun s -> s.AddSingleton (myAddResolver.GetType(), myAddResolver))
  let raw = "o => xxxadd(o.num, o.num)"
  let fn = (builder.BuildExpression(typeof<Item>, raw) :?> Expression<Func<Item, int>>).Compile ()
  let item0 = { Num = 1; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  let item1 = { Num = 2; Str = Unchecked.defaultof<_>; Sub = [| { Name = "xxx" } |] }
  Assert.Equal (2, fn.Invoke item0)
  Assert.Equal (4, fn.Invoke item1)

[<Fact>]
let ``null arguments check`` () =
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
  let eparser = Assert.Throws<ArgumentNullException>(fun  () -> DataQueryExpressionBuilder(Unchecked.defaultof<_>, inferrer, new DummyLogger<DataQueryExpressionBuilder>()) |> ignore)
  Assert.Equal ("parser", eparser.ParamName)
  let einferrer = Assert.Throws<ArgumentNullException>(fun  () -> DataQueryExpressionBuilder(parser, Unchecked.defaultof<_>, new DummyLogger<DataQueryExpressionBuilder>()) |> ignore)
  Assert.Equal ("inferrer", einferrer.ParamName)
  let elogger = Assert.Throws<ArgumentNullException>(fun  () -> DataQueryExpressionBuilder(parser, inferrer, Unchecked.defaultof<_>) |> ignore)
  Assert.Equal ("logger", elogger.ParamName)

[<Fact>]
let ``service collection extensions check`` () =
  let services = ServiceCollection ()
  services.AddDataQueryServices (Action<_> ignore) |> ignore
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.CollectionAll> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.StringLength> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.StringToLower> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.StringToUpper> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.StringContains> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.CollectionContains> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<CommonFunctions.CollectionAny> && d.Lifetime = ServiceLifetime.Singleton))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<IDataQueryExpressionBuilder> && d.ImplementationType = typeof<DataQueryExpressionBuilder>))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<IDataQueryParser> && d.ImplementationType = typeof<DataQueryParser>))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<ITypeInferrer> && d.ImplementationType = typeof<TypeInferrer>))
  Assert.Contains (services, Predicate<ServiceDescriptor> (fun d -> d.ServiceType = typeof<IFunctionDescriptorResolver>))

[<Fact>]
let ``exception serialization`` () =
  let formatter = BinaryFormatter ()
  do
    let mismatch = { TargetType = TypeRef typeof<Item>; Reason = NumericConstraint } : TypeConstriantMismatch
    let exn0 = ProtocolTypeConstraintMismatchException mismatch
    let mutable data = Unchecked.defaultof<_>
    do
      use buffer = new MemoryStream ()
      formatter.Serialize (buffer, exn0)
      data <- buffer.ToArray ()
    let exn =
      use buffer = new MemoryStream (data)
      formatter.Deserialize (buffer) :?> ProtocolTypeConstraintMismatchException
    Assert.Equal (exn0.Details, exn.Details)
    Assert.Equal (exn0.Message, exn.Message)
  do
    let mismatch = { TargetType = TypeRef typeof<Item>; Reason = NonNumericConstraint } : TypeConstriantMismatch
    let exn0 = ProtocolTypeConstraintMismatchException (mismatch, (null : exn))
    let mutable data = Unchecked.defaultof<_>
    do
      use buffer = new MemoryStream ()
      formatter.Serialize (buffer, exn0)
      data <- buffer.ToArray ()
    let exn =
      use buffer = new MemoryStream (data)
      formatter.Deserialize (buffer) :?> ProtocolTypeConstraintMismatchException
    Assert.Equal (exn0.Details, exn.Details)
    Assert.Equal (exn0.Message, exn.Message)
  do
    let exn0 = ProtocolSyntaxException ("some error")
    let mutable data = Unchecked.defaultof<_>
    do
      use buffer = new MemoryStream ()
      formatter.Serialize (buffer, exn0)
      data <- buffer.ToArray ()
    let exn =
      use buffer = new MemoryStream (data)
      formatter.Deserialize (buffer) :?> ProtocolSyntaxException
    Assert.Equal (exn0.Message, exn.Message)
  do
    let exn0 = ProtocolSyntaxException ("some error", null)
    let mutable data = Unchecked.defaultof<_>
    do
      use buffer = new MemoryStream ()
      formatter.Serialize (buffer, exn0)
      data <- buffer.ToArray ()
    let exn =
      use buffer = new MemoryStream (data)
      formatter.Deserialize (buffer) :?> ProtocolSyntaxException
    Assert.Equal (exn0.Message, exn.Message)
