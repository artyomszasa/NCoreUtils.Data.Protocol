namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Immutable
open System.Collections.Generic
open System.Linq.Expressions
open System.Runtime.CompilerServices
open NCoreUtils.Data.Protocol.TypeInference

[<AbstractClass>]
[<AllowNullLiteral>]
type FunctionDescriptor =
  static member Create (name, resultType, argumentTypes, createExpression) =
    ExplicitFunctionDescriptor (name, resultType, argumentTypes, createExpression) :> FunctionDescriptor
  val private name          : string
  val private resultType    : Type
  val private argumentTypes : ImmutableArray<Type>

  member this.Name = this.name

  member this.ResultType = this.resultType

  member this.ArgumentTypes = this.argumentTypes

  new (name, resultType, argumentTypes : ImmutableArray<Type>) =
    if isNull name       then ArgumentNullException "name"       |> raise
    if isNull resultType then ArgumentNullException "resultType" |> raise
    { name          = name
      resultType    = resultType
      argumentTypes = if argumentTypes.IsDefault then ImmutableArray.Empty else argumentTypes }

  abstract CreateExpression : arguments:IReadOnlyList<Expression> -> Expression
  interface IFunctionDescriptor with
    member this.Name = this.name
    member this.ResultType = this.resultType
    member this.ArgumentTypes = this.argumentTypes :> _
    member this.CreateExpression args = this.CreateExpression args

and
  [<Sealed>]
  private ExplicitFunctionDescriptor (name : string, resultType : Type, argumentTypes : ImmutableArray<Type>, createExpression : Func<IReadOnlyList<Expression>, Expression>) =
    inherit FunctionDescriptor (name, resultType, argumentTypes)
    override __.CreateExpression arguments = createExpression.Invoke arguments

type IFunctionDescriptorResolver =
  abstract ResolveFunction : name:string * resultTypeConstraints:TypeVariable * argumentTypeConstraints:IReadOnlyList<TypeVariable> * next:Func<IFunctionDescriptor> -> IFunctionDescriptor

[<AbstractClass; Sealed>]
[<Extension>]
type FunctionDescriptorResolverExtensions private () =

  static let retNull = Func<IFunctionDescriptor> (fun () -> null)

  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member ResolveFunction (resolver : IFunctionDescriptorResolver, name, resultTypeConstraints, argumentTypeConstraints) =
    resolver.ResolveFunction (name, resultTypeConstraints, argumentTypeConstraints, retNull)
