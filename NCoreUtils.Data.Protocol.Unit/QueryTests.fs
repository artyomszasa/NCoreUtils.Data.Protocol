module NCoreUtils.Data.Protocol.QueryTests

open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open NCoreUtils.Data
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.Linq
open NCoreUtils.Linq
open Xunit
open Microsoft.Extensions.DependencyInjection

type Entity = {
  I32    : int
  String : string }

type BaseEntity () =
  member val String = Unchecked.defaultof<_> with get, set

type DerivedEntity () =
  inherit BaseEntity ()
  member val I32 = 0 with get, set

type EItem () =
  member val Id = Unchecked.defaultof<int> with get, set
  member val Data = Unchecked.defaultof<string> with get, set

  interface IHasId<int> with
    member this.Id = this.Id

type EDerived () =
  inherit EItem ()

  member val Amount = Unchecked.defaultof<int> with get, set

let private qparser = DataQueryParser ()

[<RequiresExplicitTypeArguments>]
let private empty<'T> () =
  { new IAsyncEnumerable<'T> with
      member __.GetAsyncEnumerator (_) =
        { new IAsyncEnumerator<'T> with
            member __.Current = Unchecked.defaultof<_>
            member __.MoveNextAsync () = ValueTask<bool> false
            member __.DisposeAsync () = ValueTask ()
        }
  }

[<Fact>]
let SimpleSelect () =

  // if not(System.Diagnostics.Debugger.IsAttached) then
  //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  // while not(System.Diagnostics.Debugger.IsAttached) do
  //   System.Threading.Thread.Sleep(100)
  // System.Diagnostics.Debugger.Break()

  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let parser = ExpressionParser matcher
  let f = ref Unchecked.defaultof<_>
  let executor =
    { new IDataQueryExecutor with
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _, _, _) =
          f := filter
          Unchecked.defaultof<IAsyncEnumerable<'T>>
        member __.ExecuteReductionAsync<'TSource, 'TResult> (_, _, filter, _, _, _, _, _) =
          f := filter
          Unchecked.defaultof<Task<'TResult>>
    }
  let provider = QueryProvider (parser, executor)
  let query =
    let q = DirectQuery.Create<Entity> provider :> IQueryable<Entity>
    q.Where (fun e -> e.I32 > 32)
  query.ToListAsync(CancellationToken.None) |> ignore
  Assert.NotNull !f
  Assert.True (Node.eq !f (qparser.ParseQuery "x => x.i32 > 32"))

[<Fact>]
let DerivedSelect () =

  // if not(System.Diagnostics.Debugger.IsAttached) then
  //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  // while not(System.Diagnostics.Debugger.IsAttached) do
  //   System.Threading.Thread.Sleep(100)
  // System.Diagnostics.Debugger.Break()

  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let parser = ExpressionParser matcher
  let f = ref Unchecked.defaultof<_>
  let dt = ref Unchecked.defaultof<_>
  let executor =
    { new IDataQueryExecutor with
        member __.ExecuteEnumerationAsync<'T> (derivedType, filter, _, _, _, _, _, _) =
          f := filter
          dt := derivedType
          Unchecked.defaultof<IAsyncEnumerable<'T>>
        member __.ExecuteReductionAsync<'TSource, 'TResult> (derivedType, _, filter, _, _, _, _, _) =
          f := filter
          dt := derivedType
          Unchecked.defaultof<Task<'TResult>>
    }
  let provider = QueryProvider (parser, executor)
  let query =
    let q = DirectQuery.Create<BaseEntity> provider :> IQueryable<BaseEntity>
    q.OfType<DerivedEntity>().Where (fun e -> e.I32 > 32)
  Assert.IsType<DerivedQuery<BaseEntity, DerivedEntity>>(query) |> ignore
  query.ToListAsync(CancellationToken.None) |> ignore
  Assert.NotNull !f
  Assert.True (Node.eq !f (qparser.ParseQuery "x => x.i32 > 32"))
  Assert.Equal ("derivedentity", !dt)


[<Fact>]
let SimpleAny () =

  // if not(System.Diagnostics.Debugger.IsAttached) then
  //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  // while not(System.Diagnostics.Debugger.IsAttached) do
  //   System.Threading.Thread.Sleep(100)
  // System.Diagnostics.Debugger.Break()

  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let parser = ExpressionParser matcher
  let f = ref Unchecked.defaultof<_>
  let r = ref Unchecked.defaultof<_>
  let executor =
    { new IDataQueryExecutor with
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _, _, _) =
          f := filter
          Unchecked.defaultof<IAsyncEnumerable<'T>>
        member __.ExecuteReductionAsync<'TSource, 'TResult> (_, reduction, filter, _, _, _, _, _) =
          r := reduction
          f := filter
          Task<'T>.FromResult Unchecked.defaultof<'TResult>
    }
  let provider = QueryProvider (parser, executor)
  let query =
    let q = DirectQuery.Create<Entity> provider :> IQueryable<Entity>
    q.Where (fun e -> e.I32 > 32)
  query.AnyAsync(CancellationToken.None) |> ignore
  Assert.NotNull !f
  Assert.True (Node.eq !f (qparser.ParseQuery "x => x.i32 > 32"))
  Assert.NotNull !r
  Assert.Equal ("Any", !r)

