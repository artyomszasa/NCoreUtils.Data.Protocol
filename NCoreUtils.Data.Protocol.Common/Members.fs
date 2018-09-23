namespace NCoreUtils.Data.Protocol

open System.Reflection

[<Struct>]
[<StructuralEquality; NoComparison>]
type Members =
  | PropertyMember of Property:PropertyInfo
  | FieldMember    of Field:FieldInfo
  | NoMember

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Members =

  let getMember (name : string) (ty : System.Type) =
    match ty.GetMember (name, MemberTypes.Property ||| MemberTypes.Field, BindingFlags.IgnoreCase ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance) with
    | [||] -> NoMember
    | xs ->
      match xs |> Array.tryPick (function | :? PropertyInfo as p -> Some p | _ -> None) with
      | Some p -> PropertyMember p
      | _ ->
      match xs |> Array.tryPick (function | :? FieldInfo as p -> Some p | _ -> None) with
      | Some f -> FieldMember f
      | _      -> NoMember
