namespace NCoreUtils.Data.Protocol

open NCoreUtils.Data
open System.Collections.Immutable
open Microsoft.Extensions.Logging

type CallInfoResolverBuilder () =
  let infos = ResizeArray ()
  member internal __.Infos = infos
  member this.Add (info : ICallInfo) =
    infos.Add info
    this
  member this.AddRange info =
    infos.AddRange info
    this
  member this.Add (name, resultType, parameters : ImmutableArray<_>, createExpression) =
    CallInfo.Create(name, resultType, parameters, createExpression)
    |> this.Add
  member this.Add (name, resultType, parameters : _[], createExpression) =
    CallInfo.Create(name, resultType, parameters, createExpression)
    |> this.Add
  member this.Add (name, resultType, parameters : seq<_>, createExpression) =
    CallInfo.Create(name, resultType, parameters, createExpression)
    |> this.Add

type CallInfoResolver (builder : CallInfoResolverBuilder, logger : ILogger<CallInfoResolver>) =
  let infos =
    builder.Infos
    |> Seq.groupBy (fun info -> info.Name)
    |> Seq.fold
      (fun map (name, infos) ->
        let sub =
          infos
          |> Seq.fold
            (fun map info ->
              let l = info.Parameters.Length
              match Map.containsKey l map with
              | true ->
                logger.LogWarning ("Data query function \"{0}\" with parameter count {1} has already been defined.", name, l)
                map
              | _ -> Map.add info.Parameters.Length info map)
            Map.empty
        Map.add name sub map)
      Map.empty
  interface ICallInfoResolver with
    member __.ResolveCall (name, argNum) =
      match infos |> Map.tryFind name |> Option.bind (Map.tryFind argNum) with
      | Some info ->
        logger.LogTrace ("Successfully resolved function call \"{0}\" with parameter count {1}.", name, argNum)
        info
      | _ ->
        logger.LogTrace ("Failed to resolve function call \"{0}\" with parameter count {1}.", name, argNum)
        null