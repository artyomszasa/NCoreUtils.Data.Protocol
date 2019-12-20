namespace NCoreUtils.Data.Protocol

open System
open System.Collections.Generic
open System.Linq.Expressions
open NCoreUtils

type CompositeFunctionMatcherBuilder () =
  let matchers = ResizeArray ()
  member internal __.MatcherTypes = matchers
  member this.Add (matcherType: Type) =
    matchers.Add matcherType
    this
  member this.Add<'TMatcher when 'TMatcher :> IFunctionMatcher> () =
    this.Add typeof<'TMatcher>
  member this.AddRange matcherTypes =
    matchers.AddRange matcherTypes
    this
  member this.AddRange ([<ParamArray>] matcherTypes: Type[]) =
    this.AddRange (matcherTypes :> seq<Type>)

and CompositeFunctionMatcher (serviceProvider: IServiceProvider, builder: CompositeFunctionMatcherBuilder) =
  static let noRes = Func<ValueOption<FunctionMatch>> (fun () -> ValueNone)
  let matcherTypes = builder.MatcherTypes.ToArray ()
  member __.MatcherTypes = matcherTypes :> IReadOnlyList<_>
  member __.MatchFunction (expression : Expression, next : Func<ValueOption<FunctionMatch>>): ValueOption<FunctionMatch> =
    let rec findMatch index =
      match index = matcherTypes.Length with
      | true -> next.Invoke ()
      | _ ->
        let matcherType = matcherTypes.[index]
        use instance = serviceProvider.GetOrActivateService matcherType
        match (instance.BoxedService :?> IFunctionMatcher).MatchFunction (expression, noRes) with
        | ValueSome _ as res -> res
        | _                  -> findMatch (index + 1)
    findMatch 0
  interface IFunctionMatcher with
    member this.MatchFunction (expression, next) = this.MatchFunction (expression, next)



