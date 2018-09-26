namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Immutable
open System.Collections.Generic
open System.Linq.Expressions
open System.Runtime.CompilerServices
open NCoreUtils.Data.Protocol.TypeInference

/// Provides base class for implementing function descriptors.
[<AbstractClass>]
[<AllowNullLiteral>]
type FunctionDescriptor =
  /// <summary>
  /// Creates function descriptor from the specified arguments.
  /// </summary>
  /// <param name="name">Function name.</param>
  /// <param name="resultType">Function result type.</param>
  /// <param name="argumentTypes">Function argument types.</param>
  /// <param name="createExpression">Function used to create expression that represents function invocation.</param>
  static member Create (name, resultType, argumentTypes, createExpression) =
    ExplicitFunctionDescriptor (name, resultType, argumentTypes, createExpression) :> FunctionDescriptor
  val private name          : string
  val private resultType    : Type
  val private argumentTypes : ImmutableArray<Type>

  /// Gets function name.
  member this.Name = this.name
  /// Gets function result type.
  member this.ResultType = this.resultType
  /// Gets function argument types.
  member this.ArgumentTypes = this.argumentTypes
  /// <summary>
  /// Initializes new instance of function descriptor from the specified arguments.
  /// </summary>
  /// <param name="name">Function name.</param>
  /// <param name="resultType">Function result type.</param>
  /// <param name="argumentTypes">Function argument types.</param>
  new (name, resultType, argumentTypes : ImmutableArray<Type>) =
    if isNull name       then ArgumentNullException "name"       |> raise
    if isNull resultType then ArgumentNullException "resultType" |> raise
    { name          = name
      resultType    = resultType
      argumentTypes = if argumentTypes.IsDefault then ImmutableArray.Empty else argumentTypes }
  /// <summary>
  /// Creates expression that represents function invocation defined by the actual descriptor instance.
  /// </summary>
  /// <param name="arguments">Function arguments.</param>
  /// <returns>Expression that represents function invocation.</returns>
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

/// Defines functionality to resolve function invocations in data queries.
type IFunctionDescriptorResolver =
  /// <summary>
  /// Attempts to resolve function invocation from the specified arguments.
  /// </summary>
  /// <param name="name">Function name.</param>
  /// <param name="resultTypeConstraints">Result type constraints.</param>
  /// <param name="argumentTypeConstraints">Argument type constraints.</param>
  /// <param name="next">Function that calls next resolver in function resolvation chain.</param>
  /// <returns>Descriptor of the function that should handle the specified call.</returns>
  abstract ResolveFunction : name:string * resultTypeConstraints:TypeVariable * argumentTypeConstraints:IReadOnlyList<TypeVariable> * next:Func<IFunctionDescriptor> -> IFunctionDescriptor

/// Defines extensions methods for function resolvation.
[<AbstractClass; Sealed>]
[<Extension>]
type FunctionDescriptorResolverExtensions private () =

  static let retNull = Func<IFunctionDescriptor> (fun () -> null)

  /// <summary>
  /// Attempts to resolve function invocation from the specified arguments.
  /// </summary>
  /// <param name="resolver">Resolver to use.</param>
  /// <param name="name">Function name.</param>
  /// <param name="resultTypeConstraints">Result type constraints.</param>
  /// <param name="argumentTypeConstraints">Argument type constraints.</param>
  /// <returns>Descriptor of the function that should handle the specified call or <c>null</c>.</returns>
  [<Extension>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member ResolveFunction (resolver : IFunctionDescriptorResolver, name, resultTypeConstraints, argumentTypeConstraints) =
    resolver.ResolveFunction (name, resultTypeConstraints, argumentTypeConstraints, retNull)