[<Fact>]
let CountWithPredicate () =

  // if not(System.Diagnostics.Debugger.IsAttached) then
  //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  // while not(System.Diagnostics.Debugger.IsAttached) do
  //   System.Threading.Thread.Sleep(100)
  // System.Diagnostics.Debugger.Break()

  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let parser = ExpressionParser matcher
  let f = ref Unchecked.defaultof<_>
  let r = ref Unchecked.defaultof<_>
  let executor =
    { new IDataQueryExecutor with
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _, _, _) =
          f := filter
          Unchecked.defaultof<IAsyncEnumerable<'T>>
        member __.ExecuteReductionAsync<'TSource, 'TResult> (_, reduction, filter, _, _, _, _, _) =
          r := reduction
          f := filter
          Task<'T>.FromResult Unchecked.defaultof<'TResult>
    }
  let provider = QueryProvider (parser, executor)
  let query = DirectQuery.Create<Entity> provider :> IQueryable<Entity>
  query.CountAsync((fun e -> e.I32 > 32), CancellationToken.None).Wait ()
  Assert.NotNull !f
  Assert.True (Node.eq !f (qparser.ParseQuery "x => x.i32 > 32"))
  Assert.NotNull !r
  Assert.Equal ("Count", !r)

type private TestQueryExecutor (builder : DataQueryExpressionBuilder, services : System.IServiceProvider) =

  static let rec getTaskResultType (ty : System.Type) =
    match ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<Task<int>> with
    | true -> (ty.GetGenericArguments().[0], ty)
    | _ ->
    match isNull ty || ty = typeof<obj> with
    | true -> failwith "not a task type"
    | _    -> getTaskResultType ty.BaseType

  static member private TaskCast<'T> (source: Task): Task<'T> =
    let (ety, taskTy) = getTaskResultType (source.GetType ())
    match ety = typeof<'T> with
    | true -> source :?> Task<'T>
    | _ ->
      let resultSource = TaskCompletionSource<'T> ()
      source.ContinueWith
        (System.Action<Task>
          (fun t ->
            let res = taskTy.GetProperty("Result", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.Instance)
            res.GetValue (t, null)
            |> unbox
            |> resultSource.SetResult
          )
        ) |> ignore
      resultSource.Task




  member __.ExecuteEnumerationAsync<'T> (target: string, filter, sortBy, isDescending, _: IReadOnlyList<string>, _: IReadOnlyList<string>, offset, limit) =
    let repo = services.GetRequiredService<IDataRepository<'T>>()
    let items =
      match target with
      | null | "" -> repo.Items
      | _ ->
        let dTy = typeof<'T>.Assembly.GetTypes().First(fun ty -> typeof<'T>.IsAssignableFrom(ty) && System.StringComparer.OrdinalIgnoreCase.Equals(ty.Name, target))
        let mOfType = typeof<Queryable>.GetMethod("OfType").MakeGenericMethod(dTy)
        mOfType.Invoke(null, [| box repo.Items |]) :?> IQueryable<_>
    let ordered =
      match isNull (box sortBy) with
      | true -> items :> IQueryable<_>
      | _    ->
        let expr = builder.ResolveExpression (typeof<'T>, sortBy) |> builder.CreateExpression :?> Expressions.Expression<System.Func<'T, int>>
        match isDescending with
        | true -> items.OrderByDescending expr :> IQueryable<_>
        | _    -> items.OrderBy expr :> IQueryable<_>
    let filtered =
      match isNull (box filter) with
      | true -> ordered
      | _ ->
        let expr = builder.ResolveExpression (typeof<'T>, filter) |> builder.CreateExpression :?> Expressions.Expression<System.Func<'T, bool>>
        ordered.Where expr
    let skipped =
      match offset with
      | 0 -> filtered
      | i -> filtered.Skip i
    let final =
      match limit with
      | -1 -> skipped
      | i  -> skipped.Take i
    QueryableExtensions.ExecuteAsync(final, CancellationToken.None)
  interface IDataQueryExecutor with
    member this.ExecuteEnumerationAsync<'T> (target, filter, sortBy, isDescending, fields, includes, offset, limit) =
      this.ExecuteEnumerationAsync<'T>(target, filter, sortBy, isDescending, fields, includes, offset, limit)
    member this.ExecuteReductionAsync<'TSource, 'TResult> (target, reduction, filter, sortBy, isDescending, offset, limit, ctoken) =
      match reduction with
      | "Single" ->
        this.ExecuteEnumerationAsync<'TSource>(target, filter, sortBy, isDescending, Unchecked.defaultof<_>, Unchecked.defaultof<_>, offset, limit).SingleAsync(ctoken).AsTask()
        |> TestQueryExecutor.TaskCast<'TResult>
      | "SingleOrDefault" ->
        this.ExecuteEnumerationAsync<'TSource>(target, filter, sortBy, isDescending, Unchecked.defaultof<_>, Unchecked.defaultof<_>, offset, limit).SingleOrDefaultAsync(ctoken).AsTask()
        |> TestQueryExecutor.TaskCast<'TResult>
      | _ -> System.NotImplementedException (sprintf "reduction not implemented: %s" reduction) |> raise<Task<'TResult>>

let private createTestServiceProvider () =
  // match and parse LINQ expressions
  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let eparser = ExpressionParser matcher
  // build expressions
  let builder =
    let cirBuilder =
      FunctionDescriptorResolverBuilder()
        .Add<CommonFunctions.StringLength>()
    let services =
      ServiceCollection()
        .AddSingleton<CommonFunctions.StringLength>()
        .BuildServiceProvider()
    let resolver = cirBuilder.Build services
    let parser = DataQueryParser ()
    let inferrer = TypeInference.TypeInferrer resolver
    let logger =
      { new Microsoft.Extensions.Logging.ILogger<DataQueryExpressionBuilder> with
          member __.BeginScope (_state: 'TState) =
            { new System.IDisposable with member __.Dispose () = () }
          member __.IsEnabled(_logLevel: Microsoft.Extensions.Logging.LogLevel) =
            false
          member __.Log<'TState>(_: Microsoft.Extensions.Logging.LogLevel, _: Microsoft.Extensions.Logging.EventId, _: 'TState, _: exn, _: System.Func<'TState,exn,string>) =
            ()
      }
    DataQueryExpressionBuilder(parser, inferrer, logger)
  let services = ServiceCollection ()
  let data =
    [|
      EItem (Id = 1, Data = "xxx")
      EItem (Id = 2, Data = "yxx")
      EItem (Id = 3, Data = "xyx")
      EDerived (Id = 4, Data = "xxy", Amount = 7) :> _
    |]
  services
    .AddSingleton(eparser)
    .AddSingleton<IDataQueryExecutor, TestQueryExecutor>()
    .AddSingleton<QueryProvider>()
    .AddSingleton(builder)
    .AddInMemoryDataRepositoryContext()
    .AddInMemoryDataRepository<EItem, int>(ResizeArray data)
    .BuildServiceProvider(true)

[<Fact>]
let ``simple query`` () =
  // if not(System.Diagnostics.Debugger.IsAttached) then
  //   printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  // while not(System.Diagnostics.Debugger.IsAttached) do
  //   System.Threading.Thread.Sleep(100)
  // System.Diagnostics.Debugger.Break()

  use services = createTestServiceProvider ()
  let queryProvider = services.GetRequiredService<QueryProvider>()
  let query = DirectQuery.Create<EItem>(queryProvider)
  let one = (query.Where(fun e -> e.Id = 1).ToArrayAsync CancellationToken.None).Result
  Assert.Equal(1, (Assert.Single one).Id)
  let oneId = (query.Where(fun e -> e.Id = 1).Select(fun e -> e.Id).ToArrayAsync CancellationToken.None).Result
  Assert.Equal(1, Assert.Single oneId)
  let descIds = (query.OrderByDescending(fun e -> e.Id).Select(fun e -> e.Id).ToArrayAsync CancellationToken.None).Result
  Assert.True (descIds.SequenceEqual([ 4; 3; 2; 1]))
  // offset/limit
  Assert.Equal (
    query
      .OrderByDescending(fun e -> e.Id)
      .Select(fun e -> e.Id)
      .Skip(1)
      .Take(2)
      .ToArrayAsync(CancellationToken.None)
      .Result,
    [ 3; 2 ]
  )
  Assert.Null (query.SingleOrDefaultAsync((fun e -> e.Id > 10), CancellationToken.None).Result)
  Assert.Null (query.SingleOrDefault((fun e -> e.Id > 10)))
  let d = Assert.Single (query.OfType<EDerived>().ToArrayAsync(CancellationToken.None).Result)
  Assert.Equal (7, d.Amount)
  Assert.Empty (query.OfType<EDerived>().Skip(1).Take(2).ToArrayAsync(CancellationToken.None).Result)
  Assert.Equal (7, query.OfType<EDerived>().SingleAsync(CancellationToken.None).Result.Amount)