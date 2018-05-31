namespace NCoreUtils.Data

open System.Runtime.CompilerServices
open System
open System.Collections.Immutable
open System.Linq.Expressions

[<Extension>]
type DataQueryExpressionBuilderExtensions =

  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member BuildExpression<'root> (builder : IDataQueryExpressionBuilder, input : string) =
    builder.BuildExpression (typeof<'root>, input)

type CallInfo =
  private {
    Name             : string
    ResultType       : Type
    Parameters       : ImmutableArray<Type>
    CreateExpression : Func<Expression[], Expression> }
  with
    interface ICallInfo with
      member this.Name       = this.Name
      member this.ResultType = this.ResultType
      member this.Parameters = this.Parameters
      member this.CreateExpression arguments = this.CreateExpression.Invoke arguments
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Create (name, resultType, parameters : ImmutableArray<Type>, createExpression) =
      { Name             = name
        ResultType       = resultType
        Parameters       = parameters
        CreateExpression = createExpression }
      :> ICallInfo
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Create (name, resultType, parameters : Type[], createExpression) =
      CallInfo.Create (name, resultType, ImmutableArray.CreateRange parameters, createExpression)
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Create (name, resultType, parameters : seq<Type>, createExpression) =
      CallInfo.Create (name, resultType, ImmutableArray.CreateRange parameters, createExpression)