namespace NCoreUtils.Data.Protocol.TypeInference

open System
open System.Collections.Immutable

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal UnresolvedNode =

  [<CompiledName("Resolve")>]
  let rec resolve (resolver : _ -> Type) (node : UnresolvedNode<IFunctionDescriptor>) =
    match node with
    | UnresolvedLambda     (uid, arg, body)       -> ResolvedLambda     (resolver uid, resolve resolver arg, resolve resolver body)
    | UnresolvedBinary     (uid, left, op, right) -> ResolvedBinary     (resolver uid, resolve resolver left, op, resolve resolver right)
    | UnresolvedCall       (uid, name, args)      -> ResolvedCall       (resolver uid, name, args |> Seq.map (resolve resolver) |> ImmutableArray.CreateRange)
    | UnresolvedMember     (uid, instance, name)  -> ResolvedMember     (resolver uid, resolve resolver instance, name)
    | UnresolvedConstant   (uid, value)           -> ResolvedConstant   (resolver uid, value)
    | UnresolvedIdentifier (uid, name)            -> ResolvedIdentifier (resolver uid, name)
