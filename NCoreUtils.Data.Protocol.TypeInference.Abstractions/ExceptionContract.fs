namespace NCoreUtils.Data

open System
open System.Runtime.Serialization

/// Represents type constraint mismatch detials.
[<Struct>]
[<StructuralEquality; NoComparison>]
type TypeConstriantMismatchReason =
  /// Examined type is missing some constrainted attribute.
  | MissingMember of MemberName:string
  /// Examined type is missing some constrainted interface.
  | MissingInterfaceImplmentation of Interface:Type
  /// Examined type is incompatible with the constainted base type.
  | IncompatibleType of BaseType:Type
  /// Examined type is not numeric but constrainted to be so.
  | NumericConstraint
  /// Examined type is numeric but constrainted not to be so.
  | NonNumericConstraint
  /// Examined type is not nullable but constrainted to be so.
  | NullableConstraint
  /// Examined type is nullable but constrainted not to be so.
  | NonNullableConstraint

/// Represents type constraint mismatch.
[<Struct>]
[<StructuralEquality; NoComparison>]
type TypeConstriantMismatch = {
  /// Gets examined type.
  TargetType : Type
  /// Gets constraint mismatch details.
  Reason     : TypeConstriantMismatchReason }
  with
    /// Provides user friendly message about the mismatch.
    override this.ToString () =
      match this.Reason with
      | MissingMember name -> sprintf "Type %A has no member %s" this.TargetType name
      | MissingInterfaceImplmentation iface -> sprintf "Type %A does not implment %A" this.TargetType iface
      | IncompatibleType baseType -> sprintf "Type %A is not compatible with constrainted base type %A" this.TargetType baseType
      | NumericConstraint                   -> sprintf "Type %A has been constrinted to be numeric." this.TargetType
      | NonNumericConstraint                -> sprintf "Type %A has been constrinted to be non-numeric." this.TargetType
      | NullableConstraint                  -> sprintf "Type %A has been constrinted to be nullable." this.TargetType
      | NonNullableConstraint               -> sprintf "Type %A has been constrinted to be non-nullable." this.TargetType

/// Represents errors that occur when raw data query is semantically invalid.
[<Serializable>]
type ProtocolTypeInferenceException =
  inherit ProtocolException
  new (message) = { inherit ProtocolException (message) }
  new (message : string, innerException) = { inherit ProtocolException (message, innerException) }
  new (info : SerializationInfo, context) = { inherit ProtocolException (info, context) }

[<AutoOpen>]
module private ProtocolTypeConstraintMismatchExceptionKeys =

  [<Literal>]
  let KeyDetails = "TypeConstrintMismatchDetails"

/// Represents errors that occur when constraint mismatch error has occured while processing raw data query.
[<Serializable>]
type ProtocolTypeConstraintMismatchException =
  inherit ProtocolTypeInferenceException
  val details : TypeConstriantMismatch
  /// Gets details about the mismatch.
  member this.Details = this.details
  new (details : TypeConstriantMismatch) = ProtocolTypeConstraintMismatchException (details, Unchecked.defaultof<string>)
  new (details, innerException : exn) = ProtocolTypeConstraintMismatchException (details, null, innerException)
  new (details, message : string) =
    let msg =
      match message with
      | null -> details.ToString ()
      | _    -> message
    { inherit ProtocolTypeInferenceException (msg)
      details = details }
  new (details, message : string, innerException) =
    let msg =
      match message with
      | null -> details.ToString ()
      | _    -> message
    { inherit ProtocolTypeInferenceException (msg, innerException)
      details = details }
  new (info : SerializationInfo, context) =
    { inherit ProtocolTypeInferenceException (info, context)
      details = info.GetValue (KeyDetails, typeof<TypeConstriantMismatch>) :?> TypeConstriantMismatch }
  override this.GetObjectData (info, context) =
    base.GetObjectData (info, context)
    info.AddValue (KeyDetails, this.details, typeof<TypeConstriantMismatch>)
