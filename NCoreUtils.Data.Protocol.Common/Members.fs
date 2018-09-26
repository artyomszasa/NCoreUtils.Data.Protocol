namespace NCoreUtils.Data.Protocol

open System.Reflection

/// Represents member lookup result.
[<Struct>]
[<StructuralEquality; NoComparison>]
type Members =
  /// Represents property match.
  | PropertyMember of Property:PropertyInfo
  /// Represents field match.
  | FieldMember    of Field:FieldInfo
  /// Represents failed match.
  | NoMember

/// Contains member lookup helpers.
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Members =

  /// <summary>
  /// Perform member lookup using the specified member name on the specified type.
  /// </summary
  /// <param name="name">Name of the member to search for.</param>
  /// <param name="ty">Source type.</param>
  /// <returns>Member lookup result.</returns>
  [<CompiledName("GetMember")>]
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
