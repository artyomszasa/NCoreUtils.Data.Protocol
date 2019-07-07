module NCoreUtils.Data.Protocol.QueryTests

open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open NCoreUtils.Data.Protocol.Ast
open NCoreUtils.Data.Protocol.Linq
open NCoreUtils.Linq
open Xunit

type Entity = {
  I32    : int
  String : string }

type BaseEntity () =
  member val String = Unchecked.defaultof<_> with get, set

type DerivedEntity () =
  inherit BaseEntity ()
  member val I32 = 0 with get, set

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
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _) =
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

  if not(System.Diagnostics.Debugger.IsAttached) then
    printfn "Please attach a debugger, PID: %d" (System.Diagnostics.Process.GetCurrentProcess().Id)
  while not(System.Diagnostics.Debugger.IsAttached) do
    System.Threading.Thread.Sleep(100)
  System.Diagnostics.Debugger.Break()

  let matcher = CommonFunctions.StringLength () :> IFunctionMatcher
  let parser = ExpressionParser matcher
  let f = ref Unchecked.defaultof<_>
  let dt = ref Unchecked.defaultof<_>
  let executor =
    { new IDataQueryExecutor with
        member __.ExecuteEnumerationAsync<'T> (derivedType, filter, _, _, _, _) =
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
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _) =
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
        member __.ExecuteEnumerationAsync<'T> (_, filter, _, _, _, _) =
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
