namespace NCoreUtils.Data.Protocol

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions
open NCoreUtils
open NCoreUtils.Data.Protocol.Ast

type IDataQuerySource =
  abstract Query<'T> : filter:string * sortBy:string * isDesc:bool * offset:int * limit:int -> IAsyncEnumerable<'T>

[<AbstractClass>]
type Queryable (provider : IQueryProvider) =
  abstract ElementType : Type
  abstract Expression  : Expression
  abstract GetGenericEnumerator : unit -> IEnumerator
  member val Provider  = provider
  default this.Expression = Expression.Constant this :> _
  interface IOrderedQueryable with
    member this.ElementType = this.ElementType
    member this.Expression  = this.Expression
    member this.Provider    = this.Provider
    member this.GetEnumerator () = this.GetGenericEnumerator ()

and [<AbstractClass>] Queryable<'T> (provider : IQueryProvider) =
  inherit Queryable (provider)
  abstract ExecuteAsync : source : IDataQuerySource -> IAsyncEnumerable<'T>
  abstract ExecuteFirst : source : IDataQuerySource -> Async<'T>
  override __.ElementType = typeof<'T>
  override this.GetGenericEnumerator () = this.GetEnumerator () :> _
  member this.GetEnumerator () = (provider.Execute<seq<'T>> this.Expression).GetEnumerator ()
  interface IOrderedQueryable<'T> with
    member this.GetEnumerator () = this.GetEnumerator ()

and QueryProvider (source : IDataQuerySource) =
  member __.ExecuteAsync<'T> (expression : Expression): IAsyncEnumerable<'T> =
    match expression with
    | :? ConstantExpression as cexpr ->
      match cexpr.Value with
      | :? Queryable<'T> as q -> q.ExecuteAsync source
      | value -> invalidOpf "%A is not a supported query" value
    | _ -> invalidOpf "%A is not a supported query" expression

type DirectQueryable<'T> (provider : IQueryProvider, filter : Node, sortBy : Node, isDesc : bool, offset : int, limit : int) =
  inherit Queryable<'T> (provider)
  member val Filter = filter
  member val SortBy = sortBy
  member val IsDesc = isDesc
  member val Offset = offset
  member val Limit  = limit
  override this.ExecuteAsync source =
    source.Query<'T> (Node.stringify this.Filter, Node.stringify this.SortBy, this.IsDesc, this.Offset, this.Limit)
